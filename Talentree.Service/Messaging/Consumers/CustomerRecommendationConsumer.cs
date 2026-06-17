using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Talentree.Core.Entities.Identity;
using Talentree.Service.Contracts;
using Talentree.Service.Messaging.Contracts;

namespace Talentree.Service.Messaging.Consumers
{
    public class CustomerRecommendationConsumer : BaseRabbitMQConsumer<CustomerRecommendationMessage>
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        protected override string QueueName => "talentree.ai.customer.recommend.queue";
        protected override string RoutingKey => "ai.customer.recommend";

        public CustomerRecommendationConsumer(
            RabbitMQConnectionManager connectionManager,
            IServiceProvider serviceProvider,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<CustomerRecommendationConsumer> logger)
            : base(connectionManager, serviceProvider, logger)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        protected override async Task ProcessMessageAsync(CustomerRecommendationMessage message, IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
            var tokenService = serviceProvider.GetRequiredService<ITokenService>();

            _logger.LogInformation("Fetching user {UserId} for background customer recommendations.", message.UserId);
            var user = await userManager.FindByIdAsync(message.UserId);
            if (user == null)
            {
                _logger.LogError("User {UserId} not found. Cannot generate recommendation token.", message.UserId);
                throw new InvalidOperationException($"User {message.UserId} not found.");
            }

            // Generate Bearer token for the user dynamically
            var token = tokenService.GenerateAccessToken(user, new List<string> { "Customer" });

            var baseUrl = _configuration["AIService:RecommendationBaseUrl"] 
                ?? throw new InvalidOperationException("AIService:RecommendationBaseUrl is missing in configuration.");

            using var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(30);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var payload = new { top_k = message.TopK };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            _logger.LogInformation("Sending customer recommendation request to FastAPI Railway for user {UserId}.", message.UserId);
            var response = await httpClient.PostAsync($"{baseUrl}/customer/recommend", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogError("FastAPI customer recommend request failed. Status: {StatusCode}, Body: {ErrorBody}", response.StatusCode, errorBody);
                response.EnsureSuccessStatusCode(); // Throws and triggers Polly retry
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Successfully completed background customer recommendation call for user {UserId}. Response length: {Length}", message.UserId, responseBody.Length);
        }
    }
}
