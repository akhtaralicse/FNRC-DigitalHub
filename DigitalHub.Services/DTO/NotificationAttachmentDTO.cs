using DigitalHub.Domain.Shared;

namespace DigitalHub.Services.DTO
{
    public partial class NotificationAttachmentDTO : BaseDomainEntity
    {
        public int NotificationConfigurationId { get; set; }

        public int AttachmentId { get; set; }

    }

}
