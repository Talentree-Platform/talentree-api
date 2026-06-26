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
        /// <summary>Standard spec — respects soft-delete global filter.</summary>
        public ProductByIdSpecification(int productId)
    : base(p => p.Id == productId)
        {
            AddInclude(p => p.Images);
            AddInclude(p => p.Category);
            AddInclude(p => p.BusinessOwner);
            AddInclude("BusinessOwner.User");
        }

        /// <summary>
        /// Admin variant — bypasses soft-delete global filter so deleted products can also be retrieved.
        /// </summary>
        public ProductByIdSpecification(int productId, bool ignoreQueryFilters)
            : base(p => p.Id == productId)
        {
            AddInclude(p => p.Images);
            AddInclude(p => p.Category);
            AddInclude(p => p.BusinessOwner);
            AddInclude("BusinessOwner.User");
            if (ignoreQueryFilters)
                EnableIgnoreQueryFilters();
        }
    }
}
