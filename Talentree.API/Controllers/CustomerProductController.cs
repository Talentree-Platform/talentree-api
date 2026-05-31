using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Talentree.API.Models;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs.Common;
using Talentree.Service.DTOs.Customer;

namespace Talentree.API.Controllers
{
    [Route("api/customer")]
    public class CustomerProductController : BaseApiController
    {
        private readonly IProductService _productService;
        private readonly IReviewService _reviewService;

        public CustomerProductController(IProductService productService, IReviewService reviewService)
        {
            _productService = productService;
            _reviewService = reviewService;
        }

        // GET /api/customer/homepage (Public)
        [HttpGet("homepage")]
        [ProducesResponseType(typeof(ApiResponse<HomepageDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<HomepageDto>>> GetHomepageData()
        {
            var data = await _productService.GetHomepageDataAsync();
            return Ok(ApiResponse<HomepageDto>.SuccessResponse(data, "Homepage data loaded successfully"));
        }

        // GET /api/customer/products (Public)
        [HttpGet("products")]
        [ProducesResponseType(typeof(ApiResponse<Pagination<CustomerProductDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<Pagination<CustomerProductDto>>>> SearchProducts([FromQuery] CustomerProductFilterDto filter)
        {
            var pagination = await _productService.SearchProductsAsync(filter);
            return Ok(ApiResponse<Pagination<CustomerProductDto>>.SuccessResponse(pagination, "Products search/filter complete"));
        }

        // GET /api/customer/products/autocomplete (Public)
        [HttpGet("products/autocomplete")]
        [ProducesResponseType(typeof(ApiResponse<List<string>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<string>>>> GetAutocomplete([FromQuery] string q)
        {
            var suggestions = await _productService.GetAutocompleteAsync(q);
            return Ok(ApiResponse<List<string>>.SuccessResponse(suggestions, "Autocomplete suggestions loaded"));
        }

        // GET /api/customer/products/{id} (Public)
        [HttpGet("products/{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<CustomerProductDetailDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<CustomerProductDetailDto>>> GetProduct(int id)
        {
            var product = await _productService.GetPublicProductByIdAsync(id);
            return Ok(ApiResponse<CustomerProductDetailDto>.SuccessResponse(product, "Product details loaded"));
        }

        // GET /api/customer/categories (Public)
        [HttpGet("categories")]
        [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<CategoryDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<CategoryDto>>>> GetCategories()
        {
            var categories = await _productService.GetCategoriesAsync();
            return Ok(ApiResponse<IReadOnlyList<CategoryDto>>.SuccessResponse(categories, "Categories loaded"));
        }

        // GET /api/customer/categories/{id}/products (Public)
        [HttpGet("categories/{id:int}/products")]
        [ProducesResponseType(typeof(ApiResponse<Pagination<CustomerProductDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<Pagination<CustomerProductDto>>>> GetCategoryProducts(int id, [FromQuery] CustomerProductFilterDto filter)
        {
            var pagination = await _productService.GetByCategoryAsync(id, filter);
            return Ok(ApiResponse<Pagination<CustomerProductDto>>.SuccessResponse(pagination, "Category products loaded"));
        }

        // GET /api/customer/brands (Public)
        [HttpGet("brands")]
        [ProducesResponseType(typeof(ApiResponse<Pagination<BrandListDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<Pagination<BrandListDto>>>> GetBrands([FromQuery] BrandFilterDto filter)
        {
            var pagination = await _productService.GetBrandsAsync(filter);
            return Ok(ApiResponse<Pagination<BrandListDto>>.SuccessResponse(pagination, "Brands search/filter complete"));
        }

        // GET /api/customer/brands/{id} (Public)
        [HttpGet("brands/{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<BrandDetailDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<BrandDetailDto>>> GetBrand(int id)
        {
            var brand = await _productService.GetBrandByIdAsync(id);
            return Ok(ApiResponse<BrandDetailDto>.SuccessResponse(brand, "Brand details loaded"));
        }

        // GET /api/customer/brands/{id}/products (Public)
        [HttpGet("brands/{id:int}/products")]
        [ProducesResponseType(typeof(ApiResponse<Pagination<CustomerProductDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<Pagination<CustomerProductDto>>>> GetBrandProducts(int id, [FromQuery] CustomerProductFilterDto filter)
        {
            var pagination = await _productService.GetBrandProductsAsync(id, filter);
            return Ok(ApiResponse<Pagination<CustomerProductDto>>.SuccessResponse(pagination, "Brand products loaded"));
        }

        // GET /api/customer/products/{id}/reviews (Public)
        [HttpGet("products/{id:int}/reviews")]
        [ProducesResponseType(typeof(ApiResponse<Pagination<CustomerReviewDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<Pagination<CustomerReviewDto>>>> GetProductReviews(int id, [FromQuery] CustomerReviewFilterDto filter)
        {
            var pagination = await _reviewService.GetProductReviewsAsync(id, filter);
            return Ok(ApiResponse<Pagination<CustomerReviewDto>>.SuccessResponse(pagination, "Product reviews loaded"));
        }

        // GET /api/customer/products/{id}/reviews/distribution (Public)
        [HttpGet("products/{id:int}/reviews/distribution")]
        [ProducesResponseType(typeof(ApiResponse<ProductRatingDistributionDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<ProductRatingDistributionDto>>> GetProductReviewsDistribution(int id)
        {
            var distribution = await _reviewService.GetProductRatingDistributionAsync(id);
            return Ok(ApiResponse<ProductRatingDistributionDto>.SuccessResponse(distribution, "Rating distribution loaded"));
        }

        // POST /api/customer/products/{id}/reviews (Authenticated)
        [Authorize(Roles = "Customer")]
        [HttpPost("products/{id:int}/reviews")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<CustomerReviewDto>), StatusCodes.Status201Created)]
        public async Task<ActionResult<ApiResponse<CustomerReviewDto>>> CreateReview(int id, [FromForm] CreateReviewDto dto, [FromForm] List<IFormFile>? photos)
        {
            dto.ProductId = id; // Ensure product ID matches route parameter
            var customerUserId = GetCurrentUserId();
            var review = await _reviewService.CreateReviewAsync(dto, photos, customerUserId);
            
            return CreatedAtAction(
                nameof(GetProductReviews), 
                new { id = dto.ProductId }, 
                ApiResponse<CustomerReviewDto>.SuccessResponse(review, "Review submitted successfully")
            );
        }

        // POST /api/customer/reviews/{reviewId}/helpful (Authenticated)
        [Authorize(Roles = "Customer")]
        [HttpPost("reviews/{reviewId:int}/helpful")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status204NoContent)]
        public async Task<ActionResult<ApiResponse<object>>> MarkReviewHelpful(int reviewId)
        {
            var customerUserId = GetCurrentUserId();
            await _reviewService.MarkReviewHelpfulAsync(reviewId, customerUserId);
            return Ok(ApiResponse<object>.SuccessResponse("Review voted as helpful"));
        }
    }
}
