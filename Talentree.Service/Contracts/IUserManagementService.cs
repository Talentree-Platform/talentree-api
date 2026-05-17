using Talentree.Core.Entities;
using Talentree.Core.Enums;
using Talentree.Service.DTOs;
using Talentree.Service.DTOs.Common;
using Talentree.Service.DTOs.UserManagement;

namespace Talentree.Service.Contracts
{
    public interface IUserManagementService
    {
        // ═══════════════════════════════════════════════════════════
        // BUSINESS OWNER MANAGEMENT
        // ═══════════════════════════════════════════════════════════

        Task<Pagination<BusinessOwnerListDto>> GetBusinessOwnersAsync(
            string? searchQuery = null,
            ApprovalStatus? status = null,
            AccountStatus? accountStatus = null,
            string? category = null,
            DateTime? registrationDateFrom = null,
            DateTime? registrationDateTo = null,
            int pageIndex = 1,
            int pageSize = 20);

        Task<BusinessOwnerDetailsDto> GetBusinessOwnerDetailsAsync(string userId);
        Task SuspendBusinessOwnerAsync(SuspendUserDto dto, string adminId);
        Task UnsuspendBusinessOwnerAsync(string userId, string adminId);
        Task BanBusinessOwnerAsync(BanUserDto dto, string adminId);
        Task BlockBusinessOwnerAsync(BlockUserDto dto, string adminId);
        Task UnblockBusinessOwnerAsync(string userId, string adminId);

        // ═══════════════════════════════════════════════════════════
        // CUSTOMER MANAGEMENT
        // ═══════════════════════════════════════════════════════════

        Task<Pagination<CustomerListDto>> GetCustomersAsync(
            string? searchQuery = null,
            AccountStatus? accountStatus = null,
            DateTime? registrationDateFrom = null,
            DateTime? registrationDateTo = null,
            int pageIndex = 1,
            int pageSize = 20);

        Task<CustomerDetailsDto> GetCustomerDetailsAsync(string userId);
        Task BlockCustomerAsync(BlockUserDto dto, string adminId);
        Task UnblockCustomerAsync(string userId, string adminId);
        Task DeleteCustomerAsync(string userId, string adminId);

        // ═══════════════════════════════════════════════════════════
        // COMPLAINT MANAGEMENT
        // ═══════════════════════════════════════════════════════════

        Task<ComplaintDto> CreateComplaintAsync(CreateComplaintDto dto, string reportedByUserId);
        Task<Pagination<ComplaintDto>> GetComplaintsAsync(
            string? reportedUserId = null,
            ComplaintStatus? status = null,
            ViolationType? violationType = null,
            int pageIndex = 1,
            int pageSize = 20);
        Task<ComplaintDto> GetComplaintByIdAsync(int complaintId);
        Task ResolveComplaintAsync(ResolveComplaintDto dto, string adminId);
        Task RejectComplaintAsync(int complaintId, string reason, string adminId);

        // ═══════════════════════════════════════════════════════════
        // AUTO-BLOCK SYSTEM
        // ═══════════════════════════════════════════════════════════

        Task CheckAndApplyAutoBlockRulesAsync(string userId);
        Task<List<AutoBlockLog>> GetPendingAutoBlocksAsync();
        Task ReviewAutoBlockAsync(AutoBlockReviewDto dto, string adminId);

        // ═══════════════════════════════════════════════════════════
        // USER ACTION LOGS
        // ═══════════════════════════════════════════════════════════

        Task<List<UserActionLogDto>> GetUserActionLogsAsync(string userId);
    }
}