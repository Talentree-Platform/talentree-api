namespace Talentree.Core.Enums
{
    /// <summary>
    /// Payment state for a MaterialOrder or BoProductionRequest.
    /// Driven exclusively by Stripe webhook events — never set manually.
    /// </summary>
    public enum PaymentStatus
    {
        /// <summary>Created but payment not yet initiated or confirmed.</summary>
        Unpaid = 0,

        /// <summary>Stripe confirmed payment_intent.succeeded — funds received.</summary>
        Paid = 1,

        /// <summary>Stripe reported payment_intent.payment_failed.</summary>
        Failed = 2,

        /// <summary>Refund processed through Stripe after a successful payment.</summary>
        Refunded = 3
    }
}