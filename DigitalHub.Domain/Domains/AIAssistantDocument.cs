using DigitalHub.Domain.Shared; 
using System;

namespace DigitalHub.Domain.Domains
{
    public class AIAssistantDocument : BaseDomainEntity
    { 
        public string FileName { get; set; }
        public string Content { get; set; }
        public string Language { get; set; } // AR or EN or Mixed
        public string UploaderName { get; set; }
        public long FileSize { get; set; }
        public string GeneratedQuestions { get; set; }
        public string Category { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
