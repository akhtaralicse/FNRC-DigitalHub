using AutoMapper;
using DigitalHub.Domain.Domains; 
using DigitalHub.Services.DTO; 
using DigitalHub.Services.Services.Attachment;
using DigitalHub.Services.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DigitalHub.Services.Services.IconConfig
{
    public class NotificationConfigurationService(IGenericRepository<NotificationConfiguration> repository, IConfiguration configuration,
        IMapper mapper, IAttachmentService attachmentService) : INotificationConfigurationService
    {
        public IGenericRepository<NotificationConfiguration> _repository { get; } = repository;
        public IConfiguration Configuration { get; } = configuration;
        public IMapper Mapper { get; } = mapper;
        public IAttachmentService AttachmentService { get; } = attachmentService;

        public async Task<bool> Add(NotificationConfigurationDTO model)
        {

            if (model.Files != null && model.Files.Count > 0)
            {
                var attachmentList = await AttachmentService.UploadAttachment(model.Files);

                foreach (var attach in attachmentList)
                {
                    model.NotificationAttachment.Add(new NotificationAttachmentDTO
                    {
                        AttachmentId = attach.Id,
                    });
                }
            }

            var result = Mapper.Map<NotificationConfiguration>(model);
            await _repository.InsertAsync(result, true);

            return true;
        }

        public async Task<List<NotificationConfigurationDTO>> Get(int id = 0)
        {
            var result = _repository.GetAllIncludingNoTracking(x => x.NotificationAttachment,
                                    x => x.NotificationAttachment.Select(y => y.AttachmentTransaction)).Where(x => x.IsActive == true).AsQueryable();
            if (id > 0)
            {
                result = result.Where(x => x.Id == id);
            }

            return Mapper.Map<List<NotificationConfigurationDTO>>(await result.OrderBy(x => x.Id).ToListAsync());
        }

        public async Task<bool> Update(NotificationConfigurationDTO mod)
        {
            var result = await _repository.GetAllIncludingNoTracking().FirstOrDefaultAsync(x => x.Id == mod.Id);
            var data = Mapper.Map<NotificationConfiguration>(result);
            result.TitleAr = mod.TitleAr;
            result.TitleEn = mod.TitleEn;
            result.MessageEn = mod.MessageEn;
            result.MessageAr = mod.MessageAr;
            result.ActionTextEn = mod.ActionTextEn;
            result.ActionTextAr = mod.ActionTextAr;
            result.ActionUrl = mod.ActionUrl;
            result.StartDate = mod.StartDate;
            result.EndDate = mod.EndDate;

            _repository.Update(result, true);

            return true;
        }

        public async Task<bool> Delete(int id)
        {
            var result = await _repository.GetAllIncludingNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            result.IsActive = false;
            _repository.Update(result, true);

            return true;
        }
    }
}
