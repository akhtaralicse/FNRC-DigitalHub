using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc; 

namespace DigitalHub.AIGateway.Controllers;

[Authorize]
[ApiController]
[Route("api")]
public class OllamaController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly string _ollamaBaseUrl;
    private readonly ILogger<OllamaController> _logger;

    public OllamaController(IConfiguration configuration, ILogger<OllamaController> logger)
    {
        _logger = logger;
        _ollamaBaseUrl = configuration["Ollama:BaseUrl"] ?? "http://localhost:11434";
        
        // Use a simple HttpClient for proxying
        var handler = new HttpClientHandler { UseProxy = false };
        _httpClient = new HttpClient(handler) { Timeout = TimeSpan.FromMinutes(10) };
    }

    [HttpPost("chat")]
    public async Task Chat()
    {
        await ProxyRequest("api/chat");
    }

    [HttpPost("generate")]
    public async Task Generate()
    {
        await ProxyRequest("api/generate");
    }

    [HttpGet("tags")]
    public async Task<IActionResult> GetTags()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_ollamaBaseUrl.TrimEnd('/')}/api/tags");
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tags from Ollama");
            return StatusCode(500, "Error connecting to Ollama service");
        }
    }

    private async Task ProxyRequest(string endpoint)
    {
        try
        {
            var targetUrl = $"{_ollamaBaseUrl.TrimEnd('/')}/{endpoint}";
            
            using var request = new HttpRequestMessage(HttpMethod.Post, targetUrl);
            
            // Copy body
            if (Request.ContentLength > 0)
            {
                var memoryStream = new MemoryStream();
                await Request.Body.CopyToAsync(memoryStream);
                memoryStream.Position = 0;
                request.Content = new StreamContent(memoryStream);
                
                if (Request.ContentType != null)
                    request.Content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(Request.ContentType);
            }

            // Execute request with streaming support
            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            
            Response.StatusCode = (int)response.StatusCode;
            
            // Copy headers
            foreach (var header in response.Headers)
            {
                Response.Headers[header.Key] = header.Value.ToArray();
            }
            foreach (var header in response.Content.Headers)
            {
                if (header.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                {
                    Response.ContentType = header.Value.FirstOrDefault();
                }
                else
                {
                    Response.Headers[header.Key] = header.Value.ToArray();
                }
            }

            // Stream the response back to the client
            await response.Content.CopyToAsync(Response.Body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error proxying request to Ollama endpoint: {endpoint}");
            if (!Response.HasStarted)
            {
                Response.StatusCode = 500;
                await Response.WriteAsync($"Gateway Error: {ex.Message}");
            }
        }
    }
}
