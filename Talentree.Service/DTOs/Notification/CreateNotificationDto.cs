using System.ComponentModel.DataAnnotations;
using Talentree.Core.Enums;

namespace Talentree.Service.DTOs.Notification
{
    /// <summary>
    /// DTO for creating a notification
    /// Used internally by services to create notifications
    /// </summary>
    public class CreateNotificationDto
    {
        // ═══════════════════════════════════════════════════════════
        // REQUIRED FIELDS
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// User who receives this notification
        /// </summary>
        [Required(ErrorMessage = "User ID is required")]
        public string UserId { get; set; } = null!;

        /// <summary>
        /// Type of notification
        /// </summary>
        [Required(ErrorMessage = "Notification type is required")]
        public NotificationType Type { get; set; }

        /// <summary>
        /// Short title of notification
        /// </summary>
        [Required(ErrorMessage = "Title is required")]
        [StringLength(255, ErrorMessage = "Title cannot exceed 255 characters")]
        public string Title { get; set; } = null!;

        /// <summary>
        /// Detailed message content
        /// </summary>
        [Required(ErrorMessage = "Message is required")]
        [StringLength(2000, ErrorMessage = "Message cannot exceed 2000 characters")]
        public string Message { get; set; } = null!;

        // ═══════════════════════════════════════════════════════════
        // OPTIONAL FIELDS
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// URL to navigate to
        /// </summary>
        [StringLength(500, ErrorMessage = "Action URL cannot exceed 500 characters")]
        public string? ActionUrl { get; set; }

        /// <summary>
        /// Button text for the action
        /// </summary>
        [StringLength(100, ErrorMessage = "Action text cannot exceed 100 characters")]
        public string? ActionText { get; set; }

        /// <summary>
        /// Entity type this notification relates to
        /// </summary>
        [StringLength(100)]
        public string? RelatedEntityType { get; set; }

        /// <summary>
        /// Entity ID this notification relates to
        /// </summary>
        public int? RelatedEntityId { get; set; }

        /// <summary>
        /// Priority level of notification
        /// Affects filtering and urgency
        /// </summary>
        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

        /// <summary>
        /// Whether to send email notification in addition to in-app
        /// Respects user preferences
        /// </summary>
        public bool SendEmail { get; set; } = false;
    }
}