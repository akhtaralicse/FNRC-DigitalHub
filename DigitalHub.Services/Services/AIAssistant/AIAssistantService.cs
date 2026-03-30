using DigitalHub.Domain.DBContext;
using DigitalHub.Domain.Domains;
using DigitalHub.Services.DTO.AIAssistant;
using DigitalHub.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Text;
using UglyToad.PdfPig;
using DocumentFormat.OpenXml.Packaging; 
using Newtonsoft.Json; 

namespace DigitalHub.Services.Services.AIAssistant
{
    public class AIAssistantService : IAIAssistantService
    {
        private readonly DigitalHubDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public AIAssistantService(DigitalHubDBContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _httpClient = new HttpClient();
        }

        public async Task<bool> UploadDocuments(List<IFormFile> files, string uploaderName)
        {
            foreach (var file in files)
            {
                var content = await ExtractText(file);
                if (!string.IsNullOrEmpty(content))
                {
                    var doc = new AIAssistantDocument
                    {
                        FileName = file.FileName,
                        Content = content,
                        Language = "Mixed",
                        UploaderName = uploaderName,
                        FileSize = file.Length,
                        CreatedDate = DateTime.Now
                    };
                    _context.AIAssistantDocuments.Add(doc);
                }
            }
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<string> ExtractText(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName).ToLower();
            using var stream = file.OpenReadStream();

            try {
                if (extension == ".txt")
                {
                    using var reader = new StreamReader(stream);
                    return await reader.ReadToEndAsync();
                }
                else if (extension == ".pdf")
                {
                    using var pdf = PdfDocument.Open(stream);
                    var sb = new StringBuilder();
                    foreach (var page in pdf.GetPages())
                    {
                        sb.AppendLine(page.Text);
                    }
                    return sb.ToString();
                }
                else if (extension == ".docx")
                {
                    using var wordDoc = WordprocessingDocument.Open(stream, false);
                    var body = wordDoc.MainDocumentPart.Document.Body;
                    return body.InnerText;
                }
            } catch (Exception ex) {
                // Log error
            }

            return string.Empty;
        }

        public async Task<ChatResponseDTO> Chat(string query, string lang, string sessionId, string fullName)
        {
            // 1. Get Session History
            var history = await _context.AIChatLogs
                .Where(l => l.SessionId == sessionId || (fullName != "Anonymous" && l.FullName == fullName))
                .OrderByDescending(l => l.Timestamp)
                .Take(5)
                .OrderBy(l => l.Timestamp)
                .ToListAsync();

            // 2. Keyword-Based RAG (Simpler & More Accurate for specialized FNRC documents)
            // This implementation uses SQL 'LIKE' behavior which is sufficient for your service data.
            var relevantDocs = await _context.AIAssistantDocuments
                .Where(d => d.Content.Contains(query) || (lang == "AR" && d.Content.Contains(query)))
                .Take(5)
                .ToListAsync();

            if (!relevantDocs.Any()) {
                // Return most recent operational context if no direct match
                relevantDocs = await _context.AIAssistantDocuments.OrderByDescending(d => d.Id).Take(1).ToListAsync();
            }

            var context = string.Join("\n\n", relevantDocs.Select(d => d.Content));
            
            // 3. Call Azure OpenAI
            var rawResponse = await CallAzureOpenAI(query, context, history, lang);

            // 4. Parse JSON Response
            string responseText = "Sorry, I couldn't process this request.";
            List<string> questions = new List<string>();

            try {
                 int start = rawResponse.IndexOf("{");
                 int end = rawResponse.LastIndexOf("}");
                 if (start >= 0 && end >= 0) {
                     var jsonPart = rawResponse.Substring(start, end - start + 1);
                     dynamic result = JsonConvert.DeserializeObject(jsonPart);
                     responseText = result.answer;
                     if (result.questions != null) {
                         foreach(var q in result.questions) questions.Add(q.ToString());
                     }
                 } else {
                     responseText = rawResponse;
                 }
            } catch {
                responseText = rawResponse;
            }

            // 5. Log Analysis & History
            _context.AIChatLogs.Add(new AIChatLog {
                SessionId = sessionId,
                FullName = fullName,
                UserQuery = query,
                BotResponse = responseText,
                Timestamp = DateTime.Now,
                Language = lang
            });
            await _context.SaveChangesAsync();

            return new ChatResponseDTO
            {
                ResponseText = responseText,
                Sources = relevantDocs.Select(d => d.FileName).Distinct().ToList(),
                RecommendedQuestions = questions
            };
        }

        private async Task<string> CallAzureOpenAI(string query, string context, List<AIChatLog> history, string lang)
        {
            var endpoint = _configuration["AzureOpenAI:Endpoint"];
            var apiKey = _configuration["AzureOpenAI:ApiKey"];
            var deployment = _configuration["AzureOpenAI:DeploymentName"];

            if (string.IsNullOrEmpty(apiKey)) return "Azure OpenAI configuration is missing.";

            var systemPrompt = $@"
            You are a strict and helpful AI assistant for the Fujairah Natural Resources Corporation (FNRC).
            MISSION: Help users with FNRC services, departments, technical guidance, and system usage.
            RULE: Answer ONLY using the context. If not found, say you only help with FNRC data.

            FORMAT: Respond strictly in JSON:
            {{
              ""answer"": ""your localized answer"",
              ""questions"": [""follow-up 1"", ""follow-up 2"", ""follow-up 3""]
            }}

            Language: {(lang == "AR" ? "Arabic" : "English")}.
            Context: {context}";

            var messages = new List<object> { new { role = "system", content = systemPrompt } };
            
            foreach (var h in history) {
                messages.Add(new { role = "user", content = h.UserQuery });
                messages.Add(new { role = "assistant", content = h.BotResponse });
            }
            messages.Add(new { role = "user", content = query });

            var requestBody = new { messages = messages, max_tokens = 1500, temperature = 0.3 };
            var json = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("api-key", apiKey);

            try {
                var url = $"{endpoint.TrimEnd('/')}/openai/deployments/{deployment}/chat/completions?api-version=2024-02-15-preview";
                var response = await _httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    dynamic result = JsonConvert.DeserializeObject(responseString);
                    return result.choices[0].message.content;
                }
                return "Error calling Azure OpenAI.";
            } catch (Exception ex) {
                return "Azure AI error: " + ex.Message;
            }
        }
    }
}
