using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Talentree.Service.Contracts;
using Talentree.Service.Messaging.Contracts;

namespace Talentree.Service.Messaging.Consumers
{
    public class SupportTriageConsumer : BaseRabbitMQConsumer<TriagePredictionMessage>
    {
        protected override string QueueName => "talentree.ai.triage.queue";
        protected override string RoutingKey => "ai.triage";

        public SupportTriageConsumer(
            RabbitMQConnectionManager connectionManager,
            IServiceProvider serviceProvider,
            ILogger<SupportTriageConsumer> logger)
            : base(connectionManager, serviceProvider, logger)
        {
        }

        protected override async Task ProcessMessageAsync(TriagePredictionMessage message, IServiceProvider serviceProvider)
        {
            var aiService = serviceProvider.GetRequiredService<IAIService>();
            await aiService.PredictTriageAsync(message.TicketId);
        }
    }
}
