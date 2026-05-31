using Talentree.Core.Entities;
using Talentree.Core.Enums;

namespace Talentree.Core.Specifications.SupportSpecifications
{
    public class AllTicketsSpecification : BaseSpecifications<SupportTicket>
    {
        public AllTicketsSpecification(
            TicketStatus? status = null,
            TicketPriority? priority = null,
            string? assignedToAdminId = null,
            int? pageIndex = null,
            int? pageSize = null)
            : base(t =>
                (!status.HasValue || t.Status == status.Value) &&
                (!priority.HasValue || t.Priority == priority.Value) &&
                (string.IsNullOrEmpty(assignedToAdminId) || t.AssignedToAdminId == assignedToAdminId))
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