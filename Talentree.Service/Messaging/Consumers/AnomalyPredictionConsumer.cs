using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Talentree.Service.Contracts;
using Talentree.Service.Messaging.Contracts;

namespace Talentree.Service.Messaging.Consumers
{
    public class AnomalyPredictionConsumer : BaseRabbitMQConsumer<AnomalyPredictionMessage>
    {
        protected override string QueueName => "talentree.ai.anomaly.queue";
        protected override string RoutingKey => "ai.anomaly";

        public AnomalyPredictionConsumer(
            RabbitMQConnectionManager connectionManager,
            IServiceProvider serviceProvider,
            ILogger<AnomalyPredictionConsumer> logger)
            : base(connectionManager, serviceProvider, logger)
        {
        }

        protected override async Task ProcessMessageAsync(AnomalyPredictionMessage message, IServiceProvider serviceProvider)
        {
            var aiService = serviceProvider.GetRequiredService<IAIService>();
            await aiService.PredictAnomalyAsync(message.TransactionId);
        }
    }
}
