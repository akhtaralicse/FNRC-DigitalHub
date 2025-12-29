using System.ComponentModel.DataAnnotations;

namespace DigitalHub.Services.DTO
{
    public partial class AttachmentTransactionDTO : BaseDomainEntityDTO
    {
        [StringLength(50)] public string FileId { get; set; }
        [StringLength(200)] public string FilePath { get; set; }
        [StringLength(200)] public string ThumbPath { get; set; }
        [StringLength(500)] public string FileName { get; set; }
        [StringLength(10)] public string FileExtension { get; set; }
        public int FileSize { get; set; } = 0;
        public string FileFolder { get; set; }
        [StringLength(100)] public string FileMimeType { get; set; }
        public bool? IsThumb { get; set; }

    }
}
