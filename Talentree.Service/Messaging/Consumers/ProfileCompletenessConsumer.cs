using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Talentree.Service.Contracts;
using Talentree.Service.Messaging.Contracts;

namespace Talentree.Service.Messaging.Consumers
{
    public class ProfileCompletenessConsumer : BaseRabbitMQConsumer<ProfileComputationMessage>
    {
        protected override string QueueName => "talentree.ai.profile.queue";
        protected override string RoutingKey => "ai.profile";

        public ProfileCompletenessConsumer(
            RabbitMQConnectionManager connectionManager,
            IServiceProvider serviceProvider,
            ILogger<ProfileCompletenessConsumer> logger)
            : base(connectionManager, serviceProvider, logger)
        {
        }

        protected override async Task ProcessMessageAsync(ProfileComputationMessage message, IServiceProvider serviceProvider)
        {
            var aiService = serviceProvider.GetRequiredService<IAIService>();
            await aiService.ComputeProfileAsync(message.UserId);
        }
    }
}
