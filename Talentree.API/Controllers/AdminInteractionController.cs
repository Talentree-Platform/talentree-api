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
        public AdminInteractionController(
    IUserInteractionService interactionService)
        {
            _interactionService = interactionService;
        }

        [HttpGet("export")]
        public async Task<ActionResult> ExportInteractions(
          [FromQuery] DateTime fromDate,
          [FromQuery] DateTime toDate)
        {
            try
            {
                if (fromDate > toDate)
                    return BadRequest("fromDate must be before toDate");

                var interactions = await _interactionService.ExportInteractionsAsync(fromDate, toDate);

                if (interactions == null || interactions.Count == 0)
                    return Ok(new { message = "No interactions found in this date range", data = new List<object>() });

                var json = JsonConvert.SerializeObject(interactions, Formatting.Indented);
                var bytes = Encoding.UTF8.GetBytes(json);

                return File(bytes, "application/json",
                    $"interactions_{fromDate:yyyy-MM-dd}_{toDate:yyyy-MM-dd}.json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error exporting interactions", error = ex.Message });
            }
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
