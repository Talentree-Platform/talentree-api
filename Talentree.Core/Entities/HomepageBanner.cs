namespace Talentree.Core.Entities
{
    /// <summary>
    /// FR-AD-35: Hero and promotional banners displayed on the homepage.
    /// Supports scheduling (start/end dates) and auto-hide when expired.
    /// </summary>
    public class HomepageBanner : AuditableEntity
    {
        public string Title { get; set; } = string.Empty;
        public string? Subtitle { get; set; }

        /// <summary>Path or URL to the uploaded banner image.</summary>
        public string ImageUrl { get; set; } = string.Empty;

        /// <summary>Optional navigation target when the banner is clicked.</summary>
        public string? LinkUrl { get; set; }

        /// <summary>Optional text rendered as an overlay on top of the banner image.</summary>
        public string? TextOverlay { get; set; }

        /// <summary>Controls render order — lower values appear first.</summary>
        public int DisplayOrder { get; set; }

        /// <summary>When true, this is a full-width hero banner; when false, a smaller promotional banner.</summary>
        public bool IsHero { get; set; }

        /// <summary>When false, the banner is manually hidden regardless of schedule.</summary>
        public bool IsActive { get; set; } = true;

        /// <summary>Inclusive start of the display window. Null = display immediately.</summary>
        public DateTime? ScheduleStart { get; set; }

        /// <summary>Exclusive end of the display window. Null = display indefinitely.</summary>
        public DateTime? ScheduleEnd { get; set; }
    }
}
