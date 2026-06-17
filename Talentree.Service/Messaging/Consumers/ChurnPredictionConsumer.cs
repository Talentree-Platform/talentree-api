using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Talentree.Service.Contracts;
using Talentree.Service.Messaging.Contracts;

namespace Talentree.Service.Messaging.Consumers
{
    public class ChurnPredictionConsumer : BaseRabbitMQConsumer<ChurnPredictionMessage>
    {
        protected override string QueueName => "talentree.ai.churn.queue";
        protected override string RoutingKey => "ai.churn";

        public ChurnPredictionConsumer(
            RabbitMQConnectionManager connectionManager,
            IServiceProvider serviceProvider,
            ILogger<ChurnPredictionConsumer> logger)
            : base(connectionManager, serviceProvider, logger)
        {
        }

        protected override async Task ProcessMessageAsync(ChurnPredictionMessage message, IServiceProvider serviceProvider)
        {
            var aiService = serviceProvider.GetRequiredService<IAIService>();
            await aiService.PredictChurnAsync(message.UserId);
        }
    }
}
