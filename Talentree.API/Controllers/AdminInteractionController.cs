using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using Talentree.API.Models;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs.Common;
using Talentree.Service.DTOs.UserInteraction;

namespace Talentree.API.Controllers
{

    [Authorize(Roles = "Admin")]
    [Route("api/admin/interactions")]
    [ApiController]
    public class AdminInteractionController : BaseApiController
    {
        private readonly IUserInteractionService _interactionService;

        [HttpGet("export")]
        public async Task<ActionResult> ExportInteractions(
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate)
        {
            var interactions = await _interactionService.ExportInteractionsAsync(fromDate, toDate);

            var json = JsonConvert.SerializeObject(interactions, Formatting.Indented);
            var bytes = Encoding.UTF8.GetBytes(json);

            return File(bytes, "application/json",
                $"interactions_{fromDate:yyyy-MM-dd}_{toDate:yyyy-MM-dd}.json");
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult<ApiResponse<Pagination<UserInteractionDto>>>>
            GetUserInteractions(string userId, [FromQuery] int pageIndex = 1)
        {
            var result = await _interactionService.GetUserInteractionsAsync(
                userId, pageIndex: pageIndex);

            return Ok(ApiResponse<Pagination<UserInteractionDto>>.SuccessResponse(result));
        }
    }
}
