 
using AutoMapper;
using DigitalHub.Services.Interface;
using DigitalHub.Services.Services.Attachment;
using DigitalHub.Services.Services.IconConfig; 
using DigitalHub.Services.Services.AIAssistant;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection; 

namespace DigitalHub.Services.Shared
{
    public static class ServiceBuilderExtension
    {
        public static void CustomServicesBuilder(this IServiceCollection services, IConfiguration Configuration)
        {
            services.AddLogging();
            services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = Convert.ToInt32(Configuration["FileServer:AllowedFileSize"]);
            });
           
            //services.AddHttpClient();
            services.AddMemoryCache();
            //services.AddScoped<IHttpClientFactory>(); 

            services.AddHttpContextAccessor();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
           // services.AddScoped<LanguageContext>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
           // services.AddMapperProfile(Configuration);
            services.GenericServiceDataBuilder();
         
        }
        public static void GenericServiceDataBuilder(this IServiceCollection services)
        {
             services.AddScoped<IIconConfigurationService, IconConfigurationService>();
             services.AddScoped<IAttachmentService, AttachmentService>();
             services.AddScoped<INotificationConfigurationService, NotificationConfigurationService>();
             services.AddScoped<IIconConfigurationAttachmentService, IconConfigurationAttachmentService>();
             services.AddScoped<IAIAssistantService, AIAssistantService>();

        }

        public static IServiceCollection AddMapperProfile(this IServiceCollection services, IConfiguration Configuration)
        {
            // Automatically scans and registers all profiles in the assembly
            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile(new ServicesMapper(Configuration));
            }, typeof(ServiceBuilderExtension).Assembly);

            return services;
        }
    }
}
