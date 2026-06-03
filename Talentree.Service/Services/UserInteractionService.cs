
using AutoMapper;
using Microsoft.Extensions.Logging;
using Talentree.Core;
using Talentree.Core.Entities;
using Talentree.Core.Enums;
using Talentree.Core.Specifications.UserInteractionSpecifications;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs;
using Talentree.Service.DTOs.Common;
using Talentree.Service.DTOs.UserInteraction;

namespace Talentree.Service.Services
{
    /// <summary>
    /// Logs and manages user interactions for AI recommendation system
    /// Records: views, clicks, purchases, reorders
    /// </summary>
    public class UserInteractionService : IUserInteractionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<UserInteractionService> _logger;

        public UserInteractionService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<UserInteractionService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Log user interaction (asynchronously)
        /// Called from other services when user performs action
        /// </summary>
        public async Task LogInteractionAsync(
            string userId,
            UserInteractionType userType,
            int itemId,
            UserInteractionItemType itemType,
            UserInteractionActionType actionType,
            string category,
            int quantity = 1,
            decimal price = 0)
        {
            try
            {
                // Create interaction record
                var interaction = new UserInteraction
                {
                    UserId = userId,
                    UserType = userType,
                    ItemId = itemId,
                    ItemType = itemType,
                    ActionType = actionType,
                    Category = category,
                    Quantity = quantity,
                    Price = price,
                    InteractionTimestamp = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                // Add to database
                _unitOfWork.Repository<UserInteraction>().Add(interaction);
                await _unitOfWork.CompleteAsync();

                _logger.LogInformation(
                    "User {UserId} ({UserType}) performed {ActionType} on {ItemType} {ItemId}",
                    userId, userType, actionType, itemType, itemId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging interaction for user {UserId}", userId);
                // Don't throw - let the main operation continue
            }
        }

        /// <summary>
        /// Get user's interaction history
        /// </summary>
        public async Task<Pagination<UserInteractionDto>> GetUserInteractionsAsync(
            string userId,
            DateTime? fromDate = null,
            UserInteractionActionType? actionType = null,
            int pageIndex = 1,
            int pageSize = 20)
        {
            var countSpec = new UserInteractionsCountSpecification(userId, fromDate, actionType);
            var totalCount = await _unitOfWork.Repository<UserInteraction>()
                .GetCountWithSpecificationsAsync(countSpec);

            var spec = new UserInteractionsSpecification(userId, fromDate, actionType, pageIndex, pageSize);
            var interactions = await _unitOfWork.Repository<UserInteraction>()
                .GetAllWithSpecificationsAsync(spec);

            var dtos = _mapper.Map<List<UserInteractionDto>>(interactions);
            return new Pagination<UserInteractionDto>(pageIndex, pageSize, totalCount, dtos);
        }

        /// <summary>
        /// Get all interactions for specific item
        /// </summary>
        public async Task<List<UserInteractionDto>> GetItemInteractionsAsync(
            int itemId,
            UserInteractionItemType itemType)
        {
            var spec = new ItemInteractionsSpecification(itemId, itemType);
            var interactions = await _unitOfWork.Repository<UserInteraction>()
                .GetAllWithSpecificationsAsync(spec);

            return _mapper.Map<List<UserInteractionDto>>(interactions);
        }

        /// <summary>
        /// Export interactions for AI team (batch processing)
        /// </summary>
        public async Task<List<UserInteractionExportDto>> ExportInteractionsAsync(
            DateTime fromDate,
            DateTime toDate)
        {
            var spec = new InteractionsByDateRangeSpecification(fromDate, toDate);
            var interactions = await _unitOfWork.Repository<UserInteraction>()
                .GetAllWithSpecificationsAsync(spec);

            var dtos = interactions.Select(ui => new UserInteractionExportDto
            {
                user_type = ui.UserType.ToString().ToLower(),
                user_id = ui.UserId,
                item_id = ui.ItemId,
                item_type = ui.ItemType.ToString().ToLower(),
                interaction_type = ui.ActionType.ToString().ToLower(),
                category = ui.Category,
                quantity = ui.Quantity,
                price = ui.Price,
                interaction_timestamp = ui.InteractionTimestamp.ToString("yyyy-MM-dd HH:mm:ss")
            }).ToList();

            _logger.LogInformation("Exported {Count} interactions for date range {FromDate} to {ToDate}",
                dtos.Count, fromDate.Date, toDate.Date);

            return dtos;
        }
    }
}