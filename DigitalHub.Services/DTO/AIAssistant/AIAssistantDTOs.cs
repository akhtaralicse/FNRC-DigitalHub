namespace DigitalHub.Services.DTO.AIAssistant
{
    public class ChatResponseDTO
    {
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
}
