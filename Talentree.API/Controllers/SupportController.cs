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
    [Authorize(Roles = "BusinessOwner")]
    public class SupportController : BaseApiController
    {
        private readonly ISupportService _supportService;

        public SupportController(ISupportService supportService)
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

        // ═══════════════════════════════════════════════════════════
        // TICKET MANAGEMENT
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Create a new support ticket
        /// </summary>
        [HttpPost("tickets")]
        [ProducesResponseType(typeof(ApiResponse<TicketDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<TicketDto>>> CreateTicket([FromForm] CreateTicketDto dto)
        {
            var userId = GetCurrentUserId();
            var ticket = await _supportService.CreateTicketAsync(dto, userId);

            return Created($"/api/support/tickets/{ticket.Id}",
                ApiResponse<TicketDto>.SuccessResponse(
                    data: ticket,
                    message: $"Ticket {ticket.TicketNumber} created successfully"
                ));
        }

        /// <summary>
        /// Get my support tickets
        /// </summary>
        [HttpGet("tickets")]
        [ProducesResponseType(typeof(ApiResponse<Pagination<TicketListItemDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<Pagination<TicketListItemDto>>>> GetMyTickets(
            [FromQuery] TicketStatus? status = null,
            [FromQuery] TicketCategory? category = null,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 20)
        {
            var userId = GetCurrentUserId();
            var tickets = await _supportService.GetMyTicketsAsync(userId, status, category, pageIndex, pageSize);

            return Ok(ApiResponse<Pagination<TicketListItemDto>>.SuccessResponse(
                data: tickets,
                message: $"Retrieved {tickets.Data.Count} ticket(s)"
            ));
        }

        /// <summary>
        /// Get ticket by ID
        /// </summary>
        [HttpGet("tickets/{id}")]
        [ProducesResponseType(typeof(ApiResponse<TicketDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<TicketDto>>> GetTicketById(int id)
        {
            var userId = GetCurrentUserId();
            var ticket = await _supportService.GetTicketByIdAsync(id, userId);

            return Ok(ApiResponse<TicketDto>.SuccessResponse(
                data: ticket,
                message: "Ticket retrieved successfully"
            ));
        }

        /// <summary>
        /// Add message/reply to ticket
        /// </summary>
        [HttpPost("tickets/messages")]
        [ProducesResponseType(typeof(ApiResponse<TicketMessageDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<TicketMessageDto>>> AddMessage([FromForm] AddTicketMessageDto dto)
        {
            var userId = GetCurrentUserId();
            var message = await _supportService.AddMessageAsync(dto, userId);

            return Ok(ApiResponse<TicketMessageDto>.SuccessResponse(
                data: message,
                message: "Message added successfully"
            ));
        }

        /// <summary>
        /// Close ticket
        /// </summary>
        [HttpPut("tickets/{id}/close")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> CloseTicket(int id)
        {
            var userId = GetCurrentUserId();
            await _supportService.CloseTicketAsync(id, userId);

            return Ok(ApiResponse<object>.SuccessResponse(
                message: "Ticket closed successfully"
            ));
        }

        // ═══════════════════════════════════════════════════════════
        // FAQ
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Get all FAQs
        /// </summary>
        [HttpGet("faqs")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<List<FAQDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<FAQDto>>>> GetFAQs([FromQuery] string? category = null)
        {
            var faqs = await _supportService.GetFAQsAsync(category);

            return Ok(ApiResponse<List<FAQDto>>.SuccessResponse(
                data: faqs,
                message: $"Retrieved {faqs.Count} FAQ(s)"
            ));
        }

        /// <summary>
        /// Search FAQs
        /// </summary>
        [HttpGet("faqs/search")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<List<FAQDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<FAQDto>>>> SearchFAQs([FromQuery] string q)
        {
            var faqs = await _supportService.SearchFAQsAsync(q);

            return Ok(ApiResponse<List<FAQDto>>.SuccessResponse(
                data: faqs,
                message: $"Found {faqs.Count} FAQ(s)"
            ));
        }

        /// <summary>
        /// Get FAQ by ID
        /// </summary>
        [HttpGet("faqs/{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<FAQDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<FAQDto>>> GetFAQById(int id)
        {
            var faq = await _supportService.GetFAQByIdAsync(id);

            // Increment view count
            await _supportService.IncrementFAQViewCountAsync(id);

            return Ok(ApiResponse<FAQDto>.SuccessResponse(
                data: faq,
                message: "FAQ retrieved successfully"
            ));
        }

        /// <summary>
        /// Get FAQ categories
        /// </summary>
        [HttpGet("faqs/categories")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<List<FAQCategoryDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<FAQCategoryDto>>>> GetFAQCategories()
        {
            var categories = await _supportService.GetFAQCategoriesAsync();

            return Ok(ApiResponse<List<FAQCategoryDto>>.SuccessResponse(
                data: categories,
                message: $"Retrieved {categories.Count} categor(ies)"
            ));
        }
    }
}