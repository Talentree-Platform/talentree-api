using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Talentree.API.Models;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs.PlatformSettings;

namespace Talentree.API.Controllers
{
    /// <summary>
    /// FR-AD-35: Admin management of homepage featured content.
    /// Manages hero banners, promotional banners, featured brands, featured products, and announcement bar.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [Route("api/admin/platform/homepage")]
    [ApiController]
    public class HomepageManagementController : BaseApiController
    {
        private readonly IHomepageManagementService _homepageService;

        public HomepageManagementController(IHomepageManagementService homepageService)
        {
            _homepageService = homepageService;
        }

        // ═══════════════════════════════════════════════════════════
        // GET: api/admin/platform/homepage/preview
        // Aggregate of all active homepage elements (used for admin preview and live homepage)
        // ═══════════════════════════════════════════════════════════
        [HttpGet("preview")]
        [ProducesResponseType(typeof(ApiResponse<HomepagePreviewDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<HomepagePreviewDto>>> GetHomepagePreview()
        {
            var result = await _homepageService.GetHomepagePreviewAsync();
            return Ok(ApiResponse<HomepagePreviewDto>.SuccessResponse(
                data: result,
                message: "Homepage preview retrieved successfully"));
        }

        // ═══════════════════════════════════════════════════════════
        // BANNER ENDPOINTS
        // ═══════════════════════════════════════════════════════════

        // GET: api/admin/platform/homepage/banners
        [HttpGet("banners")]
        [ProducesResponseType(typeof(ApiResponse<List<BannerDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<BannerDto>>>> GetAllBanners()
        {
            var result = await _homepageService.GetAllBannersAsync();
            return Ok(ApiResponse<List<BannerDto>>.SuccessResponse(
                data: result,
                message: $"Retrieved {result.Count} banners"));
        }

        // GET: api/admin/platform/homepage/banners/{id}
        [HttpGet("banners/{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<BannerDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<BannerDto>>> GetBanner(int id)
        {
            var result = await _homepageService.GetBannerByIdAsync(id);
            return Ok(ApiResponse<BannerDto>.SuccessResponse(
                data: result,
                message: "Banner retrieved successfully"));
        }

        // POST: api/admin/platform/homepage/banners
        [HttpPost("banners")]
        [ProducesResponseType(typeof(ApiResponse<BannerDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<BannerDto>>> CreateBanner(
            [FromBody] CreateBannerDto dto)
        {
            var result = await _homepageService.CreateBannerAsync(dto, GetCurrentUserId());
            return CreatedAtAction(nameof(GetBanner), new { id = result.Id },
                ApiResponse<BannerDto>.SuccessResponse(
                    data: result,
                    message: "Banner created successfully"));
        }

        // PUT: api/admin/platform/homepage/banners/{id}
        [HttpPut("banners/{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<BannerDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<BannerDto>>> UpdateBanner(
            int id, [FromBody] UpdateBannerDto dto)
        {
            var result = await _homepageService.UpdateBannerAsync(id, dto, GetCurrentUserId());
            return Ok(ApiResponse<BannerDto>.SuccessResponse(
                data: result,
                message: "Banner updated successfully"));
        }

        // DELETE: api/admin/platform/homepage/banners/{id}
        [HttpDelete("banners/{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> DeleteBanner(int id)
        {
            await _homepageService.DeleteBannerAsync(id, GetCurrentUserId());
            return Ok(ApiResponse<object>.SuccessResponse(
                message: "Banner deleted successfully"));
        }

        // PUT: api/admin/platform/homepage/banners/reorder
        [HttpPut("banners/reorder")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<object>>> ReorderBanners(
            [FromBody] ReorderBannersDto dto)
        {
            await _homepageService.ReorderBannersAsync(dto, GetCurrentUserId());
            return Ok(ApiResponse<object>.SuccessResponse(
                message: "Banners reordered successfully"));
        }

        // ═══════════════════════════════════════════════════════════
        // FEATURED BRANDS ENDPOINTS
        // ═══════════════════════════════════════════════════════════

        // GET: api/admin/platform/homepage/featured-brands
        [HttpGet("featured-brands")]
        [ProducesResponseType(typeof(ApiResponse<List<FeaturedBrandDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<FeaturedBrandDto>>>> GetFeaturedBrands()
        {
            var result = await _homepageService.GetFeaturedBrandsAsync();
            return Ok(ApiResponse<List<FeaturedBrandDto>>.SuccessResponse(
                data: result,
                message: $"Retrieved {result.Count} featured brands"));
        }

        // PUT: api/admin/platform/homepage/featured-brands
        // Replaces the entire featured brands list (max 10)
        [HttpPut("featured-brands")]
        [ProducesResponseType(typeof(ApiResponse<List<FeaturedBrandDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<List<FeaturedBrandDto>>>> SetFeaturedBrands(
            [FromBody] SetFeaturedBrandsDto dto)
        {
            var result = await _homepageService.SetFeaturedBrandsAsync(dto, GetCurrentUserId());
            return Ok(ApiResponse<List<FeaturedBrandDto>>.SuccessResponse(
                data: result,
                message: "Featured brands updated successfully"));
        }

        // ═══════════════════════════════════════════════════════════
        // FEATURED PRODUCTS ENDPOINTS
        // ═══════════════════════════════════════════════════════════

        // GET: api/admin/platform/homepage/featured-products
        [HttpGet("featured-products")]
        [ProducesResponseType(typeof(ApiResponse<List<FeaturedProductDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<FeaturedProductDto>>>> GetFeaturedProducts()
        {
            var result = await _homepageService.GetFeaturedProductsAsync();
            return Ok(ApiResponse<List<FeaturedProductDto>>.SuccessResponse(
                data: result,
                message: $"Retrieved {result.Count} featured products"));
        }

        // PUT: api/admin/platform/homepage/featured-products
        // Replaces the entire featured products list (max 20)
        [HttpPut("featured-products")]
        [ProducesResponseType(typeof(ApiResponse<List<FeaturedProductDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<List<FeaturedProductDto>>>> SetFeaturedProducts(
            [FromBody] SetFeaturedProductsDto dto)
        {
            var result = await _homepageService.SetFeaturedProductsAsync(dto, GetCurrentUserId());
            return Ok(ApiResponse<List<FeaturedProductDto>>.SuccessResponse(
                data: result,
                message: "Featured products updated successfully"));
        }

        // ═══════════════════════════════════════════════════════════
        // ANNOUNCEMENT BAR ENDPOINTS
        // ═══════════════════════════════════════════════════════════

        // GET: api/admin/platform/homepage/announcement
        [HttpGet("announcement")]
        [ProducesResponseType(typeof(ApiResponse<AnnouncementBarDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<AnnouncementBarDto?>>> GetAnnouncementBar()
        {
            var result = await _homepageService.GetActiveAnnouncementBarAsync();
            return Ok(ApiResponse<AnnouncementBarDto?>.SuccessResponse(
                data: result,
                message: result != null ? "Announcement bar retrieved" : "No active announcement bar"));
        }

        // PUT: api/admin/platform/homepage/announcement
        [HttpPut("announcement")]
        [ProducesResponseType(typeof(ApiResponse<AnnouncementBarDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<AnnouncementBarDto>>> UpdateAnnouncementBar(
            [FromBody] UpdateAnnouncementBarDto dto)
        {
            var result = await _homepageService.UpdateAnnouncementBarAsync(dto, GetCurrentUserId());
            return Ok(ApiResponse<AnnouncementBarDto>.SuccessResponse(
                data: result,
                message: "Announcement bar updated successfully"));
        }
    }
}
