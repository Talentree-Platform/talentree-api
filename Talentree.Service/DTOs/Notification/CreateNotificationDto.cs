
using System.ComponentModel.DataAnnotations;
using Talentree.Core.Enums;

namespace Talentree.Service.DTOs.Notification
{
    public class CreateNotificationDto
    {
        [Required]
        public string UserId { get; set; } = null!;

        [Required]
        public NotificationType Type { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; } = null!;

        [Required]
        [StringLength(2000)]
        public string Message { get; set; } = null!;

        [StringLength(500)]
        public string? ActionUrl { get; set; }

        [StringLength(50)]
        public string? ActionText { get; set; }

        public string? RelatedEntityType { get; set; }
        public int? RelatedEntityId { get; set; }

        /// <summary>
        /// Additional JSON data
        /// </summary>
        public string? Data { get; set; }

        public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

        public bool SendEmail { get; set; } = false;

        public DateTime? ExpiresAt { get; set; }
    }
}