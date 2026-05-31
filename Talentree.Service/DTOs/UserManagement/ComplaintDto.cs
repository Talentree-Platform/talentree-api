using Talentree.Core.Enums;

namespace Talentree.Service.DTOs.UserManagement
{
    public class ComplaintDto
    {
        public int Id { get; set; }

        public string ReportedUserId { get; set; } = string.Empty;
        public string ReportedUserName { get; set; } = string.Empty;
        public string ReportedUserEmail { get; set; } = string.Empty;

        public string ReportedByUserId { get; set; } = string.Empty;
        public string ReportedByName { get; set; } = string.Empty;

        public ViolationType ViolationType { get; set; }
        public string ViolationTypeText { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public ComplaintStatus Status { get; set; }
        public string StatusText { get; set; } = string.Empty;

        public string? RelatedOrderId { get; set; }
        public string? RelatedProductId { get; set; }

        public string? ReviewedBy { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? Resolution { get; set; }
        public string? AdminNotes { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}