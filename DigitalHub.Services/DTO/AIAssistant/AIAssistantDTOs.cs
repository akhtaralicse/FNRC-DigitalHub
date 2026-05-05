namespace DigitalHub.Services.DTO.AIAssistant
{
    public class ChatResponseDTO
    {
        public int LogId { get; set; }
        public string ResponseText { get; set; }
        public List<string> Sources { get; set; }
        public List<string> RecommendedQuestions { get; set; }
    }


    public class ChatRequestDTO
    {
        public string Query { get; set; }
        public string Lang { get; set; } // AR or EN
        public string SessionId { get; set; }
    }

    public class AIAssistantDocumentDTO
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string UploaderName { get; set; }
        public long FileSize { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Category { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsActive { get; set; }
    }

    public class AIChatLogDTO
    {
        public int Id { get; set; }
        public string SessionId { get; set; }
        public string FullName { get; set; }
        public string UserQuery { get; set; }
        public string BotResponse { get; set; }
        public DateTime Timestamp { get; set; }
        public string Language { get; set; }
        public bool? IsPositive { get; set; }
        public string FeedbackComment { get; set; }
    }

    public class FeedbackRequestDTO
    {
        public int LogId { get; set; }
        public bool IsPositive { get; set; }
        public string Comment { get; set; }
    }

    public class AIChatSessionDTO
    {
        public int Id { get; set; }
        public string SessionId { get; set; }
        public string Title { get; set; }
        public string UserFullName { get; set; }
        public bool IsPinned { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class AIQAOverrideDTO
    {
        public int Id { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
        public bool IsActive { get; set; }
    }

    public class AIAnalyticsDTO
    {
        public int TotalDocuments { get; set; }
        public int TotalChats { get; set; }
        public int NegativeFeedbacks { get; set; }
        public List<string> TopTrendingKeywords { get; set; }
        public List<AIChatLogDTO> UnansweredQueries { get; set; }
    }
}
