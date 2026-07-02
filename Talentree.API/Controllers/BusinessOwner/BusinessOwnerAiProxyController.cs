using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Talentree.API.Models;

namespace Talentree.API.Controllers.BusinessOwner
{
    /// <summary>
    /// Pass-through proxy that forwards Business Owner AI dashboard requests
    /// to the Talentree FastAPI AI microservice and returns the response verbatim.
    ///
    /// Security model:
    ///   - Endpoint is restricted to authenticated BusinessOwners only.
    ///   - The BO user id is always read from the validated JWT claims;
    ///     it is NEVER accepted as a route/query parameter to prevent IDOR attacks.
    /// </summary>
    [Authorize(Roles = "BusinessOwner")]
    [Route("api/bo/ai")]
    [ApiController]
    public class BusinessOwnerAiProxyController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<BusinessOwnerAiProxyController> _logger;

        public BusinessOwnerAiProxyController(
            IHttpClientFactory httpClientFactory,
            ILogger<BusinessOwnerAiProxyController> logger)
        {
            _httpClient = httpClientFactory.CreateClient("AiService");
            _logger = logger;
        }

        // ──────────────────────────────────────────────────────────
        // PRIVATE HELPER: Extract the authenticated BO user id
        // ──────────────────────────────────────────────────────────
        private string GetBoId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User is not authenticated");

        // ══════════════════════════════════════════════════════════
        // GET /api/bo/ai/dashboard
        // Proxies: GET /ai/dashboard/{bo_user_id}
        // Returns the full AI-generated BO dashboard payload
        // ══════════════════════════════════════════════════════════
        [HttpGet("dashboard")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> GetDashboard()
            => await ProxyGetAsync($"/ai/dashboard/{GetBoId()}", "dashboard");

        // ══════════════════════════════════════════════════════════
        // GET /api/bo/ai/analytics/revenue-trend
        // Proxies: GET /ai/analytics/revenue-trend/{bo_user_id}
        // Returns the AI-computed revenue trend series for this BO
        // ══════════════════════════════════════════════════════════
        [HttpGet("analytics/revenue-trend")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> GetRevenueTrend()
            => await ProxyGetAsync($"/ai/analytics/revenue-trend/{GetBoId()}", "revenue-trend");

        // ══════════════════════════════════════════════════════════
        // GET /api/bo/ai/reviews/trends
        // Proxies: GET /ai/reviews/trends/{bo_user_id}
        // Returns the AI-computed sentiment & review trend data
        // ══════════════════════════════════════════════════════════
        [HttpGet("reviews/trends")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> GetReviewTrends()
            => await ProxyGetAsync($"/ai/reviews/trends/{GetBoId()}", "review-trends");

        // ══════════════════════════════════════════════════════════
        // GET /api/bo/ai/benchmark
        // Proxies: GET /ai/benchmark/{bo_user_id}
        // Returns the AI-generated competitive benchmark score for this BO
        // ══════════════════════════════════════════════════════════
        [HttpGet("benchmark")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> GetBenchmark()
            => await ProxyGetAsync($"/ai/benchmark/{GetBoId()}", "benchmark");

        // ══════════════════════════════════════════════════════════
        // GET /api/bo/ai/export/financial
        // Proxies: GET /ai/export/financial/{bo_user_id}
        // Streams the AI-generated financial export file (CSV or PDF)
        //
        // Query params (forwarded to AI service):
        //   format    - "csv" (default) | "pdf"
        //   from_date - ISO 8601 date string (optional)
        //   to_date   - ISO 8601 date string (optional)
        //   tx_type   - transaction type filter (optional)
        // ══════════════════════════════════════════════════════════
        [HttpGet("export/financial")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> ExportFinancial(
            [FromQuery] string format = "csv",
            [FromQuery] string? from_date = null,
            [FromQuery] string? to_date = null,
            [FromQuery] string? tx_type = null)
        {
            var boId = GetBoId();

            // Build query string — only append params that were actually provided
            var query = $"?format={Uri.EscapeDataString(format)}";
            if (!string.IsNullOrEmpty(from_date)) query += $"&from_date={Uri.EscapeDataString(from_date)}";
            if (!string.IsNullOrEmpty(to_date))   query += $"&to_date={Uri.EscapeDataString(to_date)}";
            if (!string.IsNullOrEmpty(tx_type))   query += $"&tx_type={Uri.EscapeDataString(tx_type)}";

            var aiPath = $"/ai/export/financial/{boId}{query}";

            try
            {
                _logger.LogInformation(
                    "BO {BoId} requesting financial export. Format: {Format}", boId, format);

                var response = await _httpClient.GetAsync(aiPath);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "AI financial export returned {StatusCode} for BO {BoId}",
                        response.StatusCode, boId);
                    return StatusCode(
                        (int)response.StatusCode,
                        ApiResponse<object>.ErrorResponse("AI service could not generate the export"));
                }

                // Determine content type and suggested filename from the requested format
                var contentType   = format.Equals("pdf", StringComparison.OrdinalIgnoreCase)
                    ? "application/pdf"
                    : "text/csv";
                var fileName = $"talentree_financial_{DateTime.UtcNow:yyyyMMdd}.{format.ToLower()}";

                var stream = await response.Content.ReadAsStreamAsync();
                return File(stream, contentType, fileName);
            }
            catch (TaskCanceledException)
            {
                _logger.LogWarning("AI financial export timed out for BO {BoId}", boId);
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    ApiResponse<object>.ErrorResponse("AI service request timed out"));
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "AI financial export connection error for BO {BoId}", boId);
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    ApiResponse<object>.ErrorResponse("Cannot reach AI service"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in financial export proxy for BO {BoId}", boId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.ErrorResponse("Internal error processing AI export"));
            }
        }

        // ──────────────────────────────────────────────────────────
        // PRIVATE: Generic GET proxy helper
        // ──────────────────────────────────────────────────────────
        private async Task<IActionResult> ProxyGetAsync(string aiPath, string operationName)
        {
            try
            {
                _logger.LogDebug("Proxying AI GET {OperationName} → {AiPath}", operationName, aiPath);

                var response = await _httpClient.GetAsync(aiPath);
                var body = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "AI service returned {StatusCode} for {OperationName}. Body: {Body}",
                        response.StatusCode, operationName, body);
                    return StatusCode(
                        (int)response.StatusCode,
                        ApiResponse<object>.ErrorResponse($"AI service error for {operationName}"));
                }

                // Return raw JSON body from the AI service — no re-serialisation
                return Content(body, "application/json");
            }
            catch (TaskCanceledException)
            {
                _logger.LogWarning("AI proxy timeout for {OperationName}", operationName);
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    ApiResponse<object>.ErrorResponse("AI service request timed out"));
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "AI proxy connection error for {OperationName}", operationName);
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    ApiResponse<object>.ErrorResponse("Cannot reach AI service"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected AI proxy error for {OperationName}", operationName);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.ErrorResponse("Internal error communicating with AI service"));
            }
        }
    }
}
