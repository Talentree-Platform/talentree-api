
using Talentree.Core.Entities.Identity;

namespace Talentree.Core.Specifications.BusinessOwnerSpecifications
{
    /// <summary>
    /// Specification to get business owner profile by ID with user info
    /// </summary>
    public class BusinessOwnerProfileByIdSpecification : BaseSpecifications<BusinessOwnerProfile>
    {
        public BusinessOwnerProfileByIdSpecification(int profileId)
            : base(b => b.Id == profileId && !b.IsDeleted)
        {
            AddInclude(b => b.User);
        }
    }
}