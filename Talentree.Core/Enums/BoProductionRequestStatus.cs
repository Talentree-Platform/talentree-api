namespace Talentree.Core.Enums
{
    /// <summary>
    /// Lifecycle status of a Business Owner's production service request.
    ///
    /// BO-driven transitions:
    ///   → Submitted   : BO submits the form
    ///   → Confirmed   : BO accepts Talentree's quote
    ///   → Cancelled   : BO withdraws the request (allowed before InProduction)
    ///
    /// Admin-driven transitions:
    ///   Submitted   → UnderReview  : Admin opens the request
    ///   UnderReview → Quoted       : Admin sends price + ETA to BO
    ///   Confirmed   → InProduction : Admin starts manufacturing
    ///   InProduction→ Completed    : Admin marks goods as ready
    ///   (any)       → Rejected     : Admin declines (not allowed after InProduction)
    /// </summary>
    public enum BoProductionRequestStatus
    {
        /// <summary>BO has submitted the request — it is in Talentree's inbox.</summary>
        Submitted = 0,

        /// <summary>A Talentree Admin is reviewing the request.</summary>
        UnderReview = 1,

        /// <summary>Talentree has sent a price quote and estimated timeline to the BO.</summary>
        Quoted = 2,

        /// <summary>BO accepted the quote — Talentree will proceed with production.</summary>
        Confirmed = 3,

        /// <summary>Talentree has started manufacturing the requested goods.</summary>
        InProduction = 4,

        /// <summary>Production is finished — goods are ready for the Business Owner.</summary>
        Completed = 5,

        /// <summary>
        /// Talentree declined the request.
        /// The rejection reason is stored in <c>BoProductionRequest.AdminNotes</c>.
        /// </summary>
        Rejected = 6,

        /// <summary>
        /// Request was cancelled — either by the BO before production started,
        /// or by Admin before InProduction.
        /// </summary>
        Cancelled = 7
    }
}