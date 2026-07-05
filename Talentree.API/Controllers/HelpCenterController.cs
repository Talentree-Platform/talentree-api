using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Talentree.API.Models;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs.AI.HelpCenter;

namespace Talentree.API.Controllers
{
    /// <summary>
    /// Controller for the AI Help Center chatbot integration.
    /// Acts as a proxy between the frontend and the external HuggingFace AI Help Center API.
    /// </summary>
    [Route("api/help-center")]
    [ApiController]
    public class HelpCenterController : BaseApiController
    {
        private readonly IAiHelpCenterService _aiHelpCenterService;

        public HelpCenterController(IAiHelpCenterService aiHelpCenterService)
        {
            _aiHelpCenterService = aiHelpCenterService;
        }

        /// <summary>
        /// Sends conversation history messages to the AI Help Center chatbot and returns the generated reply.
        /// </summary>
        /// <param name="dto">The chat request containing the list of conversation messages.</param>
        /// <returns>The AI generated reply wrapped in a standard API response.</returns>
        [HttpPost("chat")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<ChatResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> Chat([FromBody] ChatRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Invalid request model. Please check your input."));
            }

            var result = await _aiHelpCenterService.SendChatAsync(dto);
            return Ok(ApiResponse<ChatResponseDto>.SuccessResponse(result, "Response received from AI Help Center"));
        }
    }
}
