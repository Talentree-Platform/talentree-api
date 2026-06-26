namespace Talentree.Core.Entities
{
    /// <summary>
    /// FR-AD-35: Scrolling announcement bar displayed at the top of the homepage.
    /// Only one bar should be active at a time. Supports scheduling.
    /// </summary>
    public class AnnouncementBar : AuditableEntity
    {
        public string Message { get; set; } = string.Empty;

        /// <summary>Optional URL the user navigates to when clicking the announcement bar.</summary>
        public string? LinkUrl { get; set; }

        public bool IsActive { get; set; } = true;

        /// <summary>Inclusive start of the display window. Null = display immediately.</summary>
        public DateTime? ScheduleStart { get; set; }

        /// <summary>Exclusive end of the display window. Null = display indefinitely.</summary>
        public DateTime? ScheduleEnd { get; set; }
    }
}
