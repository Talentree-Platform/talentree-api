using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Talentree.Service.Contracts;
using Talentree.Service.Messaging.Contracts;

namespace Talentree.Service.Messaging.Consumers
{
    public class ReviewSentimentConsumer : BaseRabbitMQConsumer<SentimentPredictionMessage>
    {
        protected override string QueueName => "talentree.ai.sentiment.queue";
        protected override string RoutingKey => "ai.sentiment";

        public ReviewSentimentConsumer(
            RabbitMQConnectionManager connectionManager,
            IServiceProvider serviceProvider,
            ILogger<ReviewSentimentConsumer> logger)
            : base(connectionManager, serviceProvider, logger)
        {
        }

        protected override async Task ProcessMessageAsync(SentimentPredictionMessage message, IServiceProvider serviceProvider)
        {
            var aiService = serviceProvider.GetRequiredService<IAIService>();
            await aiService.PredictSentimentAsync(message.ReviewId);
        }
    }
}
