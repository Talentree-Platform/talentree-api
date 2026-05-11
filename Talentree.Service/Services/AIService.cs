using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Talentree.Service.Contracts;

namespace Talentree.Service.Services
{
    /// <summary>
    /// Calls the Talentree AI microservice (FastAPI on HuggingFace)
    /// Fire-and-forget — we never block the main request waiting for AI
    /// </summary>
    public class AIService : IAIService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AIService> _logger;
        private readonly string _baseUrl;

        public AIService(HttpClient httpClient, IConfiguration configuration, ILogger<AIService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _baseUrl = configuration["AIService:BaseUrl"]
                ?? throw new InvalidOperationException("AIService:BaseUrl is missing in appsettings");
        }

        // ─────────────────────────────────────────────
        // Called when new review is submitted
        // POST /ai/predict/sentiment/{review_id}
        // ─────────────────────────────────────────────
        public async Task PredictSentimentAsync(int reviewId)
        {
            await CallAIAsync($"/ai/predict/sentiment/{reviewId}", HttpMethod.Post);
        }

        // ─────────────────────────────────────────────
        // Called when new support ticket is created
        // POST /ai/predict/triage/{ticket_id}
        // ─────────────────────────────────────────────
        public async Task PredictTriageAsync(int ticketId)
        {
            await CallAIAsync($"/ai/predict/triage/{ticketId}", HttpMethod.Post);
        }

        // ─────────────────────────────────────────────
        // Called when product is created or updated
        // POST /ai/compute/product/{product_id}
        // ─────────────────────────────────────────────
        public async Task ComputeProductAsync(int productId)
        {
            await CallAIAsync($"/ai/compute/product/{productId}", HttpMethod.Post);
        }

        // ─────────────────────────────────────────────
        // Called when BO profile is updated
        // POST /ai/compute/profile/{bo_id}
        // ─────────────────────────────────────────────
        public async Task ComputeProfileAsync(string boUserId)
        {
            await CallAIAsync($"/ai/compute/profile/{boUserId}", HttpMethod.Post);
        }

        // ─────────────────────────────────────────────
        // Called when BO logs in
        // POST /ai/predict/churn/{user_id}
        // ─────────────────────────────────────────────
        public async Task PredictChurnAsync(string userId)
        {
            await CallAIAsync($"/ai/predict/churn/{userId}", HttpMethod.Post);
        }

        // ─────────────────────────────────────────────
        // PRIVATE: Generic fire-and-forget caller
        // Never throws — AI failure must never crash main API
        // ─────────────────────────────────────────────
        private async Task CallAIAsync(string endpoint, HttpMethod method)
        {
            try
            {
                var request = new HttpRequestMessage(method, $"{_baseUrl}{endpoint}");
                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "AI service returned {StatusCode} for {Endpoint}",
                        response.StatusCode, endpoint);
                }
                else
                {
                    _logger.LogInformation("AI call succeeded: {Endpoint}", endpoint);
                }
            }
            catch (Exception ex)
            {
                // Log but never throw — AI failure must never crash the main API
                _logger.LogError(ex, "AI service call failed for endpoint: {Endpoint}", endpoint);
            }
        }
    }
}