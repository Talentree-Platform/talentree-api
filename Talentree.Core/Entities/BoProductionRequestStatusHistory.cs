using Talentree.Core.Enums;

namespace Talentree.Core.Entities
{
    /// <summary>
    /// An immutable record of a single status change on a <see cref="BoProductionRequest"/>.
    /// Created whenever either the BO or an Admin transitions the request to a new status.
    /// Never updated or deleted — forms the complete audit trail.
    /// </summary>
    public class BoProductionRequestStatusHistory : AuditableEntity
    {
        /// <summary>Foreign key to the parent <see cref="BoProductionRequest"/>.</summary>
        public int BoProductionRequestId { get; set; }

        /// <summary>Navigation property to the parent request.</summary>
        public BoProductionRequest Request { get; set; } = null!;

        /// <summary>The status that was set at this point in time.</summary>
        public BoProductionRequestStatus Status { get; set; }

        /// <summary>
        /// Identity ID of the user who triggered this transition —
        /// either the Business Owner or a Talentree Admin.
        /// </summary>
        public string ChangedByUserId { get; set; } = null!;

        /// <summary>
        /// Optional contextual notes for this transition.
        /// Examples: rejection reason, tracking details, quote summary.
        /// </summary>
        public string? Notes { get; set; }
    }
}
