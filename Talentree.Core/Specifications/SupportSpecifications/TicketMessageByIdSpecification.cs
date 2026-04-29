using Talentree.Core.Entities;

namespace Talentree.Core.Specifications.SupportSpecifications
{
    public class TicketMessageByIdSpecification : BaseSpecifications<TicketMessage>
    {
        public TicketMessageByIdSpecification(int messageId)
            : base(m => m.Id == messageId)
        {
            AddInclude(m => m.Sender);
            AddInclude(m => m.Attachments);
        }
    }
}