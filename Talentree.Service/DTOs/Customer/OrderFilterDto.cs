using Talentree.Core.Enums;

namespace Talentree.Service.DTOs.Customer
{
    public class OrderFilterDto
    {
        public CustomerOrderStatus? Status { get; set; }
        public string? Search { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
