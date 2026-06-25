using System;
using System.Collections.Generic;
using Talentree.Core.Entities;
using Talentree.Core.Enums;

namespace Talentree.Core.Specifications.PlatformSettingsSpecifications
{
    /// <summary>
    /// Eagerly loads BusinessOwner and BusinessOwnerProfile for featured brands.
    /// </summary>
    public class FeaturedBrandsWithProfileSpecification : BaseSpecifications<HomepageFeaturedBrand>
    {
        public FeaturedBrandsWithProfileSpecification(bool onlyActive = false)
            : base(b => !onlyActive || b.IsActive)
        {
            AddInclude("BusinessOwner");
            AddInclude("BusinessOwner.BusinessOwnerProfile");
        }
    }

    /// <summary>
    /// Eagerly loads Product and Product.Images for featured products.
    /// </summary>
    public class FeaturedProductsWithProductSpecification : BaseSpecifications<HomepageFeaturedProduct>
    {
        public FeaturedProductsWithProductSpecification(bool onlyActive = false)
            : base(p => !onlyActive || p.IsActive)
        {
            AddInclude("Product");
            AddInclude("Product.Images");
        }
    }

    /// <summary>
    /// Fetches platform policies, optionally filtered by policy document type.
    /// </summary>
    public class PlatformPolicySpecification : BaseSpecifications<PlatformPolicy>
    {
        public PlatformPolicySpecification() : base()
        {
        }

        public PlatformPolicySpecification(PolicyDocumentType type)
            : base(p => p.DocumentType == type)
        {
        }
    }
}
