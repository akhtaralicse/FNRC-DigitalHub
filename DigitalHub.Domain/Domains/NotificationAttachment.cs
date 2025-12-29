using DigitalHub.Domain.Shared;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalHub.Domain.Domains
{
    public partial class NotificationAttachment : BaseDomainEntity
    {
        public int NotificationConfigurationId { get; set; }
        [ForeignKey(nameof(NotificationConfigurationId))] public virtual NotificationConfiguration NotificationConfiguration { get; set; }

        public int AttachmentId { get; set; }
        [ForeignKey(nameof(AttachmentId))] public virtual AttachmentTransaction AttachmentTransaction { get; set; }

    }




}
