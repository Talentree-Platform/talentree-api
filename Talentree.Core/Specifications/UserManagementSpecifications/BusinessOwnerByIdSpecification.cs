using Talentree.Core.Entities.Identity;

namespace Talentree.Core.Specifications.UserManagementSpecifications
{
    public class BusinessOwnerByIdSpecification : BaseSpecifications<AppUser>
    {
        public BusinessOwnerByIdSpecification(string userId)
            : base(u => u.Id == userId && u.BusinessOwnerProfile != null)
        {
            AddInclude(u => u.BusinessOwnerProfile);
        }
    }
}