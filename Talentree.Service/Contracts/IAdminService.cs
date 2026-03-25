// Talentree.Service/Contracts/IAdminService.cs

using Talentree.Service.DTOs.Admin;
using Talentree.Service.DTOs.Admin.Product;
using Talentree.Service.DTOs.Common;

namespace Talentree.Service.Contracts
{
    public interface IAdminService
    {
        Task<Pagination<BusinessOwnerApplicationDto>> GetPendingBusinessOwnersAsync(
            int pageIndex = 1,
            int pageSize = 20);

        Task<BusinessOwnerApplicationDto> GetBusinessOwnerDetailsAsync(int profileId);

        Task ApproveBusinessOwnerAsync(ApproveBusinessOwnerDto dto, string adminUserId);

        Task RejectBusinessOwnerAsync(RejectBusinessOwnerDto dto, string adminUserId);


        // ═══════════════════════════════════════════════════════════
        // ADMIN MANAGEMENT
        // ═══════════════════════════════════════════════════════════

        Task<AdminDto> CreateAdminAsync(CreateAdminDto dto);

        Task<List<AdminDto>> GetAllAdminsAsync();

        Task DeactivateAdminAsync(string adminUserId);

        Task ReactivateAdminAsync(string adminUserId);

        // ═══════════════════════════════════════════════════════════
        // PRODUCT APPROVAL (NEW)
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Get pending products awaiting approval
        /// </summary>
        Task<Pagination<PendingProductDto>> GetPendingProductsAsync(
            int pageIndex = 1, int pageSize = 20);

        /// <summary>
        /// Approve product
        /// Sends notification to business owner
        /// </summary>
        Task ApproveProductAsync(ApproveProductDto dto, string adminId);

        /// <summary>
        /// Reject product with reason
        /// Sends notification to business owner
        /// </summary>
        Task RejectProductAsync(RejectProductDto dto, string adminId);
    }
}