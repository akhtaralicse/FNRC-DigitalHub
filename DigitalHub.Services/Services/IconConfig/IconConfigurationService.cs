using AutoMapper;
using DigitalHub.Domain.Domains;
using DigitalHub.Domain.Enums;
using DigitalHub.Services.DTO;
using DigitalHub.Services.Interface;
using DigitalHub.Services.Services.Attachment;
using DigitalHub.Services.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DigitalHub.Services.Services.IconConfig
{
    public class IconConfigurationService(IGenericRepository<IconConfiguration> repository, IConfiguration configuration,
        IMapper mapper, IAttachmentService attachmentService) : IIconConfigurationService
    {
        public IGenericRepository<IconConfiguration> _repository { get; } = repository;
        public IConfiguration Configuration { get; } = configuration;
        public IMapper Mapper { get; } = mapper;
        public IAttachmentService AttachmentService { get; } = attachmentService;

        public async Task<bool> Add(IconConfigurationDTO model)
        {

            if (model.Files != null && model.Files.Count > 0)
            {
                var attachmentList = await AttachmentService.UploadAttachment(model.Files);

                foreach (var attach in attachmentList)
                {
                    model.IconConfigurationAttachments.Add(new IconConfigurationAttachmentDTO
                    {
                        AttachmentId = attach.Id,
                    });
                }
            }

            var result = Mapper.Map<IconConfiguration>(model);
            await _repository.InsertAsync(result, true);

            return true;
        }

        public async Task<List<IconConfigurationDTO>> Get(int id = 0)
        {
            var result = _repository.GetAllIncludingNoTracking(x => x.IconConfigurationAttachments,
                                    x => x.IconConfigurationAttachments.Select(y => y.AttachmentTransaction)).Where(x => x.IsActive == true).AsQueryable();
            if (id > 0)
            {
                result = result.Where(x => x.Id == id);
            }

            return Mapper.Map<List<IconConfigurationDTO>>(await result.OrderBy(x => x.OrderNo).ToListAsync());
        }
        public async Task<List<IconConfigurationDTO>> GetByType(IconTypeEnum type)
        {
            var result = _repository.GetAllIncludingNoTracking(x => x.IconConfigurationAttachments,
                                    x => x.IconConfigurationAttachments.Select(y => y.AttachmentTransaction))
                        .Where(x => x.IsActive == true && x.IconType == type).AsQueryable();

            return Mapper.Map<List<IconConfigurationDTO>>(await result.OrderBy(x => x.OrderNo).ToListAsync());
        }
        public async Task<bool> Update(IconConfigurationDTO mod)
        {
            var result = await _repository.GetAllIncludingNoTracking(x => x.IconConfigurationAttachments).FirstOrDefaultAsync(x => x.Id == mod.Id);

            if (mod.Files != null && mod.Files.Count > 0)
            {
                var attachmentList = await AttachmentService.UploadAttachment(mod.Files);

                foreach (var attach in attachmentList)
                {
                    result.IconConfigurationAttachments.Add(new()
                    {
                        AttachmentId = attach.Id,
                    });
                }
            }

            //var data = Mapper.Map<IconConfiguration>(result);
            result.DescriptionEn = mod.DescriptionEn;
            result.DescriptionAr = mod.DescriptionAr;
            result.NameAr = mod.NameAr;
            result.NameEn = mod.NameEn;
            result.OrderNo = mod.OrderNo;
            result.URL = mod.URL;

            _repository.Update(result, true);

            return true;
        }

        public async Task<bool> Delete(int id)
        {
            var result = await _repository.GetAllIncludingNoTracking().FirstOrDefaultAsync(x => x.Id == id);

            _repository.Delete(result, true);

            return true;
        }
    }
}
