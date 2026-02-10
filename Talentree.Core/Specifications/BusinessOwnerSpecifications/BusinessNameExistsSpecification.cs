using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talentree.Core.Entities.Identity;

namespace Talentree.Core.Specifications.BusinessOwnerSpecifications
{
    public class BusinessNameExistsSpecification : BaseSpecifications<BusinessOwnerProfile>
    {
        public BusinessNameExistsSpecification(string businessName)
            : base(b => b.BusinessName.ToLower() == businessName.ToLower()
                     && !b.IsDeleted)
        {
            // Check for exact match (case-insensitive)
            // Exclude soft-deleted profiles
        }
    }
}
