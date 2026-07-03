using System.ComponentModel.DataAnnotations;

namespace Talentree.Service.DTOs.AI
{
    /// <summary>
    /// DTO for sending a message to the AI chatbot agent.
    /// The frontend only sends the message — seller profile data
    /// (brand name, category, target audience, tone) is auto-filled
    /// from the database in the controller.
    /// </summary>
    public class ChatbotRequestDto
    {
        [Required(ErrorMessage = "Message is required")]
        [StringLength(2000, ErrorMessage = "Message cannot exceed 2000 characters")]
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Maps the response from the Railway chatbot AI service.
    /// </summary>
    public class ChatbotResponseDto
    {
        public string Response { get; set; } = string.Empty;
    }
}
