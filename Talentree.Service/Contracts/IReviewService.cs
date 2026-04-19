using Talentree.Service.DTOs.Common;
using Talentree.Service.DTOs.Reviews;

namespace Talentree.Service.Contracts
{
    public interface IReviewService
    {
        // FR-BO-24: View reviews with filter/sort/search/pagination
        Task<Pagination<ReviewDto>> GetMyReviewsAsync(
            string businessOwnerUserId,
            ReviewFilterDto filter);

        // FR-BO-25: Respond to a review
        Task<ReviewDto> RespondToReviewAsync(
            int reviewId,
            RespondToReviewDto dto,
            string businessOwnerUserId);

        // FR-BO-25: Edit response (within 24 hours)
        Task<ReviewDto> EditResponseAsync(
            int reviewId,
            RespondToReviewDto dto,
            string businessOwnerUserId);

        // FR-BO-26: Analytics
        Task<ReviewAnalyticsDto> GetReviewAnalyticsAsync(string businessOwnerUserId);
    }
}