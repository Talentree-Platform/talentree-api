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
    }
}