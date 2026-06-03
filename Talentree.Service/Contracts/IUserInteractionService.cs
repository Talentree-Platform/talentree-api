using Talentree.Core.Enums;
using Talentree.Service.DTOs.Common;
using Talentree.Service.DTOs.UserInteraction;

namespace Talentree.Service.Contracts
{
    public interface IUserInteractionService
    {
        /// <summary>
        /// Log a user interaction (fire-and-forget)
        /// Used by other services to record actions
        /// </summary>
        Task LogInteractionAsync(
            string userId,
            UserInteractionType userType,
            int itemId,
            UserInteractionItemType itemType,
            UserInteractionActionType actionType,
            string category,
            int quantity = 1,
            decimal price = 0);

        /// <summary>
        /// Get user's interaction history
        /// For analytics and debugging
        /// </summary>
        Task<Pagination<UserInteractionDto>> GetUserInteractionsAsync(
            string userId,
            DateTime? fromDate = null,
            UserInteractionActionType? actionType = null,
            int pageIndex = 1,
            int pageSize = 20);

        /// <summary>
        /// Get interactions for specific item
        /// For analytics
        /// </summary>
        Task<List<UserInteractionDto>> GetItemInteractionsAsync(
            int itemId,
            UserInteractionItemType itemType);

        /// <summary>
        /// Export interactions for AI team (batch)
        /// Date range export for model training
        /// </summary>
        Task<List<UserInteractionExportDto>> ExportInteractionsAsync(
            DateTime fromDate,
            DateTime toDate);
    }
}