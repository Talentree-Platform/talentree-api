
using Talentree.Core.Enums;

namespace Talentree.Service.DTOs.Notification
{
    public class NotificationDto
    {
        public int Id { get; set; }

        // Type
        public NotificationType Type { get; set; }
        public string TypeText { get; set; } = string.Empty;

        // Content
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? ActionUrl { get; set; }
        public string? ActionText { get; set; }

        // Metadata
        public string? RelatedEntityType { get; set; }
        public int? RelatedEntityId { get; set; }

        // Status
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }

        // Priority
        public NotificationPriority Priority { get; set; }
        public string PriorityText { get; set; } = string.Empty;

        // Timestamps
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Human-readable time ago (e.g., "5 minutes ago", "2 hours ago")
        /// Calculated in mapper
        /// </summary>
        public string TimeAgo { get; set; } = string.Empty;
    }
}