using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Talentree.Service.Contracts;

namespace Talentree.Service.Services
{
    /// <summary>
    /// Calls the Talentree AI microservice (FastAPI on HuggingFace)
    /// Centralized background job caller
    /// </summary>
    public class AIService : IAIService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AIService> _logger;
        private readonly string _baseUrl;

        public AIService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<AIService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _baseUrl = configuration["AIService:BaseUrl"]
                ?? throw new InvalidOperationException("AIService:BaseUrl is missing in appsettings");
        }

        // ═══════════════════════════════════════════════════════
        // SENTIMENT PREDICTION (Reviews)
        // ═══════════════════════════════════════════════════════
        public async Task PredictSentimentAsync(int reviewId)
        {
            _logger.LogInformation("Starting sentiment prediction for review {ReviewId}", reviewId);

            await CallAIAsync($"/ai/predict/sentiment/{reviewId}", HttpMethod.Post);

            _logger.LogInformation("Sentiment prediction logged for review {ReviewId}", reviewId);
        }

        // ═══════════════════════════════════════════════════════
        // TRIAGE PREDICTION (Support Tickets)
        // ═══════════════════════════════════════════════════════
        public async Task PredictTriageAsync(int ticketId)
        {
            _logger.LogInformation("Starting triage prediction for ticket {TicketId}", ticketId);

            try
            {
                await CallAIAsync($"/ai/predict/triage/{ticketId}", HttpMethod.Post);
                _logger.LogInformation("Triage prediction completed for ticket {TicketId}", ticketId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Triage prediction failed for ticket {TicketId}", ticketId);
            }
        }

        // ═══════════════════════════════════════════════════════
        // PRODUCT COMPUTATION (Products)
        // ═══════════════════════════════════════════════════════
        public async Task ComputeProductAsync(int productId)
        {
            _logger.LogInformation("Starting product computation for product {ProductId}", productId);

            await CallAIAsync($"/ai/compute/product/{productId}", HttpMethod.Post);
        }

        // ═══════════════════════════════════════════════════════
        // DEMAND PREDICTION (Products)
        // ═══════════════════════════════════════════════════════
        public async Task PredictDemandAsync(int productId)
        {
            _logger.LogInformation("Starting demand prediction for product {ProductId}", productId);

            await CallAIAsync($"/ai/predict/demand/{productId}", HttpMethod.Post);
        }

        // ═══════════════════════════════════════════════════════
        // PROFILE COMPUTATION (Business Owners)
        // ═══════════════════════════════════════════════════════
        public async Task ComputeProfileAsync(string boUserId)
        {
            _logger.LogInformation("Starting profile computation for BO {BoUserId}", boUserId);

            await CallAIAsync($"/ai/compute/profile/{boUserId}", HttpMethod.Post);
        }

        // ═══════════════════════════════════════════════════════
        // CHURN PREDICTION (User Login)
        // ═══════════════════════════════════════════════════════
        public async Task PredictChurnAsync(string userId)
        {
            _logger.LogInformation("Starting churn prediction for user {UserId}", userId);

            await CallAIAsync($"/ai/predict/churn/{userId}", HttpMethod.Post);
        }

        // ═══════════════════════════════════════════════════════
        // FRAUD PREDICTION (Production Requests)
        // ═══════════════════════════════════════════════════════
        public async Task PredictFraudAsync(int requestId)
        {
            _logger.LogInformation("Starting fraud prediction for production request {RequestId}", requestId);

            await CallAIAsync($"/ai/predict/fraud/{requestId}", HttpMethod.Post);
        }

        // ═══════════════════════════════════════════════════════
        // REQUEST COMPLETION COMPUTATION (Completed Requests)
        // ═══════════════════════════════════════════════════════
        public async Task ComputeRequestAsync(int requestId)
        {
            _logger.LogInformation("Starting request completion computation for production request {RequestId}", requestId);

            await CallAIAsync($"/ai/compute/request/{requestId}", HttpMethod.Post);
        }

        // ═══════════════════════════════════════════════════════
        // ANOMALY PREDICTION (Financial Transactions)
        // ═══════════════════════════════════════════════════════
        public async Task PredictAnomalyAsync(int txId)
        {
            _logger.LogInformation("Starting anomaly prediction for transaction {TxId}", txId);

            await CallAIAsync($"/ai/predict/anomaly/{txId}", HttpMethod.Post);
        }

        // ═══════════════════════════════════════════════════════
        // PRIVATE: Generic fire-and-forget HTTP caller
        // ═══════════════════════════════════════════════════════
        /// <summary>
        /// ✅ Safe HTTP call to AI microservice
        /// - Never blocks main request
        /// - Never throws — AI failure must not crash main API
        /// - Comprehensive error logging
        /// </summary>
        private async Task CallAIAsync(string endpoint, HttpMethod method)
        {
            try
            {
                _logger.LogDebug("Calling AI endpoint: {Endpoint}", endpoint);

                var request = new HttpRequestMessage(method, $"{_baseUrl}{endpoint}");
                var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromMinutes(5));

                var response = await _httpClient.SendAsync(request, cts.Token);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "AI service returned non-success status {StatusCode} for {Endpoint}. Reason: {ReasonPhrase}",
                        response.StatusCode, endpoint, response.ReasonPhrase);
                }
                else
                {
                    _logger.LogInformation(
                        "AI call succeeded: {Endpoint} - {StatusCode}",
                        endpoint, response.StatusCode);
                }
            }
            catch (System.Threading.Tasks.TaskCanceledException)
            {
                _logger.LogWarning("AI service call timeout for endpoint: {Endpoint}", endpoint);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex,
                    "AI service connection error for endpoint: {Endpoint}. BaseUrl: {BaseUrl}",
                    endpoint, _baseUrl);
            }
            catch (Exception ex)
            {
                // Log but never throw — AI failure must never crash the main API
                _logger.LogError(ex, "Unexpected error calling AI service for endpoint: {Endpoint}", endpoint);
            }
        }
    }
}