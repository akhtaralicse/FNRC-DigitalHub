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

        public async Task<List<AIAssistantDocumentDTO>> GetDocuments(string search)
        {
            var query = _context.AIAssistantDocuments.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(x => x.FileName.Contains(search) || x.Content.Contains(search));
            }

            return await query.Select(x => new AIAssistantDocumentDTO
            {
                Id = x.Id,
                FileName = x.FileName,
                UploaderName = x.UploaderName,
                FileSize = x.FileSize,
                CreatedDate = x.CreatedDate,
                Category = x.Category,
                ExpiryDate = x.ExpiryDate,
                IsActive = x.IsActive
            })
            .OrderByDescending(x => x.CreatedDate)
            .ToListAsync();
        }

        public async Task<bool> DeleteDocument(int id)
        {
            var doc = await _context.AIAssistantDocuments.FindAsync(id);
            if (doc != null)
            {
                _context.AIAssistantDocuments.Remove(doc);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> ToggleDocumentStatus(int id)
        {
            var doc = await _context.AIAssistantDocuments.FindAsync(id);
            if (doc != null)
            {
                doc.IsActive = !doc.IsActive;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public AIAssistantService(DigitalHubDBContext context, IConfiguration configuration)

        {
            _context = context;
            _configuration = configuration;

            // Bypass proxy for local Ollama calls and increase timeout for heavy LLM processing
            var handler = new HttpClientHandler { UseProxy = false };
            _httpClient = new HttpClient(handler) { Timeout = TimeSpan.FromMinutes(5) };
        }


        public async Task<bool> UploadDocuments(List<IFormFile> files, string uploaderName, string language = "Mixed", string category = "General", DateTime? expiryDate = null)
        {
            foreach (var file in files)
            {
                var content = await ExtractText(file);
                if (!string.IsNullOrEmpty(content))
                {
                    var shortContext = content.Length > 2000 ? content[..2000] : content;
                    
                    var genQuestions = new Dictionary<string, List<string>>();
                    if (language.Equals("AR", StringComparison.CurrentCultureIgnoreCase) || language.Equals("MIXED", StringComparison.CurrentCultureIgnoreCase))
                    {
                        var promptAr = $"Read this document excerpt and generate 4 common, short questions a user might ask about it. Respond ONLY with a JSON array of 4 strings in Arabic. IMPORTANT: Do NOT mention document names, filenames, or file extensions. Excerpt: {shortContext}";
                        genQuestions["AR"] = await GenerateQuestionsFromPrompt(promptAr, "AR");
                    }
                    if (language.Equals("EN", StringComparison.CurrentCultureIgnoreCase) || language.Equals("MIXED", StringComparison.CurrentCultureIgnoreCase))
                    {
                        var promptEn = $"Read this document excerpt and generate 4 common, short questions a user might ask about it. Respond ONLY with a JSON array of 4 strings in English. IMPORTANT: Do NOT mention document names, filenames, or file extensions. Excerpt: {shortContext}";
                        genQuestions["EN"] = await GenerateQuestionsFromPrompt(promptEn, "EN");
                    }

                    var doc = new AIAssistantDocument
                    {
                        FileName = file.FileName,
                        Content = content,
                        Language = language,
                        UploaderName = uploaderName,
                        FileSize = file.Length,
                        CreatedDate = DateTime.Now,
                        GeneratedQuestions = JsonConvert.SerializeObject(genQuestions),
                        Category = category,
                        ExpiryDate = expiryDate,
                        IsActive = true
                    };
                    _context.AIAssistantDocuments.Add(doc);
                }
            }
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RegenerateQuestions(int id)
        {
            var doc = await _context.AIAssistantDocuments.FindAsync(id);
            if (doc == null) return false;

            var shortContext = doc.Content.Length > 2000 ? doc.Content.Substring(0, 2000) : doc.Content;
            var genQuestions = new Dictionary<string, List<string>>();
            
            if (doc.Language.Equals("AR", StringComparison.CurrentCultureIgnoreCase) || doc.Language.Equals("MIXED", StringComparison.CurrentCultureIgnoreCase))
            {
                var promptAr = $"Read this document excerpt and generate 4 common, short questions a user might ask about it. Respond ONLY with a JSON array of 4 strings in Arabic. IMPORTANT: Do NOT mention document names, filenames, or file extensions. Excerpt: {shortContext}";
                genQuestions["AR"] = await GenerateQuestionsFromPrompt(promptAr, "AR");
            }
            if (doc.Language.Equals("EN", StringComparison.CurrentCultureIgnoreCase) || doc.Language.Equals("MIXED", StringComparison.CurrentCultureIgnoreCase))
            {
                var promptEn = $"Read this document excerpt and generate 4 common, short questions a user might ask about it. Respond ONLY with a JSON array of 4 strings in English. IMPORTANT: Do NOT mention document names, filenames, or file extensions. Excerpt: {shortContext}";
                genQuestions["EN"] = await GenerateQuestionsFromPrompt(promptEn, "EN");
            }

            doc.GeneratedQuestions = JsonConvert.SerializeObject(genQuestions);
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<List<string>> GenerateQuestionsFromPrompt(string prompt, string lang)
        {
            string rawQuestions = "[]";
            try
            {
                var activeLLM = _configuration["AIConfig:ActiveLLM"]?.ToUpper();
                if (activeLLM == "AZUREOPENAI" || activeLLM == "AZURE")
                    rawQuestions = await CallAzureOpenAI(prompt, "Generate 4 FAQ questions based on document text.", new List<AIChatLog>(), lang);
                else if (activeLLM == "GEMINI")
                    rawQuestions = await CallGemini(prompt, "Generate 4 FAQ questions based on document text.", new List<AIChatLog>(), lang);
                else 
                    rawQuestions = await CallOllama(prompt, "Generate 4 FAQ questions based on document text.", new List<AIChatLog>(), lang);
                
                int start = rawQuestions.IndexOf("[");
                int end = rawQuestions.LastIndexOf("]");
                if (start >= 0 && end >= 0)
                {
                    rawQuestions = rawQuestions.Substring(start, end - start + 1);
                    var list = JsonConvert.DeserializeObject<List<string>>(rawQuestions);
                    if (list != null) return list;
                }
            }
            catch {}
            return [];
        }

        private async Task<string> ExtractText(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName).ToLower();
            using var stream = file.OpenReadStream();

            try
            {
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
            }
            catch (Exception ex)
            {
                // Log error
            }

            return string.Empty;
        }

        public async Task<ChatResponseDTO> Chat(string query, string lang, string sessionId, string fullName)
        {
            // 0. Check Manual QA Overrides
            var overrideMatch = await _context.AIQAOverrides
                .Where(o => o.IsActive && query.Contains(o.Question))
                .FirstOrDefaultAsync();

            if (overrideMatch != null)
            {
                var oLog = new AIChatLog
                {
                    SessionId = sessionId,
                    FullName = fullName,
                    UserQuery = query,
                    BotResponse = overrideMatch.Answer,
                    Timestamp = DateTime.Now,
                    Language = lang
                };
                _context.AIChatLogs.Add(oLog);
                await _context.SaveChangesAsync();
                
                return new ChatResponseDTO
                {
                    LogId = oLog.Id,
                    ResponseText = overrideMatch.Answer,
                    Sources = new List<string> { "System Override" },
                    RecommendedQuestions = new List<string>()
                };
            }

            // 1. Get Session History
            var history = await _context.AIChatLogs
                .Where(l => l.SessionId == sessionId || (fullName != "Anonymous" && l.FullName == fullName))
                .OrderByDescending(l => l.Timestamp)
                .Take(5)
                .OrderBy(l => l.Timestamp)
                .ToListAsync();

            // 2. Keyword-Based RAG (Simpler & More Accurate for specialized FNRC documents)
            // This implementation uses SQL 'LIKE' behavior which is sufficient for your service data.
            var now = DateTime.Now;
            var relevantDocs = await _context.AIAssistantDocuments
                .Where(d => d.IsActive && (d.ExpiryDate == null || d.ExpiryDate > now))
                .Where(d => d.Content.Contains(query) || (lang == "AR" && d.Content.Contains(query)))
                .Take(5)
                .ToListAsync();

            if (relevantDocs.Count == 0)
            {
                // Return most recent operational context if no direct match
                relevantDocs = await _context.AIAssistantDocuments
                    .Where(d => d.IsActive && (d.ExpiryDate == null || d.ExpiryDate > now))
                    .OrderByDescending(d => d.Id)
                    .Take(1)
                    .ToListAsync();
            }

            var context = string.Join("\n\n", relevantDocs.Select(d => d.Content));
            if (context.Length > 12000) context = context.Substring(0, 12000) + "... [truncated]";


            // 3. Conditional Call: Azure OpenAI, Gemini or Local Ollama
            string rawResponse;
            var activeLLM = _configuration["AIConfig:ActiveLLM"]?.ToUpper();
            if (activeLLM == "AZUREOPENAI" || activeLLM == "AZURE")
            {
                rawResponse = await CallAzureOpenAI(query, context, history, lang);
            }
            else if (activeLLM == "GEMINI")
            {
                rawResponse = await CallGemini(query, context, history, lang);
            }
            else
            {
                rawResponse = await CallOllama(query, context, history, lang);
            }

            // 4. Parse JSON Response
            string responseText = "Sorry, I couldn't process this request.";
            List<string> questions = new();

            try
            {
                int start = rawResponse.IndexOf("{");
                int end = rawResponse.LastIndexOf("}");
                if (start >= 0 && end >= 0)
                {
                    var jsonPart = rawResponse.Substring(start, end - start + 1);
                    dynamic result = JsonConvert.DeserializeObject(jsonPart);
                    responseText = result.answer;
                    if (result.questions != null)
                    {
                        foreach (var q in result.questions) questions.Add(q.ToString());
                    }
                }
                else
                {
                    responseText = rawResponse;
                }
            }
            catch
            {
                responseText = rawResponse;
            }

            // 5. Log Analysis & History
            var log = new AIChatLog
            {
                SessionId = sessionId,
                FullName = fullName,
                UserQuery = query,
                BotResponse = responseText,
                Timestamp = DateTime.Now,
                Language = lang
            };
            _context.AIChatLogs.Add(log);
            await _context.SaveChangesAsync();

            return new ChatResponseDTO
            {
                LogId = log.Id,
                ResponseText = responseText,
                Sources = relevantDocs.Select(d => d.FileName).Distinct().ToList(),
                RecommendedQuestions = questions
            };
        }

        public async Task<List<AIChatLogDTO>> GetChatLogs(string search, bool? onlyNegative)
        {
            var (logs, _) = await GetChatLogsPaged(search, onlyNegative, 1, 100);
            return logs;
        }

        public async Task<(List<AIChatLogDTO> logs, int totalCount)> GetChatLogsPaged(string search, bool? onlyNegative, int page, int pageSize)
        {
            var query = _context.AIChatLogs.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(x => x.UserQuery.Contains(search) || x.BotResponse.Contains(search) || x.FullName.Contains(search));
            }

            if (onlyNegative == true)
            {
                query = query.Where(x => x.IsPositive == false);
            }

            var totalCount = await query.CountAsync();
            var logs = await query.Select(x => new AIChatLogDTO
            {
                Id = x.Id,
                SessionId = x.SessionId,
                FullName = x.FullName,
                UserQuery = x.UserQuery,
                BotResponse = x.BotResponse,
                Timestamp = x.Timestamp,
                Language = x.Language,
                IsPositive = x.IsPositive,
                FeedbackComment = x.FeedbackComment
            })
            .OrderByDescending(x => x.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

            return (logs, totalCount);
        }

        public async Task<bool> SubmitFeedback(FeedbackRequestDTO request)
        {
            var log = await _context.AIChatLogs.FindAsync(request.LogId);
            if (log != null)
            {
                log.IsPositive = request.IsPositive;
                log.FeedbackComment = request.Comment;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<List<string>> GetInitialSuggestions(string lang)
        {
            var now = DateTime.Now;
            var docs = await _context.AIAssistantDocuments
                .Where(d => d.IsActive && (d.ExpiryDate == null || d.ExpiryDate > now))
                .Where(d => d.GeneratedQuestions != null && d.GeneratedQuestions != "[]" && d.GeneratedQuestions != "")
                .OrderByDescending(d => d.Id)
                .Take(5)
                .ToListAsync();

            List<string> suggestions = new List<string>();
            foreach (var d in docs)
            {
                try
                {
                    if (d.GeneratedQuestions.TrimStart().StartsWith("[")) 
                    {
                        if (lang.ToUpper() == "AR")
                        {
                            var qList = JsonConvert.DeserializeObject<List<string>>(d.GeneratedQuestions);
                            if (qList != null) suggestions.AddRange(qList);
                        }
                    }
                    else if (d.GeneratedQuestions.TrimStart().StartsWith("{"))
                    {
                        var dict = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(d.GeneratedQuestions);
                        if (dict != null)
                        {
                            var key = lang.ToUpper() == "AR" ? "AR" : "EN";
                            if (dict.ContainsKey(key) && dict[key] != null)
                            {
                                suggestions.AddRange(dict[key]);
                            }
                        }
                    }
                }
                catch { }
            }

            if (suggestions.Any())
            {
                var rnd = new Random();
                return suggestions.OrderBy(x => rnd.Next()).Take(4).ToList();
            }

            return new List<string> 
            { 
                (lang == "AR" ? "ما هي الخدمات؟" : "What are the services?"), 
                (lang == "AR" ? "كيف أتواصل معكم؟" : "How to contact?"), 
                (lang == "AR" ? "سياسة الأمن" : "Security policy") 
            };
        }

        public async Task<List<AIChatSessionDTO>> GetSessions(string fullName)
        {
            return await _context.AIChatSessions
                .Where(x => x.UserFullName == fullName && x.IsActive)
                .OrderByDescending(x => x.IsPinned)
                .ThenByDescending(x => x.CreatedDate)
                .Select(x => new AIChatSessionDTO
                {
                    Id = x.Id,
                    SessionId = x.SessionId,
                    Title = x.Title,
                    IsPinned = x.IsPinned,
                    CreatedDate = x.CreatedDate
                })
                .ToListAsync();
        }

        public async Task<AIChatSessionDTO> CreateSession(string fullName)
        {
            var sessionId = Guid.NewGuid().ToString("N");
            var session = new AIChatSession
            {
                SessionId = sessionId,
                Title = "New Chat",
                UserFullName = fullName,
                IsPinned = false,
                CreatedDate = DateTime.Now,
                IsActive = true
            };
            _context.AIChatSessions.Add(session);
            await _context.SaveChangesAsync();

            return new AIChatSessionDTO
            {
                Id = session.Id,
                SessionId = session.SessionId,
                Title = session.Title,
                IsPinned = session.IsPinned,
                CreatedDate = session.CreatedDate
            };
        }

        public async Task<bool> UpdateSession(int id, string title, bool isPinned)
        {
            var session = await _context.AIChatSessions.FindAsync(id);
            if (session != null)
            {
                if (title != null) session.Title = title;
                session.IsPinned = isPinned;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> DeleteSession(int id)
        {
            var session = await _context.AIChatSessions.FindAsync(id);
            if (session != null)
            {
                session.IsActive = false; // Soft delete
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<List<AIChatLogDTO>> GetSessionMessages(string sessionId)
        {
            return await _context.AIChatLogs
                .Where(x => x.SessionId == sessionId)
                .OrderBy(x => x.Timestamp)
                .Select(x => new AIChatLogDTO
                {
                    Id = x.Id,
                    FullName = x.FullName,
                    UserQuery = x.UserQuery,
                    BotResponse = x.BotResponse,
                    Timestamp = x.Timestamp,
                    IsPositive = x.IsPositive,
                    FeedbackComment = x.FeedbackComment,
                    Language = x.Language
                })
                .ToListAsync();
        }

        private async Task<string> CallOllama(string query, string context, List<AIChatLog> history, string lang)
        {
            var endpoint = _configuration["Ollama:Endpoint"];
            var model = _configuration["Ollama:ModelName"] ?? "llama3";

            var systemPrompt = $@"
            You are a strict and helpful AI assistant for the Fujairah Natural Resources Corporation (FNRC).
            MISSION: Help users with FNRC services, departments, technical guidance, and system usage.
            RULE: Answer ONLY using the context. If not found, say you only help with FNRC data.
            RULE: Do NOT mention document names, filenames, or file extensions in the answer or follow-up questions. Make the response sound natural.
            RULE: If your answer contains a list of steps or items, you MUST format them properly using HTML <ul>, <ol>, and <li> tags. Use <b> or <strong> for emphasis. DO NOT use markdown asterisks (*).

            FORMAT: Respond strictly in JSON:
            {{
              ""answer"": ""your localized answer"",
              ""questions"": [""follow-up 1"", ""follow-up 2"", ""follow-up 3""]
            }}

            Language: {(lang == "AR" ? "Arabic" : "English")}.
            Context: {context}";

            var messages = new List<object> { new { role = "system", content = systemPrompt } };
            foreach (var h in history)
            {
                messages.Add(new { role = "user", content = h.UserQuery });
                messages.Add(new { role = "assistant", content = h.BotResponse });
            }
            messages.Add(new { role = "user", content = query });

            var requestBody = new { model = model, messages = messages, stream = false, options = new { temperature = 0.3 } };
            var json = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var auth = _configuration["Ollama:Auth"];
                if (!string.IsNullOrEmpty(auth))
                {
                    var authBytes = Encoding.UTF8.GetBytes(auth);
                    _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(authBytes));
                }

                var response = await _httpClient.PostAsync($"{endpoint.TrimEnd('/')}/api/chat", content);
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    dynamic result = JsonConvert.DeserializeObject(responseString);
                    return result.message.content;
                }
                var errorBody = await response.Content.ReadAsStringAsync();
                return $"Error calling Ollama: {response.StatusCode} - {errorBody}";

            }
            catch (Exception ex)
            {
                return "Ollama error: " + ex.Message;
            }
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
            RULE: Do NOT mention document names, filenames, or file extensions in the answer or follow-up questions. Make the response sound natural.
            RULE: If your answer contains a list of steps or items, you MUST format them properly using HTML <ul>, <ol>, and <li> tags. Use <b> or <strong> for emphasis. DO NOT use markdown asterisks (*).

            FORMAT: Respond strictly in JSON:
            {{
              ""answer"": ""your localized answer"",
              ""questions"": [""follow-up 1"", ""follow-up 2"", ""follow-up 3""]
            }}

            Language: {(lang == "AR" ? "Arabic" : "English")}.
            Context: {context}";

            var messages = new List<object> { new { role = "system", content = systemPrompt } };

            foreach (var h in history)
            {
                messages.Add(new { role = "user", content = h.UserQuery });
                messages.Add(new { role = "assistant", content = h.BotResponse });
            }
            messages.Add(new { role = "user", content = query });

            var requestBody = new { messages = messages, max_tokens = 1500, temperature = 0.3 };
            var json = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("api-key", apiKey);

            try
            {
                var url = $"{endpoint.TrimEnd('/')}/openai/deployments/{deployment}/chat/completions?api-version=2024-02-15-preview";
                var response = await _httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    dynamic result = JsonConvert.DeserializeObject(responseString);
                    return result.choices[0].message.content;
                }
                return "Error calling Azure OpenAI.";
            }
            catch (Exception ex)
            {
                return "Azure AI error: " + ex.Message;
            }
        }

        private async Task<string> CallGemini(string query, string context, List<AIChatLog> history, string lang)
        {
            var apiKey = _configuration["Gemini:ApiKey"];
            var model = _configuration["Gemini:ModelName"] ?? "gemini-1.5-pro";

            if (string.IsNullOrEmpty(apiKey)) return "Gemini configuration is missing.";

            var systemPrompt = $@"
            You are a strict and helpful AI assistant for the Fujairah Natural Resources Corporation (FNRC).
            MISSION: Help users with FNRC services, departments, technical guidance, and system usage.
            RULE: Answer ONLY using the context. If not found, say you only help with FNRC data.
            RULE: Do NOT mention document names, filenames, or file extensions in the answer or follow-up questions. Make the response sound natural.
            RULE: If your answer contains a list of steps or items, you MUST format them properly using HTML <ul>, <ol>, and <li> tags. Use <b> or <strong> for emphasis. DO NOT use markdown asterisks (*).

            FORMAT: Respond strictly in JSON:
            {{
              ""answer"": ""your localized answer"",
              ""questions"": [""follow-up 1"", ""follow-up 2"", ""follow-up 3""]
            }}

            Language: {(lang == "AR" ? "Arabic" : "English")}.
            Context: {context}";

            // Build the contents array
            var contents = new List<object>();

            // We can pass system instructions via systemInstruction in Gemini API
            var systemInstruction = new { parts = new[] { new { text = systemPrompt } } };

            foreach (var h in history)
            {
                contents.Add(new { role = "user", parts = new[] { new { text = h.UserQuery } } });
                contents.Add(new { role = "model", parts = new[] { new { text = h.BotResponse } } });
            }
            contents.Add(new { role = "user", parts = new[] { new { text = query } } });

            var requestBody = new 
            { 
                contents = contents, 
                systemInstruction = systemInstruction,
                generationConfig = new { temperature = 0.3 } 
            };
            
            var json = JsonConvert.SerializeObject(requestBody);
            var requestContent = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";
                var response = await _httpClient.PostAsync(url, requestContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    dynamic result = JsonConvert.DeserializeObject(responseString);
                    // Gemini response format: result.candidates[0].content.parts[0].text
                    string textResponse = result.candidates[0].content.parts[0].text;
                    // Sometimes Gemini wraps JSON in markdown block ```json ... ```
                    if (textResponse.StartsWith("```json"))
                    {
                        textResponse = textResponse.Substring(7);
                        if (textResponse.EndsWith("```"))
                        {
                            textResponse = textResponse.Substring(0, textResponse.Length - 3);
                        }
                    }
                    else if (textResponse.StartsWith("```"))
                    {
                        textResponse = textResponse.Substring(3);
                        if (textResponse.EndsWith("```"))
                        {
                            textResponse = textResponse.Substring(0, textResponse.Length - 3);
                        }
                    }
                    return textResponse.Trim();
                }
                var errorBody = await response.Content.ReadAsStringAsync();
                return $"Error calling Gemini: {response.StatusCode} - {errorBody}";
            }
            catch (Exception ex)
            {
                return "Gemini error: " + ex.Message;
            }
        }

        public async Task<AIAnalyticsDTO> GetAnalytics()
        {
            var now = DateTime.Now;
            var thirtyDaysAgo = now.AddDays(-30);
            
            var totalDocs = await _context.AIAssistantDocuments.CountAsync(d => d.IsActive);
            var totalChats = await _context.AIChatLogs.CountAsync(l => l.Timestamp >= thirtyDaysAgo);
            var negativeFeedbacks = await _context.AIChatLogs.CountAsync(l => l.IsPositive == false && l.Timestamp >= thirtyDaysAgo);

            // Simple trending words (rudimentary implementation)
            var recentQueries = await _context.AIChatLogs
                .Where(l => l.Timestamp >= now.AddDays(-7))
                .Select(l => l.UserQuery)
                .ToListAsync();
            
            var topWords = recentQueries
                .SelectMany(q => q.Split(' '))
                .Where(w => w.Length > 4)
                .GroupBy(w => w.ToLower())
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => g.Key)
                .ToList();

            var unanswered = await _context.AIChatLogs
                .Where(l => l.BotResponse.Contains("Sorry") || l.BotResponse.Contains("I don't know") || l.BotResponse.Contains("only help with FNRC data"))
                .OrderByDescending(l => l.Timestamp)
                .Take(10)
                .Select(x => new AIChatLogDTO
                {
                    Id = x.Id,
                    FullName = x.FullName,
                    UserQuery = x.UserQuery,
                    BotResponse = x.BotResponse,
                    Timestamp = x.Timestamp
                })
                .ToListAsync();

            return new AIAnalyticsDTO
            {
                TotalDocuments = totalDocs,
                TotalChats = totalChats,
                NegativeFeedbacks = negativeFeedbacks,
                TopTrendingKeywords = topWords,
                UnansweredQueries = unanswered
            };
        }

        public async Task<List<AIQAOverrideDTO>> GetQAOverrides()
        {
            return await _context.AIQAOverrides
                .Select(x => new AIQAOverrideDTO
                {
                    Id = x.Id,
                    Question = x.Question,
                    Answer = x.Answer,
                    IsActive = x.IsActive
                })
                .ToListAsync();
        }

        public async Task<bool> AddQAOverride(string question, string answer)
        {
            _context.AIQAOverrides.Add(new AIQAOverride { Question = question, Answer = answer, IsActive = true });
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteQAOverride(int id)
        {
            var item = await _context.AIQAOverrides.FindAsync(id);
            if (item != null)
            {
                _context.AIQAOverrides.Remove(item);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
    }
}
