using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Talentree.Core;
using Talentree.Core.Entities;
using Talentree.Core.Entities.Identity;
using Talentree.Core.Enums;
using Talentree.Core.Exceptions;
using Talentree.Core.Specifications.BusinessOwnerSpecifications;
using Talentree.Core.Specifications.ReviewSpecifications;
using Talentree.Core.Specifications.ProductSpecifications;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs.Common;
using Talentree.Service.DTOs.Reviews;
using Talentree.Service.DTOs.Customer;

namespace Talentree.Service.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IImageService _imageService;
        private readonly INotificationService _notificationService;
        private readonly UserManager<AppUser> _userManager;
        private readonly IAIService _aiService;
        public ReviewService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IImageService imageService,
            INotificationService notificationService,
            UserManager<AppUser> userManager,
            IAIService aiService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _imageService = imageService;
            _notificationService = notificationService;
            _userManager = userManager;
            _aiService = aiService;
        }

        // ═══════════════════════════════════════════════════════════
        // PRIVATE HELPER: Get approved BO profile
        // ═══════════════════════════════════════════════════════════
        private async Task<BusinessOwnerProfile> GetApprovedProfileAsync(string userId)
        {
            var spec = new BusinessOwnerProfileByUserIdSpecification(userId);
            var profile = await _unitOfWork.Repository<BusinessOwnerProfile>()
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
            
            _ = Task.Run(() => _aiService.PredictSentimentAsync(review.Id));
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
                    Distribution = new ReviewRatingDistributionDto(),
                    SentimentTrend = new List<SentimentTrendDto>()
                };
            }

            var totalReviews = list.Count;
            var totalResponded = list.Count(r => !string.IsNullOrEmpty(r.OwnerResponse));
            var averageRating = list.Average(r => (double)r.Rating);
            var responseRate = totalReviews > 0
                ? Math.Round((double)totalResponded / totalReviews * 100, 1)
                : 0;

            var distribution = new ReviewRatingDistributionDto
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

        // ═══════════════════════════════════════════════════════════
        // CUSTOMER PRODUCT REVIEW ENDPOINTS (BRANCH 1)
        // ═══════════════════════════════════════════════════════════

        public async Task<Pagination<CustomerReviewDto>> GetProductReviewsAsync(int productId, CustomerReviewFilterDto filter)
        {
            var filterParams = new CustomerReviewFilterParams
            {
                Rating = filter.Rating,
                SortBy = filter.SortBy,
                PageIndex = filter.PageIndex,
                PageSize = filter.PageSize
            };

            var countSpec = new ProductReviewsSpecification(productId, filterParams, true);
            var totalCount = await _unitOfWork.Repository<ProductReview>()
                .GetCountWithSpecificationsAsync(countSpec);

            var spec = new ProductReviewsSpecification(productId, filterParams);
            var reviews = await _unitOfWork.Repository<ProductReview>()
                .GetAllWithSpecificationsAsync(spec);

            var dtos = _mapper.Map<List<CustomerReviewDto>>(reviews);

            return new Pagination<CustomerReviewDto>(filter.PageIndex, filter.PageSize, totalCount, dtos);
        }

        public async Task<ProductRatingDistributionDto> GetProductRatingDistributionAsync(int productId)
        {
            var spec = new ProductReviewsSpecification(productId, new CustomerReviewFilterParams { PageSize = 1000 });
            var reviews = await _unitOfWork.Repository<ProductReview>()
                .GetAllWithSpecificationsAsync(spec);

            var list = reviews.ToList();
            var totalReviews = list.Count;
            var averageRating = totalReviews > 0 ? (float)Math.Round(list.Average(r => (double)r.Rating), 1) : 0f;

            var starCounts = new Dictionary<int, int>
            {
                { 5, list.Count(r => r.Rating == 5) },
                { 4, list.Count(r => r.Rating == 4) },
                { 3, list.Count(r => r.Rating == 3) },
                { 2, list.Count(r => r.Rating == 2) },
                { 1, list.Count(r => r.Rating == 1) }
            };

            return new ProductRatingDistributionDto
            {
                TotalReviews = totalReviews,
                AverageRating = averageRating,
                StarCounts = starCounts
            };
        }

        public async Task<CustomerReviewDto> CreateReviewAsync(CreateReviewDto dto, List<IFormFile>? photos, string customerUserId)
        {
            // 1. Verify customer exists
            var customer = await _userManager.FindByIdAsync(customerUserId);
            if (customer == null)
                throw new NotFoundException("Customer not found");

            // 2. Check product exists and is Approved
            var productSpec = new ProductByIdPublicSpecification(dto.ProductId);
            var product = await _unitOfWork.Repository<Product>()
                .GetByIdWithSpecificationsAsync(productSpec);

            if (product == null)
                throw new NotFoundException("Product not found or not approved");

            // 2.5 Verify customer has purchased this product (Delivered order)
            var purchaseSpec = new Talentree.Core.Specifications.OrderSpecifications.CustomerOrderPurchasedSpecification(customerUserId, dto.ProductId);
            var purchasedOrdersCount = await _unitOfWork.Repository<CustomerOrder>()
                .GetCountWithSpecificationsAsync(purchaseSpec);

            if (purchasedOrdersCount == 0)
                throw new BadRequestException("You can only review products that you have purchased and have been successfully delivered.");

            // 3. Check customer has not already reviewed this product
            var existingReviewSpec = new ProductReviewsSpecification(dto.ProductId, new CustomerReviewFilterParams { PageSize = 1000 });
            var productReviews = await _unitOfWork.Repository<ProductReview>()
                .GetAllWithSpecificationsAsync(existingReviewSpec);

            var alreadyReviewed = productReviews.Any(r => r.CustomerUserId == customerUserId);
            if (alreadyReviewed)
                throw new BadRequestException("You have already reviewed this product");

            // 4. Validate and upload photos (up to 3 photos allowed)
            var reviewPhotos = new List<ReviewPhoto>();
            if (photos != null && photos.Any())
            {
                if (photos.Count > 3)
                    throw new BadRequestException("Maximum of 3 photos allowed for a review");

                for (int i = 0; i < photos.Count; i++)
                {
                    var file = photos[i];
                    if (!_imageService.IsValidImage(file))
                        throw new BadRequestException($"Image '{file.FileName}' is invalid. Max 5MB, JPEG/PNG only");

                    var imageUrl = await _imageService.UploadImageAsync(file, "reviews");
                    reviewPhotos.Add(new ReviewPhoto
                    {
                        ImageUrl = imageUrl,
                        SortOrder = i
                    });
                }
            }

            // 5. Create ProductReview
            var review = new ProductReview
            {
                ProductId = dto.ProductId,
                CustomerUserId = customerUserId,
                CustomerName = customer.DisplayName,
                ReviewTitle = dto.ReviewTitle,
                Rating = dto.Rating,
                ReviewText = dto.ReviewText,
                IsAnonymous = dto.IsAnonymous,
                HelpfulVotes = 0,
                Photos = reviewPhotos
            };

            _unitOfWork.Repository<ProductReview>().Add(review);
            await _unitOfWork.CompleteAsync();

            // 6. Recalculate Product.AvgRating
            var allReviewsSpec = new ProductReviewsSpecification(dto.ProductId, new CustomerReviewFilterParams { PageSize = 1000 });
            var allReviews = await _unitOfWork.Repository<ProductReview>()
                .GetAllWithSpecificationsAsync(allReviewsSpec);

            var list = allReviews.ToList();
            if (list.Any())
            {
                product.AvgRating = (float)Math.Round(list.Average(r => (double)r.Rating), 1);
            }
            else
            {
                product.AvgRating = dto.Rating;
            }

            _unitOfWork.Repository<Product>().Update(product);
            await _unitOfWork.CompleteAsync();

            // 7. Send notification to Business Owner via INotificationService
            if (product.BusinessOwner != null)
            {
                await _notificationService.CreateNotificationAsync(new DTOs.Notification.CreateNotificationDto
                {
                    UserId = product.BusinessOwner.UserId,
                    Title = "New Product Review",
                    Message = $"A customer has reviewed your product '{product.Name}' with a rating of {dto.Rating}/5 stars.",
                    Type = NotificationType.System,
                    Priority = NotificationPriority.Normal
                });
            }

            // Return created review mapped to DTO
            var spec = new ProductReviewsSpecification(dto.ProductId, new CustomerReviewFilterParams { PageSize = 1000 });
            var refreshedReviews = await _unitOfWork.Repository<ProductReview>()
                .GetAllWithSpecificationsAsync(spec);
            var savedReview = refreshedReviews.First(r => r.Id == review.Id);

            return _mapper.Map<CustomerReviewDto>(savedReview);
        }

        public async Task MarkReviewHelpfulAsync(int reviewId, string customerUserId)
        {
            var spec = new ReviewByIdSpecification(reviewId);
            var review = await _unitOfWork.Repository<ProductReview>()
                .GetByIdWithSpecificationsAsync(spec);

            if (review == null)
                throw new NotFoundException("Review not found");

            if (review.CustomerUserId == customerUserId)
                throw new BadRequestException("You cannot vote your own review as helpful");

            review.HelpfulVotes++;
            _unitOfWork.Repository<ProductReview>().Update(review);
            await _unitOfWork.CompleteAsync();
        }
    }
}