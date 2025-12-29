using DigitalHub.Domain.Domains;
using DigitalHub.Services.DTO;
using Microsoft.AspNetCore.Http;

namespace DigitalHub.Services.Services.Attachment
{
    public interface IAttachmentService
    {
        Task<bool> AddAttachment(AttachmentTransactionDTO model);
        Task<bool> DeleteFile(AttachmentTransaction model);
        bool DeletePhysicalFile(string folder, string file);
        Task<bool> UpdateAttachment(AttachmentTransactionDTO model);
        Task<List<AttachmentTransaction>> UploadAttachment(List<IFormFile> files);
    }
}