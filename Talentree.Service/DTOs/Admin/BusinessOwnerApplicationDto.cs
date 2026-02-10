
using Talentree.Core.Enums;

namespace Talentree.Service.DTOs.Admin
{
    /// <summary>
    /// DTO for displaying pending business owner applications
    /// </summary>
    public class BusinessOwnerApplicationDto
    {
        public int ProfileId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;

        // Business info
        public string BusinessName { get; set; } = string.Empty;
        public string BusinessCategory { get; set; } = string.Empty;
        public string BusinessDescription { get; set; } = string.Empty;
        public string? BusinessAddress { get; set; }
        public string? TaxId { get; set; }

        // Social links
        public string? FacebookLink { get; set; }
        public string? InstagramLink { get; set; }
        public string? WebsiteLink { get; set; }


        // ⭐ Approval info
        public ApprovalStatus Status { get; set; }  //  Added!
        public string StatusText { get; set; } = string.Empty;  // Human-readable
        public DateTime SubmittedAt { get; set; }
        public DateTime? AutoApprovalDeadline { get; set; }
        public bool WillAutoApprove { get; set; }
        public TimeSpan? TimeUntilAutoApproval { get; set; }

        // ⭐ Additional approval details
        public DateTime? ApprovedAt { get; set; }  //  When approved/rejected
        public string? ApprovedBy { get; set; }    //  Who approved/rejected
        public string? RejectionReason { get; set; }  //  Why rejected
        
    }
}