using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Talentree.Service.Contracts;

namespace Talentree.API.Controllers.Admin
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    [Route("api/admin/ai")]
    [EnableRateLimiting("AdminAi")]
    [ApiController]
    public class AdminAiProxyController : BaseApiController
    {
        private readonly HttpClient _httpClient;
        private readonly IAuditLogService _auditLogService;
        private readonly ILogger<AdminAiProxyController> _logger;

        public AdminAiProxyController(
            IHttpClientFactory factory,
            IAuditLogService auditLogService,
            ILogger<AdminAiProxyController> logger)
        {
            _httpClient = factory.CreateClient("AiService");
            _auditLogService = auditLogService;
            _logger = logger;
        }

        // ═══════════════════════════════════════════════════════════
        // 1. Dashboard Overview
        // ═══════════════════════════════════════════════════════════
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            return await ProxyGetAsync("/admin/dashboard");
        }

        // ═══════════════════════════════════════════════════════════
        // 2. Platform KPIs
        // ═══════════════════════════════════════════════════════════
        [HttpGet("kpis")]
        public async Task<IActionResult> GetKpis()
        {
            return await ProxyGetAsync("/admin/kpis");
        }

        // ═══════════════════════════════════════════════════════════
        // 3. Platform Health Score
        // ═══════════════════════════════════════════════════════════
        [HttpGet("health")]
        public async Task<IActionResult> GetHealth()
        {
            return await ProxyGetAsync("/admin/platform/health");
        }

        // ═══════════════════════════════════════════════════════════
        // 4. Platform Analytics & Trends
        // ═══════════════════════════════════════════════════════════
        [HttpGet("analytics")]
        public async Task<IActionResult> GetAnalytics([FromQuery] string period = "monthly")
        {
            return await ProxyGetAsync($"/admin/analytics?period={period}");
        }

        // ═══════════════════════════════════════════════════════════
        // 5. Revenue Forecast (Model 8)
        // ═══════════════════════════════════════════════════════════
        [HttpGet("forecast")]
        public async Task<IActionResult> GetForecast()
        {
            return await ProxyGetAsync("/admin/analytics/forecast");
        }

        // ═══════════════════════════════════════════════════════════
        // 6. Seller Performance Report
        // ═══════════════════════════════════════════════════════════
        [HttpGet("sellers")]
        public async Task<IActionResult> GetSellers([FromQuery] string sort_by = "risk")
        {
            return await ProxyGetAsync($"/admin/sellers?sort_by={sort_by}");
        }

        [HttpGet("sellers/{sellerId}")]
        public async Task<IActionResult> GetSellerDetails(string sellerId)
        {
            return await ProxyGetAsync($"/admin/sellers/{sellerId}");
        }

        // ═══════════════════════════════════════════════════════════
        // 7. Customer Insights & RFM Segmentation
        // ═══════════════════════════════════════════════════════════
        [HttpGet("customers")]
        public async Task<IActionResult> GetCustomers()
        {
            return await ProxyGetAsync("/admin/customers");
        }

        [HttpGet("rfm-segments")]
        public async Task<IActionResult> GetRfmSegments()
        {
            return await ProxyGetAsync("/admin/customers/segments");
        }

        // ═══════════════════════════════════════════════════════════
        // 8. Category Performance
        // ═══════════════════════════════════════════════════════════
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            return await ProxyGetAsync("/admin/categories");
        }

        [HttpGet("categories/{categoryId}/trend")]
        public async Task<IActionResult> GetCategoryTrend(int categoryId, [FromQuery] string period = "monthly")
        {
            return await ProxyGetAsync($"/admin/categories/{categoryId}/trend?period={period}");
        }

        // ═══════════════════════════════════════════════════════════
        // 9. Price Anomaly Detection (Model 12)
        // ═══════════════════════════════════════════════════════════
        [HttpGet("price-anomalies")]
        public async Task<IActionResult> GetPriceAnomalies()
        {
            return await ProxyGetAsync("/admin/products/price-anomalies");
        }

        // ═══════════════════════════════════════════════════════════
        // 10. Category Demand Forecast (Model 13)
        // ═══════════════════════════════════════════════════════════
        [HttpGet("category-forecast")]
        public async Task<IActionResult> GetCategoryForecast()
        {
            return await ProxyGetAsync("/admin/categories/forecast");
        }

        // ═══════════════════════════════════════════════════════════
        // 11. Report Export Endpoints
        // ═══════════════════════════════════════════════════════════
        [HttpGet("export/kpis")]
        public async Task<IActionResult> ExportKpis([FromQuery] string format = "xlsx")
        {
            var adminId = GetCurrentUserId();
            await _auditLogService.LogActionAsync(
                userId: null,
                adminId: adminId,
                action: "Export KPI Report",
                reason: "Admin downloaded platform KPI report via proxy",
                notes: $"Format: {format}"
            );

            try
            {
                var response = await _httpClient.GetAsync($"/admin/export/kpis?format={format}");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("AI Service KPI export failed with status: {StatusCode}", response.StatusCode);
                    return StatusCode((int)response.StatusCode, "AI Export Failed");
                }

                var stream = await response.Content.ReadAsStreamAsync();
                var contentType = format.ToLower() == "xlsx"
                    ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                    : "text/csv";

                return File(stream, contentType, $"talentree_admin_report.{format}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting KPIs");
                return StatusCode(500, "Error exporting KPIs from AI service");
            }
        }

        [HttpGet("export/financial/{boUserId}")]
        public async Task<IActionResult> ExportFinancial(
            string boUserId,
            [FromQuery] string format = "csv",
            [FromQuery] string? from_date = null,
            [FromQuery] string? to_date = null,
            [FromQuery] string? tx_type = null)
        {
            var adminId = GetCurrentUserId();
            await _auditLogService.LogActionAsync(
                userId: boUserId,
                adminId: adminId,
                action: "Export BO Financial Report",
                reason: "Admin downloaded business owner financial report via proxy",
                notes: $"Format: {format}, DateFrom: {from_date}, DateTo: {to_date}, Type: {tx_type}"
            );

            try
            {
                var queryParams = $"?format={format}";
                if (!string.IsNullOrEmpty(from_date)) queryParams += $"&from_date={from_date}";
                if (!string.IsNullOrEmpty(to_date)) queryParams += $"&to_date={to_date}";
                if (!string.IsNullOrEmpty(tx_type)) queryParams += $"&tx_type={tx_type}";

                var response = await _httpClient.GetAsync($"/ai/export/financial/{boUserId}{queryParams}");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("AI Service Financial export failed with status: {StatusCode}", response.StatusCode);
                    return StatusCode((int)response.StatusCode, "AI Financial Export Failed");
                }

                var stream = await response.Content.ReadAsStreamAsync();
                var contentType = format.ToLower() == "pdf"
                    ? "application/pdf"
                    : "text/csv";

                return File(stream, contentType, $"talentree_bo_{boUserId}_financial_report.{format}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting financial report for BO {BoUserId}", boUserId);
                return StatusCode(500, "Error exporting financial report from AI service");
            }
        }

        // ═══════════════════════════════════════════════════════════
        // 12. Manual Model Training / Refreshes
        // ═══════════════════════════════════════════════════════════
        [HttpPost("train/forecast")]
        public async Task<IActionResult> TrainForecast()
        {
            var adminId = GetCurrentUserId();
            await _auditLogService.LogActionAsync(
                userId: null,
                adminId: adminId,
                action: "Retrain Revenue Forecast Model",
                reason: "Admin triggered manual retraining of revenue forecast model via proxy"
            );

            return await ProxyPostAsync("/admin/train/forecast");
        }

        [HttpPost("train/rfm")]
        public async Task<IActionResult> TrainRfm()
        {
            var adminId = GetCurrentUserId();
            await _auditLogService.LogActionAsync(
                userId: null,
                adminId: adminId,
                action: "Retrain RFM Segmentation Model",
                reason: "Admin triggered manual retraining of RFM segmentation model via proxy"
            );

            return await ProxyPostAsync("/admin/train/rfm");
        }

        [HttpPost("train/all-bo")]
        public async Task<IActionResult> TrainAllBo()
        {
            var adminId = GetCurrentUserId();
            await _auditLogService.LogActionAsync(
                userId: null,
                adminId: adminId,
                action: "Retrain All BO Models",
                reason: "Admin triggered manual retraining of all Business Owner models via proxy"
            );

            return await ProxyPostAsync("/ai/train/all");
        }

        [HttpPost("compute/all-bo")]
        public async Task<IActionResult> ComputeAllBo()
        {
            var adminId = GetCurrentUserId();
            await _auditLogService.LogActionAsync(
                userId: null,
                adminId: adminId,
                action: "Compute All BO Predictions",
                reason: "Admin triggered manual execution of all Business Owner models predictions via proxy"
            );

            return await ProxyPostAsync("/ai/compute/all");
        }

        // ═══════════════════════════════════════════════════════════
        // Helper Proxy Request Dispatchers
        // ═══════════════════════════════════════════════════════════
        private async Task<IActionResult> ProxyGetAsync(string requestPath)
        {
            try
            {
                var response = await _httpClient.GetAsync(requestPath);
                if (!response.IsSuccessStatusCode)
                {
                    var statusCode = (int)response.StatusCode;
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return StatusCode(statusCode, errorContent);
                }
                var content = await response.Content.ReadAsStringAsync();
                return Content(content, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during Admin AI proxy GET call to {Path}", requestPath);
                return StatusCode(500, "Error connecting to AI microservice.");
            }
        }

        private async Task<IActionResult> ProxyPostAsync(string requestPath)
        {
            try
            {
                var response = await _httpClient.PostAsync(requestPath, null);
                if (!response.IsSuccessStatusCode)
                {
                    var statusCode = (int)response.StatusCode;
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return StatusCode(statusCode, errorContent);
                }
                var content = await response.Content.ReadAsStringAsync();
                return Content(content, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during Admin AI proxy POST call to {Path}", requestPath);
                return StatusCode(500, "Error connecting to AI microservice.");
            }
        }
    }
}
