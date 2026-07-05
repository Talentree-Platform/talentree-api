using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Talentree.Service.DTOs.AI.HelpCenter
{
    /// <summary>
    /// Represents a single message object in the chat history.
    /// </summary>
    public class ChatMessageDto
    {
        /// <summary>
        /// Message sender role ("user" or "assistant").
        /// </summary>
        [Required(ErrorMessage = "Role is required")]
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// Text content of the message.
        /// </summary>
        [Required(ErrorMessage = "Content is required")]
        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }
}
