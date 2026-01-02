using DigitalHub.Services.DTO;

namespace DigitalHub.Services.Services.IconConfig
{
    public interface INotificationConfigurationService
    {
        Task<bool> Add(NotificationConfigurationDTO model);
        Task<bool> AddUserNotification(NotificationUserDTO model);
        Task<bool> Delete(int id); 
        Task<List<NotificationConfigurationDTO>> Get(string username = null);
        Task<List<NotificationConfigurationDTO>> GetAll();
        Task<bool> Update(NotificationConfigurationDTO mod);
    }
}