using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Talentree.Service.Messaging.Contracts;

namespace Talentree.Service.Messaging.Consumers
{
    public class AIRetrainConsumer : BaseRabbitMQConsumer<AIRetrainMessage>
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        protected override string QueueName => "talentree.ai.retrain.queue";
        protected override string RoutingKey => "ai.retrain";

        public AIRetrainConsumer(
            RabbitMQConnectionManager connectionManager,
            IServiceProvider serviceProvider,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<AIRetrainConsumer> logger)
            : base(connectionManager, serviceProvider, logger)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        protected override async Task ProcessMessageAsync(AIRetrainMessage message, IServiceProvider serviceProvider)
        {
            var baseUrl = _configuration["AIService:RecommendationBaseUrl"] 
                ?? throw new InvalidOperationException("AIService:RecommendationBaseUrl is missing in configuration.");

            string endpoint;
            if (message.ModelType.Equals("customer", StringComparison.OrdinalIgnoreCase))
            {
                endpoint = "/customer/retrain";
            }
            else if (message.ModelType.Equals("owner", StringComparison.OrdinalIgnoreCase))
            {
                endpoint = "/owner/retrain";
            }
            else
            {
                _logger.LogError("Invalid model type '{ModelType}' specified for retraining.", message.ModelType);
                return; // Do not retry invalid model types
            }

            using var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromMinutes(5);

            _logger.LogInformation("Sending retraining trigger request to FastAPI Railway for model: {ModelType}.", message.ModelType);
            var response = await httpClient.PostAsync($"{baseUrl}{endpoint}", new StringContent(string.Empty, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogError("FastAPI retrain trigger request failed. Status: {StatusCode}, Body: {ErrorBody}", response.StatusCode, errorBody);
                response.EnsureSuccessStatusCode(); // Throws and triggers Polly retry
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Successfully triggered background retraining for model {ModelType}. Response: {Response}", message.ModelType, responseBody);
        }
    }
}
