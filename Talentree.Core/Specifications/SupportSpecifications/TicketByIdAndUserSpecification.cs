using Talentree.Core.Entities;

namespace Talentree.Core.Specifications.SupportSpecifications
{
    public class TicketByIdAndUserSpecification : BaseSpecifications<SupportTicket>
    {
        public TicketByIdAndUserSpecification(int ticketId, string userId)
            : base(t => t.Id == ticketId && t.BusinessOwnerUserId == userId)
        {
            AddInclude(t => t.BusinessOwner);
            AddInclude(t => t.Messages);
            AddInclude(t => t.Attachments);
            AddInclude("Messages.Sender");
            AddInclude("Messages.Attachments");
        }
    }
}