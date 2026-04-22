namespace FNRC_DigitalHub.Models
{
    public class UnifiedNewsViewModel
    {
        public string Id { get; set; }
        public string TitleEn { get; set; }
        public string TitleAr { get; set; }
        public string DescriptionEn { get; set; }
        public string DescriptionAr { get; set; }
        public string ImageUrl { get; set; }
        public DateTime PublishDate { get; set; }
        public string Source { get; set; } // Useful to know where the news came from
    }
}
