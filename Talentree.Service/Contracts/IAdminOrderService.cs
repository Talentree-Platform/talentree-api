using System.Threading.Tasks;
using Talentree.Service.DTOs.Admin.Orders;
using Talentree.Service.DTOs.Common;

namespace Talentree.Service.Contracts
{
    public interface IAdminOrderService
    {
        Task<Pagination<AdminOrderSummaryDto>> GetOrdersAsync(AdminOrderFilterDto filter);
        Task<AdminOrderDetailDto> GetOrderByIdAsync(int orderId);
        Task<AdminOrderStatsDto> GetOrderStatsAsync();
        Task<AdminOrderDetailDto> UpdateOrderStatusAsync(int orderId, UpdateOrderStatusDto dto, string adminId);
        Task<AdminOrderDetailDto> AddAdminNoteAsync(int orderId, string note, string adminId);
        Task<byte[]> ExportOrdersToCsvAsync(AdminOrderFilterDto filter);
    }
}
