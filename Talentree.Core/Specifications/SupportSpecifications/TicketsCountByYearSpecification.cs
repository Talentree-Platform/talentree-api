using Talentree.Core.Entities;

namespace Talentree.Core.Specifications.SupportSpecifications
{
    public class TicketsCountByYearSpecification : BaseSpecifications<SupportTicket>
    {
        public TicketsCountByYearSpecification(int year)
            : base(t => t.CreatedAt.Year == year)
        {
        }
    }
}