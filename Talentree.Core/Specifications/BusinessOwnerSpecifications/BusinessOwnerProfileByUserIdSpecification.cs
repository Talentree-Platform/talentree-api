using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talentree.Core.Entities.Identity;

namespace Talentree.Core.Specifications.BusinessOwnerSpecifications
{
    public class BusinessOwnerProfileByUserIdSpecification : BaseSpecifications<BusinessOwnerProfile>
    {
        public BusinessOwnerProfileByUserIdSpecification(string userId)
            : base(b => b.UserId == userId && !b.IsDeleted)
        {
        }
    }
}
