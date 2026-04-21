using AutoMapper;
using DigitalHub.Domain.Domains;
using DigitalHub.Services.DTO;
using DigitalHub.Services.Services.Attachment;
using DigitalHub.Services.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

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
            model.Id = 0;
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
                if (!data.NotificationUser.Any(x => x.NotificationConfigurationId == model.Id && x.Username == model.Username))
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
        public async Task<NotificationConfigurationDTO> GetById(int id)
        {
            var result = await _repository.GetAllIncludingNoTracking(x => x.NotificationAttachment,
                                    x => x.NotificationAttachment.Select(y => y.AttachmentTransaction))
                        .Where(x => x.IsActive == true && x.Id == id).FirstOrDefaultAsync();

            return Mapper.Map<NotificationConfigurationDTO>(result);
        }

        public async Task<List<NotificationConfigurationDTO>> GetBySearch(string name, DateTime? dateFrom, DateTime? dateTo, int pageSize = 0, int skip = 0)
        {
            var result = _repository.GetAllIncludingNoTracking(x => x.NotificationAttachment,
                                    x => x.NotificationAttachment.Select(y => y.AttachmentTransaction)).AsQueryable();
            if (!string.IsNullOrEmpty(name))
            {
                result = result.Where(x => x.TitleAr.Contains(name) || x.TitleEn.Contains(name) || x.MessageAr.Contains(name) || x.MessageEn.Contains(name));
            }
            if (dateFrom != null && dateTo != null)
            {
                result = result.Where(x => x.StartDate >= dateFrom && x.EndDate <= dateTo);
            }
            if (pageSize == 0)
            {
                return Mapper.Map<List<NotificationConfigurationDTO>>(await result.OrderByDescending(x => x.StartDate).ToListAsync());
            }

            return Mapper.Map<List<NotificationConfigurationDTO>>(await result.Skip(skip).Take(pageSize).OrderByDescending(x => x.StartDate).ToListAsync());
        }
        public async Task<List<NotificationConfigurationDTO>> Get(string username = null)
        {
            var result = _repository.GetAllIncludingNoTracking(x => x.NotificationAttachment,
                                    x => x.NotificationAttachment.Select(y => y.AttachmentTransaction))
                .Where(x => x.IsActive == true && x.StartDate <= DateTime.Now.AddMonths(1)).AsQueryable();
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
                .Where(x => x.IsActive == true && x.StartDate <= DateTime.Now.AddMonths(1)  
                        && !x.NotificationUser.Any(x => x.Username == username && x.IsRead == true)).OrderByDescending(x => x.StartDate).FirstAsync();

            return Mapper.Map<NotificationConfigurationDTO>(result);
        }
        public async Task<bool> Update(NotificationConfigurationDTO mod)
        {
            // 1. Fetch existing record with attachments, users, and the actual file transactions
            var existing = await _repository.GetAllIncluding(
                    x => x.NotificationAttachment,
                    x => x.NotificationUser,
                    x => x.NotificationAttachment.Select(y => y.AttachmentTransaction)
                ).FirstOrDefaultAsync(x => x.Id == mod.Id);

            if (existing == null) return false;

            // 2. Clear NotificationUser records (reset read status)
            existing.NotificationUser.Clear();

            // 3. Update scalar properties
            existing.TitleAr = mod.TitleAr;
            existing.TitleEn = mod.TitleEn;
            existing.MessageAr = mod.MessageAr;
            existing.MessageEn = mod.MessageEn;
            existing.StartDate = mod.StartDate;
            existing.EndDate = mod.EndDate;
            existing.ActionUrl = mod.ActionUrl;
            existing.ActionTextAr = mod.ActionTextAr;
            existing.ActionTextEn = mod.ActionTextEn;

            // 4. Handle image replacement (Requirement: Max 1 file)
            if (mod.Files != null && mod.Files.Count > 0)
            {
                // A. Delete ALL old attachments associated with this notification (Physical + DB)
                var oldAttachments = existing.NotificationAttachment.ToList();
                foreach (var oldNav in oldAttachments)
                {
                    var trans = oldNav.AttachmentTransaction;
                    if (trans != null)
                    {
                        try
                        {
                            // Delete physical files from disk
                            string fileName = trans.FileId + trans.FileExtension;
                            string thumbName = trans.FileId + "_thumb" + trans.FileExtension;
                            AttachmentService.DeletePhysicalFile(trans.FilePath, fileName);
                            AttachmentService.DeletePhysicalFile(trans.FilePath, thumbName);

                            // Delete the transaction record from the database
                            await AttachmentService.DeleteFile(trans);
                        }
                        catch (Exception) { /* Log error if necessary */ }
                    }
                    existing.NotificationAttachment.Remove(oldNav);
                }

                // B. Upload and attach the NEW file (strictly pick only the first one if multiple were sent)
                var newFiles = mod.Files.Take(1).ToList();
                var attachmentList = await AttachmentService.UploadAttachment(newFiles);
                foreach (var attach in attachmentList)
                {
                    existing.NotificationAttachment.Add(new NotificationAttachment
                    {
                        AttachmentId = attach.Id,
                        NotificationConfigurationId = existing.Id
                    });
                }
            }

            // 5. Save all changes in-place
            await _repository.UpdateAsync(existing, true);

            return true;
        }



        public async Task<bool> Delete(int id)
        { 
            var notification = await _repository.GetAllIncludingNoTracking(
                    x => x.NotificationAttachment,
                    x => x.NotificationAttachment.Select(y => y.AttachmentTransaction)
                ).FirstOrDefaultAsync(x => x.Id == id);

             
            if (notification == null) return false;           

            foreach (var nav in notification.NotificationAttachment)
            {
                var trans = nav.AttachmentTransaction;
                if (trans != null)
                {
                    try
                    { 
                        //await AttachmentService.DeleteFile(trans);

                        // Delete Physical Files
                        string fileName = trans.FileId + trans.FileExtension;
                        string thumbName = trans.FileId + "_thumb" + trans.FileExtension;

                        AttachmentService.DeletePhysicalFile(trans.FilePath, fileName);
                        AttachmentService.DeletePhysicalFile(trans.FilePath, thumbName);
                    }
                    catch (Exception ex)
                    { 
                    }
                }
            }

            // 3. Delete the parent record last
            await _repository.DeleteAsync(notification, true);

            return true;
        }
         
    }
}
