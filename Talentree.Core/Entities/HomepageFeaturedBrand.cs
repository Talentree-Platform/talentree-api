using Talentree.Core.Entities.Identity;

namespace Talentree.Core.Entities
{
    /// <summary>
    /// FR-AD-35: Featured brands displayed on the homepage (up to 10).
    /// References the AppUser (BusinessOwner) account of the brand.
    /// </summary>
    public class HomepageFeaturedBrand : AuditableEntity
    {
        /// <summary>FK to the business owner's AppUser.Id.</summary>
        public string BusinessOwnerId { get; set; } = string.Empty;
        public AppUser? BusinessOwner { get; set; }

        /// <summary>Controls render order — lower values appear first.</summary>
        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
