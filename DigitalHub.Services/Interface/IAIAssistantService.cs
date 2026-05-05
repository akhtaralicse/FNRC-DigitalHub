using Microsoft.AspNetCore.Http;
using DigitalHub.Services.DTO.AIAssistant;

namespace DigitalHub.Services.Interface
{
    public interface IAIAssistantService
    {
        Task<bool> UploadDocuments(List<IFormFile> files, string uploaderName, string language = "Mixed", string category = "General", DateTime? expiryDate = null);
        Task<ChatResponseDTO> Chat(string query, string lang, string sessionId, string fullName);
        Task<List<AIAssistantDocumentDTO>> GetDocuments(string search);
        Task<bool> DeleteDocument(int id);
        Task<bool> ToggleDocumentStatus(int id);
        Task<List<AIChatLogDTO>> GetChatLogs(string search, bool? onlyNegative);
        Task<(List<AIChatLogDTO> logs, int totalCount)> GetChatLogsPaged(string search, bool? onlyNegative, int page, int pageSize);
        Task<bool> SubmitFeedback(FeedbackRequestDTO request);
        Task<List<string>> GetInitialSuggestions(string lang);
        Task<AIAnalyticsDTO> GetAnalytics();
        Task<bool> RegenerateQuestions(int id);
        Task<List<AIQAOverrideDTO>> GetQAOverrides();
        Task<bool> AddQAOverride(string question, string answer);
        Task<bool> DeleteQAOverride(int id);

        // Session Management
        Task<List<AIChatSessionDTO>> GetSessions(string fullName);
        Task<AIChatSessionDTO> CreateSession(string fullName);
        Task<bool> UpdateSession(int id, string title, bool isPinned);
        Task<bool> DeleteSession(int id);
        Task<List<AIChatLogDTO>> GetSessionMessages(string sessionId);
    }
}
