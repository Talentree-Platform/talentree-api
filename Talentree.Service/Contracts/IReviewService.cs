using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using Talentree.Service.DTOs.Common;
using Talentree.Service.DTOs.Reviews;
using Talentree.Service.DTOs.Customer;

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

        // Customer product review endpoints
        Task<Pagination<CustomerReviewDto>> GetProductReviewsAsync(int productId, CustomerReviewFilterDto filter);
        Task<ProductRatingDistributionDto> GetProductRatingDistributionAsync(int productId);
        Task<CustomerReviewDto> CreateReviewAsync(CreateReviewDto dto, List<IFormFile>? photos, string customerUserId);
        Task MarkReviewHelpfulAsync(int reviewId, string customerUserId);
    }
}