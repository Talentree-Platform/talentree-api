using Talentree.Core;
using Talentree.Core.Entities;
using Talentree.Core.Enums;
using Talentree.Core.Exceptions;
using Talentree.Core.Specifications.BusinessOwnerSpecifications;
using Talentree.Core.Specifications.ReviewSpecifications;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs.Common;
using Talentree.Service.DTOs.Reviews;

namespace Talentree.Service.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ReviewService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // ═══════════════════════════════════════════════════════════
        // PRIVATE HELPER: Get approved BO profile
        // ═══════════════════════════════════════════════════════════
        private async Task<Core.Entities.Identity.BusinessOwnerProfile> GetApprovedProfileAsync(string userId)
        {
            var spec = new BusinessOwnerProfileByUserIdSpecification(userId);
            var profile = await _unitOfWork.Repository<Core.Entities.Identity.BusinessOwnerProfile>()
                .GetByIdWithSpecificationsAsync(spec);

            if (profile == null)
                throw new NotFoundException("Business owner profile not found");

            if (profile.Status != ApprovalStatus.Approved)
                throw new ForbiddenException("Your business account is not approved yet");

            return profile;
        }

        // ═══════════════════════════════════════════════════════════
        // PRIVATE HELPER: Map filter DTO → params
        // ═══════════════════════════════════════════════════════════
        private static ReviewFilterParams MapToParams(ReviewFilterDto filter) => new()
        {
            Rating = filter.Rating,
            ProductId = filter.ProductId,
            Search = filter.Search,
            SortBy = filter.SortBy,
            SortDescending = filter.SortDescending,
            PageIndex = filter.PageIndex,
            PageSize = filter.PageSize
        };

        // ═══════════════════════════════════════════════════════════
        // PRIVATE HELPER: Map review entity → DTO
        // ═══════════════════════════════════════════════════════════
        private static ReviewDto MapToDto(ProductReview r)
        {
            return new ReviewDto
            {
                Id = r.Id,
                ProductId = r.ProductId,
                ProductName = r.Product?.Name ?? string.Empty,
                CustomerName = r.IsAnonymous ? "Anonymous" : r.CustomerName,
                Rating = r.Rating,
                ReviewText = r.ReviewText,
                IsAnonymous = r.IsAnonymous,
                HasResponse = !string.IsNullOrEmpty(r.OwnerResponse),
                OwnerResponse = r.OwnerResponse,
                ResponseAt = r.ResponseAt,
                CanEditResponse = r.ResponseAt.HasValue &&
                                  DateTime.UtcNow <= r.ResponseAt.Value.AddHours(24),
                SentimentScore = r.SentimentScore,
                SentimentLabel = r.SentimentLabel,
                FlaggedToxic = r.FlaggedToxic,
                CreatedAt = r.CreatedAt
            };
        }

        // ═══════════════════════════════════════════════════════════
        // FR-BO-24: Get My Reviews
        // ═══════════════════════════════════════════════════════════
        public async Task<Pagination<ReviewDto>> GetMyReviewsAsync(
            string businessOwnerUserId,
            ReviewFilterDto filter)
        {
            var profile = await GetApprovedProfileAsync(businessOwnerUserId);
            var filterParams = MapToParams(filter);

            var countSpec = new ReviewsByBusinessOwnerSpecification(profile.Id, filterParams, true);
            var totalCount = await _unitOfWork.Repository<ProductReview>()
                .GetCountWithSpecificationsAsync(countSpec);

            var spec = new ReviewsByBusinessOwnerSpecification(profile.Id, filterParams);
            var reviews = await _unitOfWork.Repository<ProductReview>()
                .GetAllWithSpecificationsAsync(spec);

            var dtos = reviews.Select(MapToDto).ToList();

            return new Pagination<ReviewDto>(filter.PageIndex, filter.PageSize, totalCount, dtos);
        }

        // ═══════════════════════════════════════════════════════════
        // FR-BO-25: Respond to Review
        // ═══════════════════════════════════════════════════════════
        public async Task<ReviewDto> RespondToReviewAsync(
            int reviewId,
            RespondToReviewDto dto,
            string businessOwnerUserId)
        {
            var profile = await GetApprovedProfileAsync(businessOwnerUserId);
            var review = await GetOwnedReviewAsync(reviewId, profile.Id);

            // One response per review
            if (!string.IsNullOrEmpty(review.OwnerResponse))
                throw new BadRequestException(
                    "You have already responded to this review. Use the edit endpoint to update it.");

            review.OwnerResponse = dto.Response;
            review.ResponseAt = DateTime.UtcNow;

            _unitOfWork.Repository<ProductReview>().Update(review);
            await _unitOfWork.CompleteAsync();

            return MapToDto(review);
        }

        // ═══════════════════════════════════════════════════════════
        // FR-BO-25: Edit Response (within 24 hours)
        // ═══════════════════════════════════════════════════════════
        public async Task<ReviewDto> EditResponseAsync(
            int reviewId,
            RespondToReviewDto dto,
            string businessOwnerUserId)
        {
            var profile = await GetApprovedProfileAsync(businessOwnerUserId);
            var review = await GetOwnedReviewAsync(reviewId, profile.Id);

            if (string.IsNullOrEmpty(review.OwnerResponse))
                throw new BadRequestException("No response found to edit.");

            // Only editable within 24 hours
            if (review.ResponseAt.HasValue &&
                DateTime.UtcNow > review.ResponseAt.Value.AddHours(24))
                throw new BadRequestException(
                    "Response can only be edited within 24 hours of posting.");

            review.OwnerResponse = dto.Response;
            // Keep original ResponseAt — don't reset the 24h window

            _unitOfWork.Repository<ProductReview>().Update(review);
            await _unitOfWork.CompleteAsync();

            return MapToDto(review);
        }

        // ═══════════════════════════════════════════════════════════
        // FR-BO-26: Review Analytics
        // ═══════════════════════════════════════════════════════════
        public async Task<ReviewAnalyticsDto> GetReviewAnalyticsAsync(string businessOwnerUserId)
        {
            var profile = await GetApprovedProfileAsync(businessOwnerUserId);

            var spec = new AllReviewsByBusinessOwnerSpecification(profile.Id);
            var reviews = await _unitOfWork.Repository<ProductReview>()
                .GetAllWithSpecificationsAsync(spec);

            var list = reviews.ToList();

            if (!list.Any())
            {
                return new ReviewAnalyticsDto
                {
                    AverageRating = 0,
                    TotalReviews = 0,
                    TotalResponded = 0,
                    ResponseRate = 0,
                    Distribution = new RatingDistributionDto(),
                    SentimentTrend = new List<SentimentTrendDto>()
                };
            }

            var totalReviews = list.Count;
            var totalResponded = list.Count(r => !string.IsNullOrEmpty(r.OwnerResponse));
            var averageRating = list.Average(r => (double)r.Rating);
            var responseRate = totalReviews > 0
                ? Math.Round((double)totalResponded / totalReviews * 100, 1)
                : 0;

            var distribution = new RatingDistributionDto
            {
                FiveStar = list.Count(r => r.Rating == 5),
                FourStar = list.Count(r => r.Rating == 4),
                ThreeStar = list.Count(r => r.Rating == 3),
                TwoStar = list.Count(r => r.Rating == 2),
                OneStar = list.Count(r => r.Rating == 1)
            };

            // Sentiment trend — group by month, last 6 months
            var sentimentTrend = list
                .Where(r => r.SentimentScore.HasValue)
                .GroupBy(r => r.CreatedAt.ToString("yyyy-MM"))
                .OrderBy(g => g.Key)
                .TakeLast(6)
                .Select(g => new SentimentTrendDto
                {
                    Month = g.Key,
                    AverageSentiment = Math.Round(g.Average(r => (double)r.SentimentScore!.Value), 2),
                    ReviewCount = g.Count()
                })
                .ToList();

            return new ReviewAnalyticsDto
            {
                AverageRating = Math.Round(averageRating, 2),
                TotalReviews = totalReviews,
                TotalResponded = totalResponded,
                ResponseRate = responseRate,
                Distribution = distribution,
                SentimentTrend = sentimentTrend
            };
        }

        // ═══════════════════════════════════════════════════════════
        // PRIVATE HELPER: Get review and verify it belongs to this BO
        // ═══════════════════════════════════════════════════════════
        private async Task<ProductReview> GetOwnedReviewAsync(int reviewId, int businessOwnerProfileId)
        {
            var spec = new ReviewByIdSpecification(reviewId);
            var review = await _unitOfWork.Repository<ProductReview>()
                .GetByIdWithSpecificationsAsync(spec);

            if (review == null)
                throw new NotFoundException("Review not found");

            if (review.Product?.BusinessOwnerProfileId != businessOwnerProfileId)
                throw new ForbiddenException("This review does not belong to your products");

            return review;
        }
    }
}