using DigitalHub.Services.DTO;

namespace DigitalHub.Services.Services.IconConfig
{
    public interface INotificationConfigurationService
    {
        Task<bool> Add(NotificationConfigurationDTO model);
        Task<bool> Delete(int id);
        Task<List<NotificationConfigurationDTO>> Get(int id = 0);
        Task<bool> Update(NotificationConfigurationDTO mod);
    }
}