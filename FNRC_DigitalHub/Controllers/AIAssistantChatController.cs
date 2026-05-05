using DigitalHub.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace FNRC_DigitalHub.Controllers
{
    public class AIAssistantChatController : BaseController
    {
        private readonly IAIAssistantService _aiService;

        public AIAssistantChatController(IAIAssistantService aiService)
        {
            _aiService = aiService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetSessions()
        {
            var user = GetUser();
            string fullName = user?.NameEn ?? "Anonymous";
            var sessions = await _aiService.GetSessions(fullName);
            return Json(new { success = true, data = sessions });
        }

        [HttpPost]
        public async Task<IActionResult> CreateSession()
        {
            var user = GetUser();
            string fullName = user?.NameEn ?? "Anonymous";
            var session = await _aiService.CreateSession(fullName);
            return Json(new { success = true, data = session });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateSession(int id, string title, bool isPinned)
        {
            var result = await _aiService.UpdateSession(id, title, isPinned);
            return Json(new { success = result });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSession(int id)
        {
            var result = await _aiService.DeleteSession(id);
            return Json(new { success = result });
        }

        [HttpGet]
        public async Task<IActionResult> GetSessionMessages(string sessionId)
        {
            var messages = await _aiService.GetSessionMessages(sessionId);
            return Json(new { success = true, data = messages });
        }
    }
}
