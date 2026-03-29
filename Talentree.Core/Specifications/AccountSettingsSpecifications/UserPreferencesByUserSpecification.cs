using Talentree.Core.Entities.Identity;

namespace Talentree.Core.Specifications.AccountSettingsSpecifications
{
    public class UserPreferencesByUserSpecification : BaseSpecifications<UserPreferences>
    {
        public UserPreferencesByUserSpecification(string userId)
            : base(p => p.UserId == userId)
        {
        }
    }
}