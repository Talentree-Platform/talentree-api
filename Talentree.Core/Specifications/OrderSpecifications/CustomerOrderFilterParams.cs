using Talentree.Core.Enums;

namespace Talentree.Core.Specifications.OrderSpecifications
{
    public class CustomerOrderFilterParams
    {
        public CustomerOrderStatus? Status { get; set; }
        public string? Search { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
