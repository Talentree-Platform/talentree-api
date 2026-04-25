using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Talentree.API.Models;
using Talentree.Service.Contracts;

namespace Talentree.API.Controllers
{
    /// <summary>
    /// Handles Stripe PaymentIntent creation for Business Owners
    /// and processes inbound Stripe webhook events.
    /// </summary>
    public class PaymentController : BaseApiController
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
            => _paymentService = paymentService;

        /// <summary>
        /// Step 1 of the material order payment flow.
        /// Creates a Stripe PaymentIntent and returns a clientSecret for Stripe.js.
        /// </summary>
        /// <remarks>
        /// Full flow:
        ///   1. POST /api/material-orders/checkout  → get orderId
        ///   2. POST /api/payment/material-orders/{orderId}/create-intent  → get clientSecret
        ///   3. Frontend: stripe.confirmCardPayment(clientSecret, { payment_method: {...} })
        ///   4. Stripe webhook → order PaymentStatus = Paid
        /// </remarks>
        [HttpPost("material-orders/{orderId:int}/create-intent")]
        [Authorize(Roles = "BusinessOwner")]
        public async Task<IActionResult> CreateMaterialOrderIntent(int orderId)
        {
            var result = await _paymentService.CreateMaterialOrderIntentAsync(orderId, GetBoId());
            return Ok(ApiResponse<object>.SuccessResponse(result));
        }

        /// <summary>
        /// Step 1 of the production request payment flow.
        /// Only available once Talentree has quoted (status = Quoted).
        /// Paying this intent replaces the manual confirm-quote step.
        /// </summary>
        /// <remarks>
        /// Full flow:
        ///   1. Admin sends quote → request status = Quoted
        ///   2. POST /api/payment/production-requests/{id}/create-intent → get clientSecret
        ///   3. Frontend: stripe.confirmCardPayment(clientSecret, { payment_method: {...} })
        ///   4. Stripe webhook → PaymentStatus = Paid, Status = Confirmed
        /// </remarks>
        [HttpPost("production-requests/{requestId:int}/create-intent")]
        [Authorize(Roles = "BusinessOwner")]
        public async Task<IActionResult> CreateProductionRequestIntent(int requestId)
        {
            var result = await _paymentService.CreateProductionRequestIntentAsync(requestId, GetBoId());
            return Ok(ApiResponse<object>.SuccessResponse(result));
        }

        /// <summary>
        /// Stripe webhook receiver — called directly by Stripe, not by the frontend.
        /// Must be publicly accessible (no auth). Verifies the Stripe-Signature header.
        ///
        /// Register this URL in Stripe Dashboard → Developers → Webhooks:
        ///   https://yourdomain.com/api/payment/webhook
        ///
        /// Events to enable:
        ///   - payment_intent.succeeded
        ///   - payment_intent.payment_failed
        /// </summary>
        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> Webhook()
        {
            string payload;
            using (var reader = new StreamReader(Request.Body))
                payload = await reader.ReadToEndAsync();

            var sig = Request.Headers["Stripe-Signature"].FirstOrDefault();

            if (string.IsNullOrEmpty(sig))
                return BadRequest("Missing Stripe-Signature header.");

            try
            {
                await _paymentService.HandleWebhookAsync(payload, sig);
                return Ok();
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("signature"))
            {
                return BadRequest(ex.Message);
            }
            catch
            {
                // Return 200 so Stripe doesn't retry — log the error in production
                return Ok();
            }
        }

        private string GetBoId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    }
}