using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Talentree.API.Models;
using Talentree.Core.Enums;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs;
using Talentree.Service.DTOs.Common;
using Talentree.Service.DTOs.Support;

namespace Talentree.API.Controllers
{
    [Route("api/admin/support")]
    [Authorize(Roles = "Admin")]
    public class AdminSupportController : BaseApiController
    {
        private readonly ISupportService _supportService;

        public AdminSupportController(ISupportService supportService)
        {
            _supportService = supportService;
        }
        protected string GetCurrentUserId()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User is not authenticated");

            return userId;
        }


        /// <summary>
        /// Get all tickets (admin view)
        /// </summary>
        [HttpGet("tickets")]
        [ProducesResponseType(typeof(ApiResponse<Pagination<TicketListItemDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<Pagination<TicketListItemDto>>>> GetAllTickets(
            [FromQuery] TicketStatus? status = null,
            [FromQuery] TicketPriority? priority = null,
            [FromQuery] string? assignedToAdminId = null,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 20)
        {
            var tickets = await _supportService.GetAllTicketsAsync(status, priority, assignedToAdminId, pageIndex, pageSize);

            return Ok(ApiResponse<Pagination<TicketListItemDto>>.SuccessResponse(
                data: tickets,
                message: $"Retrieved {tickets.Data.Count} ticket(s)"
            ));
        }

        /// <summary>
        /// Update ticket status
        /// </summary>
        [HttpPut("tickets/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> UpdateTicketStatus([FromBody] UpdateTicketStatusDto dto)
        {
            var adminId = GetCurrentUserId();
            await _supportService.UpdateTicketStatusAsync(dto, adminId);

            return Ok(ApiResponse<object>.SuccessResponse(
                message: "Ticket status updated successfully. Notification sent to business owner."
            ));
        }

        /// <summary>
        /// Assign ticket to admin
        /// </summary>
        [HttpPut("tickets/assign")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> AssignTicket([FromBody] AssignTicketDto dto)
        {
            var adminId = GetCurrentUserId();
            await _supportService.AssignTicketAsync(dto, adminId);

            return Ok(ApiResponse<object>.SuccessResponse(
                message: "Ticket assigned successfully. Notification sent to admin."
            ));
        }

        /// <summary>
        /// Update ticket priority
        /// </summary>
        [HttpPut("tickets/{id}/priority")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> UpdateTicketPriority(
            int id,
            [FromBody] TicketPriority priority)
        {
            var adminId = GetCurrentUserId();
            await _supportService.UpdateTicketPriorityAsync(id, priority, adminId);

            return Ok(ApiResponse<object>.SuccessResponse(
                message: "Ticket priority updated successfully"
            ));
        }
    }
}