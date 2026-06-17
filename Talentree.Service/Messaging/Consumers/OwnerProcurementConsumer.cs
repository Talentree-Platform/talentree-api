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
    public class OwnerProcurementConsumer : BaseRabbitMQConsumer<OwnerProcurementMessage>
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        protected override string QueueName => "talentree.ai.owner.recommend.queue";
        protected override string RoutingKey => "ai.owner.recommend";

        public OwnerProcurementConsumer(
            RabbitMQConnectionManager connectionManager,
            IServiceProvider serviceProvider,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<OwnerProcurementConsumer> logger)
            : base(connectionManager, serviceProvider, logger)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        protected override async Task ProcessMessageAsync(OwnerProcurementMessage message, IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
            var tokenService = serviceProvider.GetRequiredService<ITokenService>();

            _logger.LogInformation("Fetching user {UserId} for background owner procurement recommendations.", message.UserId);
            var user = await userManager.FindByIdAsync(message.UserId);
            if (user == null)
            {
                _logger.LogError("User {UserId} not found. Cannot generate recommendation token.", message.UserId);
                throw new InvalidOperationException($"User {message.UserId} not found.");
            }

            // Generate Bearer token for the user dynamically
            var token = tokenService.GenerateAccessToken(user, new List<string> { "Owner" });

            var baseUrl = _configuration["AIService:RecommendationBaseUrl"] 
                ?? throw new InvalidOperationException("AIService:RecommendationBaseUrl is missing in configuration.");

            using var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(30);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var payload = new { top_k = message.TopK };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            _logger.LogInformation("Sending owner recommendations request to FastAPI Railway for owner {UserId}.", message.UserId);
            var response = await httpClient.PostAsync($"{baseUrl}/owner/recommend", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogError("FastAPI owner recommend request failed. Status: {StatusCode}, Body: {ErrorBody}", response.StatusCode, errorBody);
                response.EnsureSuccessStatusCode(); // Throws and triggers Polly retry
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Successfully completed background owner recommendation call for owner {UserId}. Response length: {Length}", message.UserId, responseBody.Length);
        }
    }
}
