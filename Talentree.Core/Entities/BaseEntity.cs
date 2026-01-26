namespace Talentree.Core.Entities
{
    /// <summary>
    /// Base class for all domain entities in Talentree
    /// Provides common properties that all entities share
    /// </summary>
    /// <remarks>
    /// This is an abstract class following Domain-Driven Design principles.
    /// All domain entities should inherit from this class to ensure consistency.
    /// </remarks>
    public abstract class BaseEntity
    {
        /// <summary>
        /// Unique identifier for the entity
        /// </summary>
        /// <remarks>
        /// This is the primary key in the database.
        /// Using int for better performance compared to GUID.
        /// </remarks>
        public int Id { get; set; }

        // ===============================
        // Future Enhancements (Commented for now)
        // ===============================
        // These properties can be added when we implement audit functionality

        /// <summary>
        /// Date and time when the entity was created (UTC)
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Date and time when the entity was last updated (UTC)
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// User who created this entity
        /// </summary>
        public string? CreatedBy { get; set; }

        /// <summary>
        /// User who last updated this entity
        /// </summary>
        public string? UpdatedBy { get; set; }

        /// <summary>
        /// Soft delete flag - true if entity is deleted
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Date and time when the entity was deleted (UTC)
        /// </summary>
        public DateTime? DeletedAt { get; set; }
    }
}