using Talentree.Core.Entities.Identity;
using Talentree.Core.Enums;

namespace Talentree.Core.Specifications.BusinessOwnerSpecifications
{
    public class BrandByIdSpecification : BaseSpecifications<BusinessOwnerProfile>
    {
        public BrandByIdSpecification(int brandProfileId)
            : base(b => b.Id == brandProfileId && b.Status == ApprovalStatus.Approved)
        {
            AddInclude(b => b.User);
            AddInclude("Products.Images");
        }
    }
}
