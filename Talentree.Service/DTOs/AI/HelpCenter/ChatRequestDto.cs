using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Talentree.Service.DTOs.AI.HelpCenter
{
    /// <summary>
    /// Request payload for the AI Help Center chat endpoint.
    /// Contains the list of messages in the conversation history.
    /// </summary>
    public class ChatRequestDto
    {
        /// <summary>
        /// Array of previous and current messages sent to the AI service.
        /// </summary>
        [Required(ErrorMessage = "Messages list is required")]
        [MinLength(1, ErrorMessage = "Messages list cannot be empty")]
        [JsonPropertyName("messages")]
        public List<ChatMessageDto> Messages { get; set; } = new List<ChatMessageDto>();
    }
}
