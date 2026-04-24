using Microsoft.Extensions.Configuration;
using Stripe;
using Talentree.Core;
using Talentree.Core.Entities;
using Talentree.Core.Enums;
using Talentree.Core.Specifications.BoProductionRequests;
using Talentree.Core.Specifications.MaterialOrders;
using Talentree.Core.Specifications.Transactions;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs.Payment;

namespace Talentree.Service.Services
{
    /// <summary>
    /// Stripe integration service.
    /// Creates PaymentIntents, processes webhook events, and writes
    /// Transaction ledger entries when payments succeed.
    /// </summary>
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly PaymentIntentService _stripeIntentService;
        private readonly IConfiguration _configuration;

        public PaymentService(
            IUnitOfWork unitOfWork,
            PaymentIntentService stripeIntentService,
            IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _stripeIntentService = stripeIntentService;
            _configuration = configuration;
        }

        // ── BO endpoints ───────────────────────────────────────

        /// <inheritdoc/>
        public async Task<PaymentIntentDto> CreateMaterialOrderIntentAsync(int orderId, string boId)
        {
            var spec = new MaterialOrderByIdAndBoSpecification(orderId, boId);
            var order = await _unitOfWork.Repository<MaterialOrder>()
                .GetByIdWithSpecificationsAsync(spec)
                ?? throw new KeyNotFoundException($"Order #{orderId} not found.");

            if (order.PaymentStatus == PaymentStatus.Paid)
                throw new InvalidOperationException("This order has already been paid.");

            // Return existing usable intent if present — handles page refresh / retry
            if (!string.IsNullOrEmpty(order.StripePaymentIntentId))
            {
                var existing = await _stripeIntentService.GetAsync(order.StripePaymentIntentId);
                if (IsIntentReusable(existing))
                    return ToDto(existing, order.TotalAmount);
            }

            var intent = await _stripeIntentService.CreateAsync(new PaymentIntentCreateOptions
            {
                Amount = ToStripeAmount(order.TotalAmount),
                Currency = "egp",
                Metadata = new Dictionary<string, string>
                {
                    { "type",    "material_order" },
                    { "orderId", orderId.ToString() },
                    { "boId",    boId }
                }
            });

            order.StripePaymentIntentId = intent.Id;
            await _unitOfWork.CompleteAsync();

            return ToDto(intent, order.TotalAmount);
        }

        /// <inheritdoc/>
        public async Task<PaymentIntentDto> CreateProductionRequestIntentAsync(int requestId, string boId)
        {
            var spec = new ProductionRequestByIdAndBoSpecification(requestId, boId);
            var request = await _unitOfWork.Repository<BoProductionRequest>()
                .GetByIdWithSpecificationsAsync(spec)
                ?? throw new KeyNotFoundException($"Production request #{requestId} not found.");

            if (request.Status != BoProductionRequestStatus.Quoted)
                throw new InvalidOperationException(
                    "Payment is only available for requests that Talentree has quoted.");

            if (!request.QuotedPrice.HasValue)
                throw new InvalidOperationException("No quoted price is set on this request.");

            if (request.PaymentStatus == PaymentStatus.Paid)
                throw new InvalidOperationException("This request has already been paid.");

            if (!string.IsNullOrEmpty(request.StripePaymentIntentId))
            {
                var existing = await _stripeIntentService.GetAsync(request.StripePaymentIntentId);
                if (IsIntentReusable(existing))
                    return ToDto(existing, request.QuotedPrice.Value);
            }

            var intent = await _stripeIntentService.CreateAsync(new PaymentIntentCreateOptions
            {
                Amount = ToStripeAmount(request.QuotedPrice.Value),
                Currency = "egp",
                Metadata = new Dictionary<string, string>
                {
                    { "type",      "production_request" },
                    { "requestId", requestId.ToString() },
                    { "boId",      boId }
                }
            });

            request.StripePaymentIntentId = intent.Id;
            await _unitOfWork.CompleteAsync();

            return ToDto(intent, request.QuotedPrice.Value);
        }

        // ── Webhook handler ────────────────────────────────────

        /// <inheritdoc/>
        public async Task HandleWebhookAsync(string payload, string stripeSignature)
        {
            var webhookSecret = _configuration["Stripe:WebhookSecret"]
                ?? throw new InvalidOperationException("Stripe:WebhookSecret is not configured.");

            Event stripeEvent;
            try
            {
                stripeEvent = EventUtility.ConstructEvent(payload, stripeSignature, webhookSecret);
            }
            catch (StripeException ex)
            {
                throw new InvalidOperationException($"Webhook signature invalid: {ex.Message}");
            }

            if (stripeEvent.Data.Object is not PaymentIntent intent) return;

            switch (stripeEvent.Type)
            {
                case "payment_intent.succeeded":
                    await HandleSucceededAsync(intent);
                    break;

                case "payment_intent.payment_failed":
                    await HandleFailedAsync(intent);
                    break;
            }
        }

        // ── Private helpers ────────────────────────────────────

        /// <summary>
        /// On payment success:
        /// - Marks the entity as Paid
        /// - For production requests: transitions to Confirmed + appends history entry
        /// - Creates a Transaction ledger entry
        /// </summary>
        private async Task HandleSucceededAsync(PaymentIntent intent)
        {
            // Try MaterialOrder first
            var orderSpec = new MaterialOrderByPaymentIntentSpecification(intent.Id);
            var order = await _unitOfWork.Repository<MaterialOrder>()
                .GetByIdWithSpecificationsAsync(orderSpec);

            if (order is not null)
            {
                order.PaymentStatus = PaymentStatus.Paid;

                await CreateTransactionAsync(
                    boId: order.BusinessOwnerId,
                    type: TransactionType.MaterialPurchase,
                    amount: -order.TotalAmount, // debit — BO spent money
                    description: $"Raw material order #{order.Id}",
                    referenceId: order.Id,
                    referenceType: "MaterialOrder",
                    intentId: intent.Id);

                // TODO: send BO notification — order payment confirmed
                return;
            }

            // Try ProductionRequest
            var reqSpec = new ProductionRequestByPaymentIntentSpecification(intent.Id);
            var request = await _unitOfWork.Repository<BoProductionRequest>()
                .GetByIdWithSpecificationsAsync(reqSpec);

            if (request is not null)
            {
                request.PaymentStatus = PaymentStatus.Paid;
                request.Status = BoProductionRequestStatus.Confirmed;
                request.StatusHistory.Add(new BoProductionRequestStatusHistory
                {
                    Status = BoProductionRequestStatus.Confirmed,
                    ChangedByUserId = "stripe-webhook",
                    Notes = $"Quote confirmed — payment received via Stripe (Intent: {intent.Id}).",
                    CreatedAt = DateTime.UtcNow
                });

                await CreateTransactionAsync(
                    boId: request.BusinessOwnerId,
                    type: TransactionType.ProductionRequest,
                    amount: -request.QuotedPrice!.Value, // debit
                    description: $"Production request #{request.Id} — {request.Title}",
                    referenceId: request.Id,
                    referenceType: "ProductionRequest",
                    intentId: intent.Id);

                // TODO: notify BO (confirmed) + Admin (ready to start production)
            }
        }

        /// <summary>
        /// On payment failure:
        /// - Marks entity as Failed
        /// - Clears the PaymentIntentId so the BO can retry with a fresh intent
        /// </summary>
        private async Task HandleFailedAsync(PaymentIntent intent)
        {
            var orderSpec = new MaterialOrderByPaymentIntentSpecification(intent.Id);
            var order = await _unitOfWork.Repository<MaterialOrder>()
                .GetByIdWithSpecificationsAsync(orderSpec);

            if (order is not null)
            {
                order.PaymentStatus = PaymentStatus.Failed;
                order.StripePaymentIntentId = null; // allow retry
                await _unitOfWork.CompleteAsync();
                return;
            }

            var reqSpec = new ProductionRequestByPaymentIntentSpecification(intent.Id);
            var request = await _unitOfWork.Repository<BoProductionRequest>()
                .GetByIdWithSpecificationsAsync(reqSpec);

            if (request is not null)
            {
                request.PaymentStatus = PaymentStatus.Failed;
                request.StripePaymentIntentId = null; // allow retry
                await _unitOfWork.CompleteAsync();
            }
        }

        /// <summary>
        /// Creates a Transaction ledger entry and updates the BO's running balance.
        /// Called after every successful payment.
        /// </summary>
        private async Task CreateTransactionAsync(
            string boId,
            TransactionType type,
            decimal amount,
            string description,
            int referenceId,
            string referenceType,
            string intentId)
        {
            // Get current balance (last transaction's BalanceAfter for this BO)
            var lastTxSpec = new TransactionsByBoSpecification(boId, null, 1, 1);
            var lastTxList = await _unitOfWork.Repository<Transaction>()
                .GetAllWithSpecificationsAsync(lastTxSpec);

            var currentBalance = lastTxList.FirstOrDefault()?.BalanceAfter ?? 0m;
            var newBalance = currentBalance + amount;

            var transaction = new Transaction
            {
                BusinessOwnerId = boId,
                Type = type,
                Description = description,
                Amount = amount,
                BalanceAfter = newBalance,
                ReferenceId = referenceId,
                ReferenceType = referenceType,
                StripePaymentIntentId = intentId,
                CreatedAt = DateTime.UtcNow
            };

            _unitOfWork.Repository<Transaction>().Add(transaction);
            await _unitOfWork.CompleteAsync();
        }

        /// <summary>
        /// Converts EGP decimal to Stripe's smallest unit (piastres).
        /// 45.50 EGP → 4550 piastres.
        /// </summary>
        private static long ToStripeAmount(decimal amount)
            => (long)Math.Round(amount * 100, MidpointRounding.AwayFromZero);

        /// <summary>
        /// A PaymentIntent is reusable if the BO hasn't attempted payment yet or it requires action.
        /// Creating a new intent for an already-usable one would result in a duplicate charge attempt.
        /// </summary>
        private static bool IsIntentReusable(PaymentIntent intent)
            => intent.Status is "requires_payment_method"
                             or "requires_confirmation"
                             or "requires_action";

        /// <summary>Maps a Stripe PaymentIntent to the response DTO.</summary>
        private static PaymentIntentDto ToDto(PaymentIntent intent, decimal amount)
            => new()
            {
                ClientSecret = intent.ClientSecret,
                PaymentIntentId = intent.Id,
                Amount = amount,
                Currency = "egp"
            };
    }
}