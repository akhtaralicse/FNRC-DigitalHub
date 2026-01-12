using DigitalHub.Domain.Enums;
using Microsoft.AspNetCore.Http; 

namespace DigitalHub.Services.DTO
{
    public class IconConfigurationDTO : BaseDomainEntityDTO
    {
        public List<IFormFile> Files { get; set; } = [];
        public string NameEn { get; set; }
        public string NameAr { get; set; }
        public string URL { get; set; }
        public string DescriptionEn { get; set; }
        public string DescriptionAr { get; set; }
        public int OrderNo { get; set; }
        public int VideoDisplaySec { get; set; }
        public IconTypeEnum IconType { get; set; }
        public ICollection<IconConfigurationAttachmentDTO> IconConfigurationAttachments { get; set; } = [];


    }

}
