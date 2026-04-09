namespace Talentree.Core.Enums
{
    /// <summary>Lifecycle status of a BO payout request.</summary>
    public enum PayoutStatus
    {
        /// <summary>BO submitted the request — awaiting Admin review.</summary>
        Pending = 0,

        /// <summary>Admin approved — queued for processing.</summary>
        Approved = 1,

        /// <summary>Transfer initiated to BO's bank account.</summary>
        Processing = 2,

        /// <summary>Funds successfully transferred to BO.</summary>
        Completed = 3,

        /// <summary>Admin rejected the request — reason stored in RejectionReason.</summary>
        Rejected = 4
    }
}