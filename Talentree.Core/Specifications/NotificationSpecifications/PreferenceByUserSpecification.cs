
using Talentree.Core.Entities;

namespace Talentree.Core.Specifications.NotificationSpecifications
{
    public class PreferenceByUserSpecification : BaseSpecifications<NotificationPreference>
    {
        public PreferenceByUserSpecification(string userId)
            : base(p => p.UserId == userId)
        {
        }
    }
}