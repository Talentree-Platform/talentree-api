namespace Talentree.Core.Entities
{
    /// <summary>
    /// FR-AD-35: Featured products displayed on the homepage (up to 20).
    /// Supports scheduling to auto-hide expired features.
    /// </summary>
    public class HomepageFeaturedProduct : AuditableEntity
    {
        /// <summary>FK to Product.Id.</summary>
        public int ProductId { get; set; }
        public Product? Product { get; set; }

        /// <summary>Controls render order — lower values appear first.</summary>
        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; } = true;

        /// <summary>Inclusive start of the display window. Null = display immediately.</summary>
        public DateTime? ScheduleStart { get; set; }

        /// <summary>Exclusive end of the display window. Null = display indefinitely.</summary>
        public DateTime? ScheduleEnd { get; set; }
    }
}
