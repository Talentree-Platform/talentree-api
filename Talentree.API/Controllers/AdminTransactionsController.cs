using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs.Admin.Transactions;

namespace Talentree.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminTransactionsController : ControllerBase
    {
        private readonly IFinancialService _financialService;

        public AdminTransactionsController(IFinancialService financialService)
        {
            _financialService = financialService;
        }

        [HttpGet]
        public async Task<IActionResult> GetTransactions([FromQuery] AdminTransactionFilterDto filter)
        {
            var result = await _financialService.GetAdminTransactionsAsync(filter);
            return Ok(result);
        }

        [HttpGet("report")]
        public async Task<IActionResult> GetFinancialReport([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var result = await _financialService.GetAdminFinancialReportAsync(from, to);
            return Ok(result);
        }
    }
}
