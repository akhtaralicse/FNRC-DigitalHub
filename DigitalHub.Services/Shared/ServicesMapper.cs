using AutoMapper;
using DigitalHub.Domain.Domains;
using DigitalHub.Services.DTO;
using Microsoft.Extensions.Configuration;

namespace DigitalHub.Services.Shared
{
    public class ServicesMapper : Profile
    {
        public IConfiguration Configuration { get; }
        public ServicesMapper(IConfiguration configuration)
        {
            Configuration = configuration;
            AttachmentMapper();
            CreateMap<IconConfiguration, IconConfigurationDTO>().ReverseMap();
            
            CreateMap<IconConfigurationAttachment, IconConfigurationAttachmentDTO>().ReverseMap();
            CreateMap<Users, UsersDTO>().ReverseMap(); 
            CreateMap<NotificationConfiguration,NotificationConfigurationDTO>().ReverseMap(); 
            CreateMap<NotificationUser, NotificationUserDTO>().ReverseMap(); 
            CreateMap<NotificationAttachment,NotificationAttachmentDTO>().ReverseMap(); 


        }



        private void AttachmentMapper()
        {
            var FileURL = Configuration["FileServer:FileURL"];

            CreateMap<AttachmentTransaction, AttachmentTransactionDTO>()
                 .ForMember(dest => dest.FilePath,
                                opt => opt.MapFrom(src => Path.Combine(FileURL, src.FilePath, src.FileId + src.FileExtension).Replace("\\", "/")))

                 .ForMember(dest => dest.ThumbPath,
                    opt => opt.MapFrom(src => src.IsThumb ? Path.Combine(FileURL, src.FilePath, src.FileId + "_thumb" + src.FileExtension).Replace("\\", "/") : ""))

                 .ForMember(dest => dest.FileFolder,
                    opt => opt.MapFrom(src => src.FilePath))

                .ReverseMap();
        }
    }
}
