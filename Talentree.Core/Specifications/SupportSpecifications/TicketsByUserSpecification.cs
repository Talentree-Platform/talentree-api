using Talentree.Core.Entities;
using Talentree.Core.Enums;

namespace Talentree.Core.Specifications.SupportSpecifications
{
    public class TicketsByUserSpecification : BaseSpecifications<SupportTicket>
    {
        public TicketsByUserSpecification(
            string userId,
            TicketStatus? status = null,
            TicketCategory? category = null,
            int? pageIndex = null,
            int? pageSize = null)
            : base(t =>
                t.BusinessOwnerUserId == userId &&
                (!status.HasValue || t.Status == status.Value) &&
                (!category.HasValue || t.Category == category.Value))
        {
            AddInclude(t => t.BusinessOwner);
            AddInclude(t => t.Messages);
            AddInclude(t => t.Attachments);

            AddOrderByDescending(t => t.CreatedAt);

            if (pageIndex.HasValue && pageSize.HasValue)
            {
                ApplyPagination(pageIndex.Value, pageSize.Value);
            }
        }
    }
}