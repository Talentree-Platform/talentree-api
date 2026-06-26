using Talentree.Core.Enums;

namespace Talentree.Service.DTOs.PlatformSettings
{
    // ═══════════════════════════════════════════════════════════
    // FR-AD-36: Terms & Policies DTOs
    // ═══════════════════════════════════════════════════════════

    /// <summary>Full policy document including rich-text content.</summary>
    public class PolicyDocumentDto
    {
        public int Id { get; set; }
        public PolicyDocumentType DocumentType { get; set; }
        public string DocumentTypeName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int VersionNumber { get; set; }
        public bool IsPublished { get; set; }
        public bool RequireUserAcceptance { get; set; }
        public DateTime? PublishedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
    }

    /// <summary>Lightweight summary without the full content body — for list views.</summary>
    public class PolicyDocumentSummaryDto
    {
        public int Id { get; set; }
        public PolicyDocumentType DocumentType { get; set; }
        public string DocumentTypeName { get; set; } = string.Empty;
        public int VersionNumber { get; set; }
        public bool IsPublished { get; set; }
        public bool RequireUserAcceptance { get; set; }
        public DateTime? PublishedAt { get; set; }
    }

    /// <summary>Write model for creating a new version of a policy document.</summary>
    public class UpdatePolicyDocumentDto
    {
        public string Content { get; set; } = string.Empty;

        /// <summary>When true, existing users will be prompted to accept this new version.</summary>
        public bool RequireUserAcceptance { get; set; }
    }

    /// <summary>Single entry in a policy's version history list.</summary>
    public class PolicyVersionHistoryDto
    {
        public int Id { get; set; }
        public int VersionNumber { get; set; }
        public bool IsPublished { get; set; }
        public DateTime? PublishedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
    }
}
