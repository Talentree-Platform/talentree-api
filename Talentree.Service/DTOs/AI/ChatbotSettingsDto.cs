using System.ComponentModel.DataAnnotations;

namespace Talentree.Service.DTOs.AI
{
    /// <summary>
    /// DTO for reading / updating the AI chatbot profile settings.
    /// These fields provide context to the AI agent so it can
    /// generate brand-appropriate responses.
    /// </summary>
    public class ChatbotSettingsDto
    {
        [StringLength(500, ErrorMessage = "Target audience cannot exceed 500 characters")]
        public string? TargetAudience { get; set; }

        [StringLength(100, ErrorMessage = "Brand tone cannot exceed 100 characters")]
        public string? Tone { get; set; }

        // Read-only — populated from the existing profile for display
        public string? BusinessName { get; set; }
        public string? BusinessCategory { get; set; }
    }

    /// <summary>
    /// DTO for updating chatbot settings (target audience and tone only).
    /// </summary>
    public class UpdateChatbotSettingsDto
    {
        [Required(ErrorMessage = "Target audience is required")]
        [StringLength(500, ErrorMessage = "Target audience cannot exceed 500 characters")]
        public string TargetAudience { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tone is required")]
        [StringLength(100, ErrorMessage = "Tone cannot exceed 100 characters")]
        public string Tone { get; set; } = "Professional";
    }
}
