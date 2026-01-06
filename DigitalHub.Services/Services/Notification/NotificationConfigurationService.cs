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
        public async Task<bool> AddUserNotification(NotificationUserDTO model)
        {

            var result = Mapper.Map<NotificationUser>(model);
            result.IsRead = true;

            var data = await _repository.GetAllIncludingNoTracking(x => x.NotificationUser).FirstOrDefaultAsync(x => x.Id == model.Id);
            if (data != null)
            {
                if (!data.NotificationUser.Any(x => x.NotificationConfigurationId == model.Id))
                {
                    result.Id = 0;
                    data.NotificationUser.Add(result);
                    await _repository.UpdateAsync(data, true);
                }
            }

            return true;
        }
        public async Task<List<NotificationConfigurationDTO>> GetAll()
        {
            var result = _repository.GetAllIncludingNoTracking(x => x.NotificationAttachment,
                                    x => x.NotificationAttachment.Select(y => y.AttachmentTransaction))
                        .Where(x => x.IsActive == true).AsQueryable();

            return Mapper.Map<List<NotificationConfigurationDTO>>(await result.OrderByDescending(x => x.StartDate).ToListAsync());
        }
        public async Task<List<NotificationConfigurationDTO>> Get(string username = null)
        {
            var result = _repository.GetAllIncludingNoTracking(  x => x.NotificationAttachment,
                                    x => x.NotificationAttachment.Select(y => y.AttachmentTransaction))
                .Where(x => x.IsActive == true && x.StartDate <= DateTime.Now && x.EndDate >= DateTime.Now).AsQueryable();
            //if (username != null)
            //{
            //    result = result.Where(x => !x.NotificationUser.Any(x => x.Username == username && x.IsRead == true));
            //}
            return Mapper.Map<List<NotificationConfigurationDTO>>(await result.OrderByDescending(x => x.StartDate).ToListAsync());
        }
        public async Task<NotificationConfigurationDTO> GetNotificationToDisplay(string username)
        {
            var result = await _repository.GetAllIncludingNoTracking(x => x.NotificationAttachment,
                                    x => x.NotificationAttachment.Select(y => y.AttachmentTransaction))
                .Where(x => x.IsActive == true && x.StartDate <= DateTime.Now && x.EndDate >= DateTime.Now
                        && !x.NotificationUser.Any(x => x.Username == username && x.IsRead == true)).OrderByDescending(x => x.StartDate).FirstAsync();

            return Mapper.Map<NotificationConfigurationDTO>(result);
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
