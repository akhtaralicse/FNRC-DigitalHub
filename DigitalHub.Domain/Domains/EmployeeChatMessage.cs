using DigitalHub.Domain.Shared;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalHub.Domain.Domains
{
    public class EmployeeChatMessage : BaseDomainEntity
    {
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public string Message { get; set; }
        public DateTime SentDate { get; set; }
        public bool IsRead { get; set; }
        public string AttachmentUrl { get; set; }

        [ForeignKey(nameof(SenderId))] public virtual Users Sender { get; set; }
        [ForeignKey(nameof(ReceiverId))] public virtual Users Receiver { get; set; }
    }

    public class UserConnection : BaseDomainEntity
    {
        public int UserId { get; set; }
        public string ConnectionId { get; set; }
        public bool IsOnline { get; set; }
        public DateTime LastActive { get; set; }

        [ForeignKey(nameof(UserId))] public virtual Users User { get; set; }
    }
}
