namespace Talentree.Core.Entities
{
    public abstract class BaseEntity
    {
        public int Id { get; set; }
    }


    // ============================================================
    public abstract class AuditableEntity : BaseEntity
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }


    // ============================================================
    public interface ISoftDelete
    {
        bool IsDeleted { get; set; }
        DateTime? DeletedAt { get; set; }
        string? DeletedBy { get; set; }
    }
}