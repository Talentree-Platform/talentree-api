using System.Threading.Tasks;

namespace Talentree.Service.Messaging
{
    public interface IEventPublisher
    {
        Task PublishAsync<T>(string routingKey, T message) where T : class;
    }
}
