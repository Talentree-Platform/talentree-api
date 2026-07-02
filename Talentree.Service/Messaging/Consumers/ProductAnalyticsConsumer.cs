using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Talentree.Service.Contracts;
using Talentree.Service.Messaging.Contracts;

namespace Talentree.Service.Messaging.Consumers
{
    public class ProductAnalyticsConsumer : BaseRabbitMQConsumer<ProductComputationMessage>
    {
        protected override string QueueName => "talentree.ai.product.queue";
        protected override string RoutingKey => "ai.product";

        public ProductAnalyticsConsumer(
            RabbitMQConnectionManager connectionManager,
            IServiceProvider serviceProvider,
            ILogger<ProductAnalyticsConsumer> logger)
            : base(connectionManager, serviceProvider, logger)
        {
        }

        protected override async Task ProcessMessageAsync(ProductComputationMessage message, IServiceProvider serviceProvider)
        {
            var aiService = serviceProvider.GetRequiredService<IAIService>();
            await aiService.ComputeProductAsync(message.ProductId);
            await aiService.PredictDemandAsync(message.ProductId);
        }
    }
}
