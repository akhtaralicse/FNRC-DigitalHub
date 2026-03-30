using DigitalHub.Domain.DBContext;
using DigitalHub.Domain.Domains;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FNRC_DigitalHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeChatController : BaseController
    {
        private readonly DigitalHubDBContext _context;

        public EmployeeChatController(DigitalHubDBContext context)
        {
            _context = context;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var currentUser = GetUser();
            if (currentUser == null) return Unauthorized();

            var users = await _context.Users
                .Where(u => u.EmployeeId != currentUser.EmployeeID)
                .Select(u => new {
                    u.Id,
                    u.EmployeeId,
                    u.NameEn,
                    u.NameAr,
                    u.DepartmentNameEn,
                    IsOnline = _context.UserConnections.Any(c => c.UserId == u.Id && c.IsOnline)
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpGet("history/{otherUserId}")]
        public async Task<IActionResult> GetHistory(int otherUserId)
        {
            var currentUser = GetUser();
            if (currentUser == null) return Unauthorized();

            var myId = (await _context.Users.FirstAsync(u => u.EmployeeId == currentUser.EmployeeID)).Id;

            var history = await _context.EmployeeChatMessages
                .Where(m => (m.SenderId == myId && m.ReceiverId == otherUserId) || (m.SenderId == otherUserId && m.ReceiverId == myId))
                .OrderBy(m => m.SentDate)
                .Select(m => new {
                    m.SenderId,
                    m.ReceiverId,
                    m.Message,
                    m.SentDate,
                    m.IsRead
                })
                .ToListAsync();

            return Ok(history);
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var session = GetUser();
            if (session == null) return Unauthorized();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.EmployeeId == session.EmployeeID);
            return Ok(user);
        }
    }
}
