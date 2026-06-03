// 📁 Talentree.Core/Specifications/UserInteractionSpecifications/UserInteractionsSpecification.cs

using System;
using System.Collections.Generic;
using Talentree.Core.Entities;
using Talentree.Core.Enums;

namespace Talentree.Core.Specifications.UserInteractionSpecifications
{
    /// <summary>
    /// Get user interactions for recommendations
    /// Filters by user, date range, interaction type
    /// </summary>
    public class UserInteractionsSpecification : BaseSpecifications<UserInteraction>
    {
        public UserInteractionsSpecification(
            string userId,
            DateTime? fromDate = null,
            UserInteractionActionType? actionType = null,
            int pageIndex = 1,
            int pageSize = 100)
            : base(ui =>
                ui.UserId == userId &&
                (fromDate == null || ui.InteractionTimestamp >= fromDate) &&
                (actionType == null || ui.ActionType == actionType))
        {
            AddOrderByDescending(ui => ui.InteractionTimestamp);
            ApplyPagination(pageIndex, pageSize);
        }
    }




}