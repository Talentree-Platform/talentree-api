using Talentree.Core.Entities.Identity;

namespace Talentree.Core.Specifications.AccountSettingsSpecifications
{
    public class LoginHistoryByUserSpecification : BaseSpecifications<LoginHistory>
    {
        public LoginHistoryByUserSpecification(string userId, int pageIndex, int pageSize)
            : base(l => l.UserId == userId)
        {
            AddOrderByDescending(l => l.LoginAt);
            ApplyPagination(pageIndex, pageSize);
        }

        public LoginHistoryByUserSpecification(string userId)
            : base(l => l.UserId == userId)
        {
        }
    }
}