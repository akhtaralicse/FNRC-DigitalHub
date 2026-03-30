using Microsoft.AspNetCore.SignalR;
using DigitalHub.Domain.DBContext;
using DigitalHub.Domain.Domains;
using Microsoft.EntityFrameworkCore;

namespace DigitalHub.Services.SignalR
{
    public class ChatHub : Hub
    {
        private readonly DigitalHubDBContext _context;

        public ChatHub(DigitalHubDBContext context)
        {
            _context = context;
        }

        public async Task Join(int userId)
        {
            var connections = _context.UserConnections.Where(c => c.UserId == userId);
            _context.UserConnections.RemoveRange(connections);

            _context.UserConnections.Add(new UserConnection
            {
                UserId = userId,
                ConnectionId = Context.ConnectionId,
                IsOnline = true,
                LastActive = DateTime.Now
            });

            await _context.SaveChangesAsync();
            await Clients.All.SendAsync("UserStatusChange", userId, true);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var conn = await _context.UserConnections.FirstOrDefaultAsync(c => c.ConnectionId == Context.ConnectionId);
            if (conn != null)
            {
                conn.IsOnline = false;
                conn.LastActive = DateTime.Now;
                await _context.SaveChangesAsync();
                await Clients.All.SendAsync("UserStatusChange", conn.UserId, false);
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(int senderId, int receiverId, string message)
        {
            var msg = new EmployeeChatMessage
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Message = message,
                SentDate = DateTime.Now,
                IsRead = false
            };

            _context.EmployeeChatMessages.Add(msg);
            await _context.SaveChangesAsync();

            var receiverConn = await _context.UserConnections.Where(c => c.UserId == receiverId && c.IsOnline).ToListAsync();
            
            foreach (var conn in receiverConn)
            {
                await Clients.Client(conn.ConnectionId).SendAsync("ReceiveMessage", senderId, message, msg.SentDate);
            }
        }
    }
}
