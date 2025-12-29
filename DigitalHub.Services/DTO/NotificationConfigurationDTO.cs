using DigitalHub.Domain.Enums;
using DigitalHub.Domain.Shared;
using Microsoft.AspNetCore.Http;

namespace DigitalHub.Services.DTO
{
    public class NotificationConfigurationDTO : BaseDomainEntity
    {
        public List<IFormFile> Files { get; set; } = [];
        public string TitleAr { get; set; }
        public string TitleEn { get; set; }
        public string MessageAr { get; set; }
        public string MessageEn { get; set; }
        public NotificationTypeEnum Type { get; set; } = NotificationTypeEnum.Info;
        public NotificationSeverityEnum Severity { get; set; } = NotificationSeverityEnum.Normal;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public NotificationAudienceEnum Audience { get; set; } = NotificationAudienceEnum.AllUsers;
        public string ActionUrl { get; set; }
        public string ActionTextEn { get; set; }
        public string ActionTextAr { get; set; }
        public ICollection<NotificationAttachmentDTO> NotificationAttachment { get; set; } = [];


    }

}
