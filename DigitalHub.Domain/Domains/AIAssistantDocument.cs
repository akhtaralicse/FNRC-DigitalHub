using DigitalHub.Domain.Shared; 

namespace DigitalHub.Domain.Domains
{
    public class AIAssistantDocument : BaseDomainEntity
    { 
        public string FileName { get; set; }
        public string Content { get; set; }
        public string Language { get; set; } // AR or EN or Mixed
        public string UploaderName { get; set; }
        public long FileSize { get; set; }
    }
}
