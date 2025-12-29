using DigitalHub.Domain.Enums;
using DigitalHub.Services.DTO;

namespace DigitalHub.Services.Interface
{
    public interface IIconConfigurationService
    {
        Task<bool> Add(IconConfigurationDTO mod);
        Task<bool> Delete(int id); 
        Task<List<IconConfigurationDTO>> Get(int id = 0);
        Task<List<IconConfigurationDTO>> GetByType(IconTypeEnum type);
        Task<bool> Update(IconConfigurationDTO mod);
    }
}