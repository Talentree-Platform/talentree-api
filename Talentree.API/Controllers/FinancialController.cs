using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Talentree.API.Models;
using Talentree.Core.Enums;
using Talentree.Service.Contracts;

namespace Talentree.API.Controllers
{
    /// <summary>
    /// Provides the financial dashboard and transaction history for Business Owners.
    /// Reads from the Transactions ledger — read-only endpoints.
    /// </summary>
    [Authorize(Roles = "BusinessOwner")]
    public class FinancialController : BaseApiController
    {
        private readonly IFinancialService _financialService;

        public FinancialController(IFinancialService financialService)
            => _financialService = financialService;

        /// <summary>
        /// Returns financial summary cards for the dashboard.
        /// Default period: last 30 days.
        /// </summary>
        /// <remarks>
        /// GET /api/financial/summary?from=2026-01-01&amp;to=2026-03-31
        /// GET /api/financial/summary  (defaults to last 30 days)
        /// </remarks>
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            var periodFrom = from ?? DateTime.UtcNow.AddDays(-30);
            var periodTo = to ?? DateTime.UtcNow;

            var result = await _financialService.GetSummaryAsync(GetBoId(), periodFrom, periodTo);
            return Ok(ApiResponse<object>.SuccessResponse(result));
        }

        /// <summary>
        /// Returns paginated transaction history for the authenticated BO.
        /// Optionally filtered by transaction type.
        /// </summary>
        /// <remarks>
        /// GET /api/financial/transactions?pageIndex=1&amp;pageSize=20
        /// GET /api/financial/transactions?type=MaterialPurchase
        /// </remarks>
        [HttpGet("transactions")]
        public async Task<IActionResult> GetTransactions(
            [FromQuery] TransactionType? type,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _financialService.GetTransactionHistoryAsync(
                GetBoId(), type, pageIndex, pageSize);
            return Ok(ApiResponse<object>.SuccessResponse(result));
        }

        private string GetBoId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    }
}