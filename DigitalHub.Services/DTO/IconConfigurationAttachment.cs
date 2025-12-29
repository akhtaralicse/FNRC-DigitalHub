using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalHub.Services.DTO
{
    public partial class IconConfigurationAttachmentDTO : BaseDomainEntityDTO
    {
        public int IconConfigurationId { get; set; }
        [ForeignKey(nameof(IconConfigurationId))] public virtual IconConfigurationDTO IconConfiguration { get; set; }

        public int AttachmentId { get; set; }
        [ForeignKey(nameof(AttachmentId))] public virtual AttachmentTransactionDTO AttachmentTransaction { get; set; }

    }
}
