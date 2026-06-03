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
    /// Count user interactions (for pagination)
    /// </summary>
    public class UserInteractionsCountSpecification : BaseSpecifications<UserInteraction>
    {
        public UserInteractionsCountSpecification(
            string userId,
            DateTime? fromDate = null,
            UserInteractionActionType? actionType = null)
            : base(ui =>
                ui.UserId == userId &&
                (fromDate == null || ui.InteractionTimestamp >= fromDate) &&
                (actionType == null || ui.ActionType == actionType))
        {
        }
    }
}
