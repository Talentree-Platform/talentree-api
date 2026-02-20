using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talentree.Core.Entities;

namespace Talentree.Core.Specifications.ProductSpecifications
{
    public class ProductByIdSpecification : BaseSpecifications<Product>
    {
        public ProductByIdSpecification(int productId)
            : base(p => p.Id == productId && !p.IsDeleted)
        {
            AddInclude(p => p.Images);
            AddInclude(p => p.Category);
            AddInclude(p => p.BusinessOwner);
        }
    }
}
