namespace Talentree.Service.DTOs.Support
{
    public class FAQDto
    {
        public int Id { get; set; }
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public int ViewCount { get; set; }
        public List<FAQDto> RelatedFaqs { get; set; } = new();
    }
}