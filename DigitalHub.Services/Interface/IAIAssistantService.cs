using Microsoft.AspNetCore.Http;
using DigitalHub.Services.DTO.AIAssistant;

namespace DigitalHub.Services.Interface
{
    public interface IAIAssistantService
    {
        Task<bool> UploadDocuments(List<IFormFile> files, string uploaderName);
        Task<ChatResponseDTO> Chat(string query, string lang, string sessionId, string fullName);
    }
}
