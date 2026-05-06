using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Talentree.API.Models;
using Talentree.Core.Enums;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs;
using Talentree.Service.DTOs.Common;
using Talentree.Service.DTOs.UserManagement;

namespace Talentree.API.Controllers
{
    [Route("api/admin/users")]
    [Authorize(Roles = "Admin")]
    public class AdminUserManagementController : BaseApiController
    {
        private readonly IUserManagementService _userManagementService;

        public AdminUserManagementController(IUserManagementService userManagementService)
        {
            _userManagementService = userManagementService;
        }

      
        // ═══════════════════════════════════════════════════════════
        // BUSINESS OWNER MANAGEMENT
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Get all business owners (sellers)
        /// </summary>
        [HttpGet("business-owners")]
        [ProducesResponseType(typeof(ApiResponse<Pagination<BusinessOwnerListDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<Pagination<BusinessOwnerListDto>>>> GetBusinessOwners(
            [FromQuery] string? searchQuery = null,
            [FromQuery] ApprovalStatus? status = null,
            [FromQuery] AccountStatus? accountStatus = null,
            [FromQuery] string? category = null,
            [FromQuery] DateTime? registrationDateFrom = null,
            [FromQuery] DateTime? registrationDateTo = null,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 20)
        {
            pageIndex = Math.Max(1, pageIndex);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var result = await _userManagementService.GetBusinessOwnersAsync(
                searchQuery, status, accountStatus, category,
                registrationDateFrom, registrationDateTo, pageIndex, pageSize);

            return Ok(ApiResponse<Pagination<BusinessOwnerListDto>>.SuccessResponse(
                data: result,
                message: $"Retrieved {result.Data.Count} business owner(s)"
            ));
        }

        /// <summary>
        /// Get business owner details
        /// </summary>
        [HttpGet("business-owners/{userId}")]
        [ProducesResponseType(typeof(ApiResponse<BusinessOwnerDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<BusinessOwnerDetailsDto>>> GetBusinessOwnerDetails(string userId)
        {
            var result = await _userManagementService.GetBusinessOwnerDetailsAsync(userId);

            return Ok(ApiResponse<BusinessOwnerDetailsDto>.SuccessResponse(
                data: result,
                message: "Business owner details retrieved successfully"
            ));
        }

        /// <summary>
        /// Suspend business owner account
        /// </summary>
        [HttpPost("business-owners/suspend")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<object>>> SuspendBusinessOwner([FromBody] SuspendUserDto dto)
        {
            var adminId = GetCurrentUserId();
            await _userManagementService.SuspendBusinessOwnerAsync(dto, adminId);

            return Ok(ApiResponse<object>.SuccessResponse(
                message: "Business owner suspended successfully. Notification sent to user."
            ));
        }

        /// <summary>
        /// Unsuspend business owner account
        /// </summary>
        [HttpPost("business-owners/{userId}/unsuspend")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<object>>> UnsuspendBusinessOwner(string userId)
        {
            var adminId = GetCurrentUserId();
            await _userManagementService.UnsuspendBusinessOwnerAsync(userId, adminId);

            return Ok(ApiResponse<object>.SuccessResponse(
                message: "Business owner account reactivated successfully."
            ));
        }

        /// <summary>
        /// Ban business owner account (permanent)
        /// </summary>
        [HttpPost("business-owners/ban")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<object>>> BanBusinessOwner([FromBody] BanUserDto dto)
        {
            var adminId = GetCurrentUserId();
            await _userManagementService.BanBusinessOwnerAsync(dto, adminId);

            return Ok(ApiResponse<object>.SuccessResponse(
                message: "Business owner banned permanently. All products hidden."
            ));
        }

        /// <summary>
        /// Block business owner account
        /// </summary>
        [HttpPost("business-owners/block")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<object>>> BlockBusinessOwner([FromBody] BlockUserDto dto)
        {
            var adminId = GetCurrentUserId();
            await _userManagementService.BlockBusinessOwnerAsync(dto, adminId);

            return Ok(ApiResponse<object>.SuccessResponse(
                message: "Business owner blocked successfully."
            ));
        }

        /// <summary>
        /// Unblock business owner account
        /// </summary>
        [HttpPost("business-owners/{userId}/unblock")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<object>>> UnblockBusinessOwner(string userId)
        {
            var adminId = GetCurrentUserId();
            await _userManagementService.UnblockBusinessOwnerAsync(userId, adminId);

            return Ok(ApiResponse<object>.SuccessResponse(
                message: "Business owner unblocked successfully."
            ));
        }

        // ═══════════════════════════════════════════════════════════
        // CUSTOMER MANAGEMENT
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Get all customers
        /// </summary>
        [HttpGet("customers")]
        [ProducesResponseType(typeof(ApiResponse<Pagination<CustomerListDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<Pagination<CustomerListDto>>>> GetCustomers(
            [FromQuery] string? searchQuery = null,
            [FromQuery] AccountStatus? accountStatus = null,
            [FromQuery] DateTime? registrationDateFrom = null,
            [FromQuery] DateTime? registrationDateTo = null,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 20)
        {
            pageIndex = Math.Max(1, pageIndex);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var result = await _userManagementService.GetCustomersAsync(
                searchQuery, accountStatus, registrationDateFrom, registrationDateTo,
                pageIndex, pageSize);

            return Ok(ApiResponse<Pagination<CustomerListDto>>.SuccessResponse(
                data: result,
                message: $"Retrieved {result.Data.Count} customer(s)"
            ));
        }

        /// <summary>
        /// Get customer details
        /// </summary>
        [HttpGet("customers/{userId}")]
        [ProducesResponseType(typeof(ApiResponse<CustomerDetailsDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<CustomerDetailsDto>>> GetCustomerDetails(string userId)
        {
            var result = await _userManagementService.GetCustomerDetailsAsync(userId);

            return Ok(ApiResponse<CustomerDetailsDto>.SuccessResponse(
                data: result,
                message: "Customer details retrieved successfully"
            ));
        }

        /// <summary>
        /// Block customer account
        /// </summary>
        [HttpPost("customers/block")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<object>>> BlockCustomer([FromBody] BlockUserDto dto)
        {
            var adminId = GetCurrentUserId();
            await _userManagementService.BlockCustomerAsync(dto, adminId);

            return Ok(ApiResponse<object>.SuccessResponse(
                message: "Customer blocked successfully."
            ));
        }

        /// <summary>
        /// Unblock customer account
        /// </summary>
        [HttpPost("customers/{userId}/unblock")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<object>>> UnblockCustomer(string userId)
        {
            var adminId = GetCurrentUserId();
            await _userManagementService.UnblockCustomerAsync(userId, adminId);

            return Ok(ApiResponse<object>.SuccessResponse(
                message: "Customer unblocked successfully."
            ));
        }

        /// <summary>
        /// Delete customer account permanently
        /// </summary>
        [HttpDelete("customers/{userId}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<object>>> DeleteCustomer(string userId)
        {
            var adminId = GetCurrentUserId();
            await _userManagementService.DeleteCustomerAsync(userId, adminId);

            return Ok(ApiResponse<object>.SuccessResponse(
                message: "Customer account deleted permanently."
            ));
        }

        // ═══════════════════════════════════════════════════════════
        // USER ACTION LOGS
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Get user action logs
        /// </summary>
        [HttpGet("{userId}/logs")]
        [ProducesResponseType(typeof(ApiResponse<List<UserActionLogDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<UserActionLogDto>>>> GetUserActionLogs(string userId)
        {
            var logs = await _userManagementService.GetUserActionLogsAsync(userId);

            return Ok(ApiResponse<List<UserActionLogDto>>.SuccessResponse(
                data: logs,
                message: $"Retrieved {logs.Count} action log(s)"
            ));
        }
    }
}