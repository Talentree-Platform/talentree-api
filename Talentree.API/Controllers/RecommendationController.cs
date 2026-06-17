using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Talentree.Service.Messaging;
using Talentree.Service.Messaging.Contracts;

namespace Talentree.API.Controllers
{
    [Authorize]
    public class RecommendationController : BaseApiController
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILogger<RecommendationController> _logger;
        private readonly string _baseUrl;

        public RecommendationController(
            IHttpClientFactory httpClientFactory,
            IEventPublisher eventPublisher,
            IConfiguration configuration,
            ILogger<RecommendationController> logger)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            _baseUrl = configuration["AIService:RecommendationBaseUrl"]
                ?? throw new InvalidOperationException("AIService:RecommendationBaseUrl is missing in appsettings.");
        }

        [HttpPost("customer")]
        public async Task<IActionResult> GetCustomerRecommendations([FromBody] RecommendationRequestDto requestDto)
        {
            var authHeader = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return Unauthorized("Authorization header with Bearer token is required.");
            }

            try
            {
                _logger.LogInformation("Processing synchronous customer recommendations request (TopK: {TopK})", requestDto.TopK);

                using var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(30);

                var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/customer/recommend");
                request.Headers.Add("Authorization", authHeader);
                request.Content = new StringContent(JsonSerializer.Serialize(new { top_k = requestDto.TopK }), Encoding.UTF8, "application/json");

                var response = await client.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("FastAPI customer recommend request failed. Status: {StatusCode}, Body: {Body}", response.StatusCode, responseBody);
                    return StatusCode((int)response.StatusCode, responseBody);
                }

                _logger.LogInformation("Successfully retrieved customer recommendations from Railway.");
                return Content(responseBody, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error retrieving customer recommendations.");
                return StatusCode(500, "An error occurred retrieving recommendations.");
            }
        }

        [HttpPost("owner")]
        public async Task<IActionResult> GetOwnerRecommendations([FromBody] RecommendationRequestDto requestDto)
        {
            var authHeader = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return Unauthorized("Authorization header with Bearer token is required.");
            }

            try
            {
                _logger.LogInformation("Processing synchronous owner recommendations request (TopK: {TopK})", requestDto.TopK);

                using var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(30);

                var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/owner/recommend");
                request.Headers.Add("Authorization", authHeader);
                request.Content = new StringContent(JsonSerializer.Serialize(new { top_k = requestDto.TopK }), Encoding.UTF8, "application/json");

                var response = await client.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("FastAPI owner recommend request failed. Status: {StatusCode}, Body: {Body}", response.StatusCode, responseBody);
                    return StatusCode((int)response.StatusCode, responseBody);
                }

                _logger.LogInformation("Successfully retrieved owner recommendations from Railway.");
                return Content(responseBody, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error retrieving owner recommendations.");
                return StatusCode(500, "An error occurred retrieving recommendations.");
            }
        }

        [AllowAnonymous]
        [HttpPost("retrain")]
        public async Task<IActionResult> TriggerRetraining([FromBody] RetrainRequestDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.ModelType))
            {
                return BadRequest("ModelType must be specified (customer/owner).");
            }

            if (!dto.ModelType.Equals("customer", StringComparison.OrdinalIgnoreCase) &&
                !dto.ModelType.Equals("owner", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Invalid ModelType. Must be 'customer' or 'owner'.");
            }

            _logger.LogInformation("Enqueuing retraining trigger for model: {ModelType}", dto.ModelType);
            await _eventPublisher.PublishAsync("ai.retrain", new AIRetrainMessage { ModelType = dto.ModelType });

            return Ok(new { success = true, message = $"Retraining for model '{dto.ModelType}' triggered in the background via RabbitMQ." });
        }
    }

    public class RecommendationRequestDto
    {
        public int TopK { get; set; } = 6;
    }

    public class RetrainRequestDto
    {
        public string ModelType { get; set; } = string.Empty;
    }
}
