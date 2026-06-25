using Talentree.Core.Enums;

namespace Talentree.Core.Entities
{
    /// <summary>
    /// FR-AD-36: Versioned legal/policy document.
    /// Each publish call creates a new row; previous versions are preserved for audit.
    /// Only one row per DocumentType should have IsPublished = true at any time.
    /// </summary>
    public class PlatformPolicy : AuditableEntity
    {
        /// <summary>Identifies which legal document this row represents.</summary>
        public PolicyDocumentType DocumentType { get; set; }

        /// <summary>
        /// Full rich-text HTML content of the document.
        /// Stored without length limit (nvarchar(max) / TEXT column).
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>Auto-incremented version number within a DocumentType.</summary>
        public int VersionNumber { get; set; }

        /// <summary>
        /// When true, this is the currently active version shown to users.
        /// Guaranteed unique per DocumentType via application logic.
        /// </summary>
        public bool IsPublished { get; set; }

        /// <summary>
        /// When true, users who have not yet accepted this version will be prompted
        /// to accept before proceeding.
        /// </summary>
        public bool RequireUserAcceptance { get; set; }

        /// <summary>UTC timestamp when this version was published. Null for drafts.</summary>
        public DateTime? PublishedAt { get; set; }
    }
}
