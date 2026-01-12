using DigitalHub.Domain.Shared;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalHub.Domain.Domains
{
    public partial class IconConfigurationAttachment : BaseDomainEntity
    {
        public int IconConfigurationId { get; set; }
        [ForeignKey(nameof(IconConfigurationId))] public virtual IconConfiguration IconConfiguration { get; set; }

        public int AttachmentId { get; set; }
        [ForeignKey(nameof(AttachmentId))] public virtual AttachmentTransaction AttachmentTransaction { get; set; }

      

    }
}
