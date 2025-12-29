using AutoMapper;
using DigitalHub.Domain.Domains;
using DigitalHub.Services.Services.Attachment;
using DigitalHub.Services.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalHub.Services.Services.IconConfig
{
    public class IconConfigurationAttachmentService(IGenericRepository<IconConfigurationAttachment> repository, IConfiguration configuration, IMapper mapper,
      IAttachmentService attachmentService) : IIconConfigurationAttachmentService
    {
        public IGenericRepository<IconConfigurationAttachment> _repository { get; } = repository;
        public IConfiguration Configuration { get; } = configuration;
        public IMapper Mapper { get; } = mapper;
        public IAttachmentService AttachmentService { get; } = attachmentService;


        public async Task<bool> DeleteAttachment(int id)
        {
            var attachment = await _repository.GetAllIncludingNoTracking(x => x.AttachmentTransaction).FirstOrDefaultAsync(x => x.Id == id);
            if (attachment != null)
            {
                await _repository.DeleteAsync(attachment, true);

                await AttachmentService.DeleteFile(attachment.AttachmentTransaction);

                var isDeleted = AttachmentService.DeletePhysicalFile(attachment.AttachmentTransaction.FilePath, attachment.AttachmentTransaction.FileId + attachment.AttachmentTransaction.FileExtension);
                var isDeletedThumb = AttachmentService.DeletePhysicalFile(attachment.AttachmentTransaction.FilePath, attachment.AttachmentTransaction.FileId + "_thumb" + attachment.AttachmentTransaction.FileExtension);
                return isDeleted;
            }

            return false;
        }

    }
}
