using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Talentree.Service.Contracts;
using Talentree.Service.Messaging.Contracts;

namespace Talentree.Service.Messaging.Consumers
{
    public class FraudPredictionConsumer : BaseRabbitMQConsumer<FraudPredictionMessage>
    {
        protected override string QueueName => "talentree.ai.fraud.queue";
        protected override string RoutingKey => "ai.fraud";

        public FraudPredictionConsumer(
            RabbitMQConnectionManager connectionManager,
            IServiceProvider serviceProvider,
            ILogger<FraudPredictionConsumer> logger)
            : base(connectionManager, serviceProvider, logger)
        {
        }

        protected override async Task ProcessMessageAsync(FraudPredictionMessage message, IServiceProvider serviceProvider)
        {
            var aiService = serviceProvider.GetRequiredService<IAIService>();
            await aiService.PredictFraudAsync(message.RequestId);
        }
    }
}
