using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Talentree.Core.Entities.Identity;
using Talentree.Service.DTOs.AI;

namespace Talentree.API.Controllers.BusinessOwner
{
    /// <summary>
    /// Proxy controller for the AI Chatbot Agent service (Railway).
    /// Acts as a middleman between the frontend and the external chatbot API.
    ///
    /// Flow:
    ///   1. Frontend calls POST /api/bo/chatbot/send with { "message": "..." }
    ///   2. Controller fetches seller profile from DB (business name, category, etc.)
    ///   3. Controller forwards the enriched request to the Railway chatbot
    ///   4. Returns the AI response to the frontend
    ///
    /// The frontend NEVER calls the Railway service directly.
    /// </summary>
    [Authorize(Roles = "BusinessOwner")]
    [Route("api/bo/chatbot")]
    [ApiController]
    public class ChatbotController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<ChatbotController> _logger;

        public ChatbotController(
            IHttpClientFactory httpClientFactory,
            UserManager<AppUser> userManager,
            ILogger<ChatbotController> logger)
        {
            _httpClient = httpClientFactory.CreateClient("ChatbotService");
            _userManager = userManager;
            _logger = logger;
        }

        // ══════════════════════════════════════════════════════════
        // GET /api/bo/chatbot/settings
        // Returns the current chatbot context settings for this BO.
        // Frontend uses this to check if setup is needed
        // (i.e. TargetAudience is null → show setup card).
        // ══════════════════════════════════════════════════════════
        [HttpGet("settings")]
        [ProducesResponseType(typeof(ChatbotSettingsDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSettings()
        {
            var (_, profile) = await GetUserAndProfileAsync();

            return Ok(new ChatbotSettingsDto
            {
                TargetAudience = profile.TargetAudience,
                Tone = profile.BrandTone,
                BusinessName = profile.BusinessName,
                BusinessCategory = profile.BusinessCategory
            });
        }

        // ══════════════════════════════════════════════════════════
        // PUT /api/bo/chatbot/settings
        // Saves the target audience and brand tone to the BO profile.
        // Called from the chatbot setup card (first time) or settings.
        // ══════════════════════════════════════════════════════════
        [HttpPut("settings")]
        [ProducesResponseType(typeof(ChatbotSettingsDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateSettings([FromBody] UpdateChatbotSettingsDto dto)
        {
            var (_, profile) = await GetUserAndProfileAsync();

            profile.TargetAudience = dto.TargetAudience;
            profile.BrandTone = dto.Tone;

            await _userManager.UpdateAsync(profile.User);

            _logger.LogInformation(
                "Chatbot settings updated for seller {SellerId}. Audience: {Audience}, Tone: {Tone}",
                profile.UserId, dto.TargetAudience, dto.Tone);

            return Ok(new ChatbotSettingsDto
            {
                TargetAudience = profile.TargetAudience,
                Tone = profile.BrandTone,
                BusinessName = profile.BusinessName,
                BusinessCategory = profile.BusinessCategory
            });
        }

        // ══════════════════════════════════════════════════════════
        // POST /api/bo/chatbot/send
        // Sends a message to the AI chatbot agent.
        // Auto-fills seller context from the database.
        // ══════════════════════════════════════════════════════════
        [HttpPost("send")]
        [ProducesResponseType(typeof(ChatbotResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> SendMessage([FromBody] ChatbotRequestDto dto)
        {
            var (user, profile) = await GetUserAndProfileAsync();

            // Build the request payload the Railway chatbot expects
            var chatbotPayload = new
            {
                seller_id = user.Id,
                brand_name = profile.BusinessName,
                category = profile.BusinessCategory,
                target_audience = profile.TargetAudience ?? "General audience",
                tone = profile.BrandTone ?? "Professional",
                message = dto.Message
            };

            try
            {
                var json = JsonSerializer.Serialize(chatbotPayload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation(
                    "Sending chatbot request for seller {SellerId}", user.Id);

                var response = await _httpClient.PostAsync("/api/chat", content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "Chatbot returned {StatusCode} for seller {SellerId}: {Body}",
                        response.StatusCode, user.Id, responseBody);
                    return StatusCode((int)response.StatusCode, new { error = "AI chatbot returned an error", detail = responseBody });
                }

                _logger.LogInformation(
                    "Chatbot responded successfully for seller {SellerId}", user.Id);
                return Content(responseBody, "application/json");
            }
            catch (TaskCanceledException)
            {
                _logger.LogWarning("Chatbot request timed out for seller {SellerId}", user.Id);
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    new { error = "AI chatbot service timed out. Please try again." });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Cannot reach chatbot service for seller {SellerId}", user.Id);
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    new { error = "Cannot reach AI chatbot service. Please try again later." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected chatbot error for seller {SellerId}", user.Id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { error = "An unexpected error occurred." });
            }
        }

        // ──────────────────────────────────────────────────────────
        // PRIVATE: Get the authenticated user + their BO profile
        // ──────────────────────────────────────────────────────────
        private async Task<(AppUser user, BusinessOwnerProfile profile)> GetUserAndProfileAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("User is not authenticated");

            var user = await _userManager.Users
                .Include(u => u.BusinessOwnerProfile)
                .FirstOrDefaultAsync(u => u.Id == userId)
                ?? throw new UnauthorizedAccessException("User not found");

            var profile = user.BusinessOwnerProfile
                ?? throw new InvalidOperationException("Business owner profile not found");

            return (user, profile);
        }
    }
}
