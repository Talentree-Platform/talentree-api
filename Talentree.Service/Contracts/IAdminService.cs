// Talentree.Service/Contracts/IAdminService.cs

using Talentree.Service.DTOs.Admin;
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

        //Create a new admin user (Admin only)
        Task<AdminDto> CreateAdminAsync(CreateAdminDto dto);

        // Get all admins (Admin only)
        Task<List<AdminDto>> GetAllAdminsAsync();

        // Deactivate admin account (Admin only)
        Task DeactivateAdminAsync(string adminUserId);

        // Reactivate admin account (Admin only)
        Task ReactivateAdminAsync(string adminUserId);
    }
}