using Talentree.Core.Entities;

namespace Talentree.Core.Specifications.NotificationSpecifications
{
    /// <summary>
    /// Get notification preferences for a specific user
    /// Used to retrieve user's notification settings
    /// </summary>
    public class GetNotificationPreferenceByUserIdSpecification : BaseSpecifications<NotificationPreference>
    {
        public GetNotificationPreferenceByUserIdSpecification(string userId)
            : base(np => np.UserId == userId)
        {
            AddInclude(np => np.User);
        }
    }
}