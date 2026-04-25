using Talentree.Service.DTOs.Payment;

namespace Talentree.Service.Contracts
{
    /// <summary>
    /// Handles Stripe PaymentIntent creation and webhook processing
    /// for Material Orders and Production Requests.
    /// </summary>
    public interface IPaymentService
    {
        /// <summary>
        /// Creates a Stripe PaymentIntent for a material order.
        /// If an active intent already exists on the order, returns it instead.
        /// The order must be Unpaid. Returns a clientSecret for Stripe.js.
        /// </summary>
        Task<PaymentIntentDto> CreateMaterialOrderIntentAsync(int orderId, string boId);

        /// <summary>
        /// Creates a Stripe PaymentIntent for a production request's quoted price.
        /// The request must be in Quoted status. Paying this intent replaces
        /// the manual confirm step — webhook transitions to Confirmed automatically.
        /// </summary>
        Task<PaymentIntentDto> CreateProductionRequestIntentAsync(int requestId, string boId);

        /// <summary>
        /// Processes a raw Stripe webhook event.
        /// Handles: payment_intent.succeeded → marks Paid, creates Transaction.
        ///          payment_intent.payment_failed → marks Failed, clears intent for retry.
        /// </summary>
        Task HandleWebhookAsync(string payload, string stripeSignature);
    }
}