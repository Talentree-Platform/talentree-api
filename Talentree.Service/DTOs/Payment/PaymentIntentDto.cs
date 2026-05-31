namespace Talentree.Service.DTOs.Payment
{
    /// <summary>
    /// Returned after creating a Stripe PaymentIntent.
    /// Pass ClientSecret to Stripe.js — it never leaves the client session.
    /// </summary>
    public class PaymentIntentDto
    {
        /// <summary>
        /// Pass this to stripe.confirmCardPayment() on the frontend.
        /// Do not log or store this value.
        /// </summary>
        public string ClientSecret { get; set; } = null!;

        /// <summary>Stripe PaymentIntent ID — store on the frontend for tracking.</summary>
        public string PaymentIntentId { get; set; } = null!;

        /// <summary>Amount in EGP — for display on the payment page.</summary>
        public decimal Amount { get; set; }

        /// <summary>Currency — always "egp" for MVP.</summary>
        public string Currency { get; set; } = "egp";
    }
}