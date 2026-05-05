using DigitalHub.Services.DTO.AIAssistant;
using DigitalHub.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace FNRC_DigitalHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AIAssistantController : BaseController
    {
        private readonly IAIAssistantService _aiService;

        public AIAssistantController(IAIAssistantService aiService)
        {
            _aiService = aiService;
        }

        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] ChatRequestDTO request)
        {
            if (string.IsNullOrEmpty(request.Query))
                return BadRequest("Query cannot be empty.");

            var user = GetUser();
            string fullName = user?.NameEn ?? "Anonymous";
            
            var response = await _aiService.Chat(request.Query, request.Lang ?? "EN", request.SessionId ?? HttpContext.Session.Id, fullName);
            
            return Ok(response);
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] List<IFormFile> files, [FromForm] string language = "Mixed", [FromForm] string category = "General", [FromForm] DateTime? expiryDate = null)
        {
            if (files == null || files.Count == 0)
                return BadRequest("No files uploaded.");

            var user = GetUser();
            string uploaderName = user?.NameEn ?? "Admin";

            var result = await _aiService.UploadDocuments(files, uploaderName, language, category, expiryDate);
            return Ok(new { success = result, message = "Documents processed successfully." });
        }

        [HttpPost("toggle-status")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var result = await _aiService.ToggleDocumentStatus(id);
            return Ok(new { success = result });
        }

        [HttpPost("regenerate-questions")]
        public async Task<IActionResult> RegenerateQuestions(int id)
        {
            var result = await _aiService.RegenerateQuestions(id);
            if (!result) return NotFound(new { success = false, message = "Document not found." });
            return Ok(new { success = true });
        }

        [HttpPost("submit-feedback")]
        public async Task<IActionResult> SubmitFeedback([FromBody] FeedbackRequestDTO request)
        {
            var result = await _aiService.SubmitFeedback(request);
            return Ok(new { success = result });
        }

        [HttpGet("suggestions")]
        public async Task<IActionResult> GetSuggestions(string lang = "EN")
        {
            var suggestions = await _aiService.GetInitialSuggestions(lang);
            return Ok(suggestions);
        }

        [HttpGet("analytics")]
        public async Task<IActionResult> GetAnalytics()
        {
            var result = await _aiService.GetAnalytics();
            return Ok(new { success = true, data = result });
        }

        [HttpGet("qa-overrides")]
        public async Task<IActionResult> GetQAOverrides()
        {
            var result = await _aiService.GetQAOverrides();
            return Ok(new { success = true, data = result });
        }

        [HttpPost("add-qa-override")]
        public async Task<IActionResult> AddQAOverride([FromForm] string question, [FromForm] string answer)
        {
            if (string.IsNullOrEmpty(question) || string.IsNullOrEmpty(answer))
                return BadRequest("Question and answer are required.");
                
            var result = await _aiService.AddQAOverride(question, answer);
            return Ok(new { success = result });
        }

        [HttpPost("delete-qa-override")]
        public async Task<IActionResult> DeleteQAOverride(int id)
        {
            var result = await _aiService.DeleteQAOverride(id);
            return Ok(new { success = result });
        }
    }
}
