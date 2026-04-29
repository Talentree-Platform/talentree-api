using Talentree.Core.Entities;

namespace Talentree.Core.Specifications.SupportSpecifications
{
    public class TicketByIdSpecification : BaseSpecifications<SupportTicket>
    {
        public TicketByIdSpecification(int ticketId)
            : base(t => t.Id == ticketId)
        {
            AddInclude(t => t.BusinessOwner);
            AddInclude(t => t.Messages);
            AddInclude(t => t.Attachments);
            AddInclude("Messages.Sender");
            AddInclude("Messages.Attachments");
        }
    }
}