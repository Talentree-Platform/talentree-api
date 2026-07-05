using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Talentree.Core.Exceptions;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs.AI.HelpCenter;

namespace Talentree.Service.Services
{
    /// <summary>
    /// Implementation of the AI Help Center service that acts as a proxy
    /// to the external HuggingFace AI API endpoint (POST /chat).
    /// </summary>
    public class AiHelpCenterService : IAiHelpCenterService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AiHelpCenterService> _logger;
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public AiHelpCenterService(
            HttpClient httpClient,
            ILogger<AiHelpCenterService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<ChatResponseDto> SendChatAsync(ChatRequestDto dto)
        {
            if (dto == null || dto.Messages == null || dto.Messages.Count == 0)
            {
                throw new BadRequestException("Messages list cannot be empty.");
            }

            _logger.LogInformation("Sending AI Help Center chat request with {MessageCount} messages", dto.Messages.Count);

            try
            {
                var jsonPayload = JsonSerializer.Serialize(dto, _jsonOptions);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/chat", content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "AI Help Center service returned HTTP {StatusCode}: {ResponseBody}",
                        (int)response.StatusCode,
                        responseBody);

                    if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        var errorDetail = TryExtractErrorDetail(responseBody);
                        throw new BadRequestException(errorDetail ?? "Invalid request sent to AI Help Center.");
                    }

                    throw new HttpRequestException($"AI Help Center service returned status code {(int)response.StatusCode}.");
                }

                var chatResponse = JsonSerializer.Deserialize<ChatResponseDto>(responseBody, _jsonOptions);
                if (chatResponse == null || string.IsNullOrEmpty(chatResponse.Reply))
                {
                    _logger.LogWarning("AI Help Center returned empty or null reply response: {ResponseBody}", responseBody);
                    return new ChatResponseDto { Reply = "Sorry, I couldn't generate a reply. Please try again." };
                }

                _logger.LogInformation("Successfully received AI Help Center response.");
                return chatResponse;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogWarning(ex, "Request to AI Help Center timed out.");
                throw new HttpRequestException("AI Help Center request timed out. Please try again later.", ex);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network or HTTP error communicating with AI Help Center.");
                throw;
            }
            catch (BadRequestException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred during AI Help Center call.");
                throw new HttpRequestException("An error occurred while communicating with the AI Help Center service.", ex);
            }
        }

        private static string? TryExtractErrorDetail(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("detail", out var detailElement))
                {
                    return detailElement.GetString();
                }
            }
            catch
            {
                // Ignore parsing errors
            }
            return null;
        }
    }
}
