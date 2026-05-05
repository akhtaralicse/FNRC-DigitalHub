using DigitalHub.Domain.Shared;

namespace DigitalHub.Domain.Domains
{
    public class AIQAOverride : BaseDomainEntity
    {
        public string Question { get; set; }
        public string Answer { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
