using DigitalHub.Domain.Shared;

namespace DigitalHub.Domain.Domains
{
    public class AIChatSession : BaseDomainEntity
    {
        public string SessionId { get; set; } // The unique string ID used in logs
        public string Title { get; set; }
        public string UserFullName { get; set; }
        public bool IsPinned { get; set; }
    }
}
