using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Talentree.Service.Contracts;
using Talentree.Service.Messaging.Contracts;

namespace Talentree.Service.Messaging.Consumers
{
    public class RequestComputationConsumer : BaseRabbitMQConsumer<RequestComputationMessage>
    {
        protected override string QueueName => "talentree.ai.request.queue";
        protected override string RoutingKey => "ai.request";

        public RequestComputationConsumer(
            RabbitMQConnectionManager connectionManager,
            IServiceProvider serviceProvider,
            ILogger<RequestComputationConsumer> logger)
            : base(connectionManager, serviceProvider, logger)
        {
        }

        protected override async Task ProcessMessageAsync(RequestComputationMessage message, IServiceProvider serviceProvider)
        {
            var aiService = serviceProvider.GetRequiredService<IAIService>();
            await aiService.ComputeRequestAsync(message.RequestId);
        }
    }
}
