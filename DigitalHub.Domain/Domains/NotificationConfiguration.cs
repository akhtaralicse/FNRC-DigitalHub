using DigitalHub.Domain.Enums;
using DigitalHub.Domain.Shared;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalHub.Domain.Domains
{
    public class NotificationConfiguration : BaseDomainEntity
    {
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
        public ICollection<NotificationAttachment> NotificationAttachment { get; set; } = [];
        public ICollection<NotificationUser> NotificationUser { get; set; } = [];


    }
    public class NotificationUser : BaseDomainEntity
    {
        public string Username { get; set; }

        public int NotificationConfigurationId { get; set; }
        [ForeignKey(nameof(NotificationConfigurationId))] public virtual NotificationConfiguration NotificationConfiguration { get; set; }

        public bool IsRead { get; set; } = false;
        public DateTime? ReadDateTime { get; set; }
         
    }
}
