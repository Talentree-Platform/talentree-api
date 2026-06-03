using System.Threading.Tasks;
using Talentree.Core.Enums;
using Talentree.Service.DTOs.Common;
using Talentree.Service.DTOs.Refund;

namespace Talentree.Service.Contracts
{
    public interface IRefundService
    {
        // Customer-facing
        Task<RefundRequestDto> SubmitRefundRequestAsync(int orderId, int itemId, string customerId, CreateRefundRequestDto dto);

        // BO-facing
        Task<Pagination<RefundRequestDto>> GetBoRefundRequestsAsync(string boId, RefundStatus? status, int pageIndex, int pageSize);
        Task<RefundRequestDto> RespondToRefundRequestAsync(int requestId, string boId, RespondRefundDto dto);

        // Admin-facing
        Task<Pagination<RefundRequestDto>> GetPendingRefundsAsync(RefundStatus? status, int pageIndex, int pageSize);
        Task<RefundRequestDto> ApproveRefundAsync(int requestId, string adminId);
        Task<RefundRequestDto> RejectRefundAsync(int requestId, string adminId, RejectRefundDto dto);
    }
}
