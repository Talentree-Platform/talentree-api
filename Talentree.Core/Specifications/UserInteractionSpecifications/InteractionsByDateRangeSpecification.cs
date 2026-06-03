using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talentree.Core.Entities;

namespace Talentree.Core.Specifications.UserInteractionSpecifications
{
    /// <summary>
    /// Get all interactions in date range (for AI batch processing)
    /// </summary>
    public class InteractionsByDateRangeSpecification : BaseSpecifications<UserInteraction>
    {
        public InteractionsByDateRangeSpecification(
            DateTime fromDate,
            DateTime toDate,
            int pageSize = 10000)
            : base(ui =>
                ui.InteractionTimestamp >= fromDate &&
                ui.InteractionTimestamp <= toDate)
        {
            AddOrderBy(ui => ui.CreatedAt);
            ApplyPagination(1, pageSize);
        }
    }
}
