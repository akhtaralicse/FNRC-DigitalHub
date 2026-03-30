using DigitalHub.Domain.Shared;

namespace DigitalHub.Domain.Domains
{
    public class AIChatLog : BaseDomainEntity
    {
        public string SessionId { get; set; }
        public string FullName { get; set; }
        public string UserQuery { get; set; }
        public string BotResponse { get; set; }
        public DateTime Timestamp { get; set; }
        public string Language { get; set; }
    }
}
