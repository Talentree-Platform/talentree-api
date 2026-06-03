using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talentree.Core.Entities;
using Talentree.Core.Enums;

namespace Talentree.Core.Specifications.UserInteractionSpecifications
{

    /// <summary>
    /// Get interactions for specific item (for analytics)
    /// </summary>
    public class ItemInteractionsSpecification : BaseSpecifications<UserInteraction>
    {
        public ItemInteractionsSpecification(
            int itemId,
            UserInteractionItemType itemType,
            int pageSize = 1000)
            : base(ui =>
                ui.ItemId == itemId &&
                ui.ItemType == itemType)
        {
            AddOrderByDescending(ui => ui.CreatedAt);
            ApplyPagination(1, pageSize);
        }
    }

}
