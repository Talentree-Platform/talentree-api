using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Talentree.Service.Contracts;
using Talentree.Service.Messaging.Contracts;

namespace Talentree.Service.Messaging.Consumers
{
    public class UserInteractionConsumer : BaseRabbitMQConsumer<InteractionLogMessage>
    {
        protected override string QueueName => "talentree.interaction.queue";
        protected override string RoutingKey => "interaction.log";

        public UserInteractionConsumer(
            RabbitMQConnectionManager connectionManager,
            IServiceProvider serviceProvider,
            ILogger<UserInteractionConsumer> logger)
            : base(connectionManager, serviceProvider, logger)
        {
        }

        protected override async Task ProcessMessageAsync(InteractionLogMessage message, IServiceProvider serviceProvider)
        {
            var userInteractionService = serviceProvider.GetRequiredService<IUserInteractionService>();

            await userInteractionService.LogInteractionAsync(
                userId: message.UserId,
                userType: message.UserType,
                itemId: message.ItemId,
                itemType: message.ItemType,
                actionType: message.ActionType,
                category: message.Category,
                quantity: message.Quantity,
                price: message.Price
            );
        }
    }
}
