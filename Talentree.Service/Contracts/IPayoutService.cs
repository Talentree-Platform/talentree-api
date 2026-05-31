using Talentree.Core.Enums;
using Talentree.Service.DTOs.Common;
using Talentree.Service.DTOs.Payout;

namespace Talentree.Service.Contracts
{
    /// <summary>Manages BO payout requests and Admin payout processing.</summary>
    public interface IPayoutService
    {
        /// <summary>Creates a payout request. Enforces one-pending-at-a-time rule.</summary>
        Task<PayoutRequestDto> CreateRequestAsync(string boId, CreatePayoutRequestDto dto);

        /// <summary>Returns paginated payout history for a BO.</summary>
        Task<Pagination<PayoutRequestDto>> GetBoHistoryAsync(string boId, int pageIndex, int pageSize);

        // Admin
        Task<Pagination<PayoutRequestDto>> GetAllAsync(PayoutStatus? status, int pageIndex, int pageSize);
        Task<PayoutRequestDto> ApproveAsync(int requestId, string adminId);
        Task<PayoutRequestDto> RejectAsync(int requestId, string adminId, RejectPayoutDto dto);
        Task<PayoutRequestDto> CompleteAsync(int requestId, string adminId);
    }
}