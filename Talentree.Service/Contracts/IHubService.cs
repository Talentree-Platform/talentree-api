
namespace Talentree.Service.Contracts
{
    public interface IHubService
    {
        Task SendNotificationToUserAsync(string userId, object notification);
        
        Task UpdateUnreadCountAsync(string userId, int count);

        Task SendNotificationToUsersAsync(List<string> userIds, object notification);

        Task SendToAllAsync(string method, object data);
    }
}