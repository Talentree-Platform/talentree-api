namespace Talentree.Core.Entities
{
    public class FAQ : AuditableEntity, ISoftDelete
    {
        public string Question { get; set; } = null!;
        public string Answer { get; set; } = null!;
        public string Category { get; set; } = null!;

        public int DisplayOrder { get; set; } = 0;
        public bool IsPublished { get; set; } = true;
        public int ViewCount { get; set; } = 0;

        public string? Keywords { get; set; }
        public string? RelatedFaqIds { get; set; }

        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
    }
}