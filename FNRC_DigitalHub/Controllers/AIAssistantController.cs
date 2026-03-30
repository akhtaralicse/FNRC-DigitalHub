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
        public async Task<IActionResult> Upload(List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
                return BadRequest("No files uploaded.");

            var user = GetUser();
            string uploaderName = user?.NameEn ?? "Admin";

            var result = await _aiService.UploadDocuments(files, uploaderName);
            return Ok(new { success = result, message = "Documents processed successfully." });
        }
    }
}
