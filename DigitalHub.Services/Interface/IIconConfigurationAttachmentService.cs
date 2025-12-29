
namespace DigitalHub.Services.Services.IconConfig
{
    public interface IIconConfigurationAttachmentService
    {
        Task<bool> DeleteAttachment(int id);
    }
}