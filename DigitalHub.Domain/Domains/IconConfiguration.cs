using DigitalHub.Domain.Enums;
using DigitalHub.Domain.Shared;

namespace DigitalHub.Domain.Domains
{
    public class IconConfiguration : BaseDomainEntity
    {
        public string NameEn { get; set; }
        public string NameAr { get; set; }
        public string DescriptionEn { get; set; }
        public string DescriptionAr { get; set; }
        public int OrderNo { get; set; }
        public string URL { get; set; }
        public IconTypeEnum IconType { get; set; }
        public int VideoDisplaySec { get; set; } = 2;
        public ICollection<IconConfigurationAttachment> IconConfigurationAttachments { get; set; } = [];

    }
    
 
}
