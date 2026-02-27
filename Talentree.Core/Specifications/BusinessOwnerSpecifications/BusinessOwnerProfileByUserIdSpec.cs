using Talentree.Core.Entities.Identity;
namespace Talentree.Core.Specifications.Identity
{
    /// <summary>Fetches a BO profile by the user's Identity ID — used to resolve business category.</summary>
    public class BusinessOwnerProfileByUserIdSpec : BaseSpecifications<BusinessOwnerProfile>
    {
        public BusinessOwnerProfileByUserIdSpec(string userId)
            : base(p => p.UserId == userId) { }
    }
}
