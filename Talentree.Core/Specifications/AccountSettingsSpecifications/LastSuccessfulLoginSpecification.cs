using Talentree.Core.Entities.Identity;

namespace Talentree.Core.Specifications.AccountSettingsSpecifications
{
    public class LastSuccessfulLoginSpecification : BaseSpecifications<LoginHistory>
    {
        public LastSuccessfulLoginSpecification(string userId)
            : base(l => l.UserId == userId && l.IsSuccessful)
        {
            AddOrderByDescending(l => l.LoginAt);
            ApplyPagination(1, 1);
        }
    }
}
