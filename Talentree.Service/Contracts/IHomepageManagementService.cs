using Talentree.Service.DTOs.PlatformSettings;

namespace Talentree.Service.Contracts
{
    /// <summary>
    /// FR-AD-35: Admin management of homepage featured content.
    /// </summary>
    public interface IHomepageManagementService
    {
        // ── Preview ─────────────────────────────────────────────────

        /// <summary>
        /// Returns all currently active/scheduled homepage elements aggregated
        /// in a single response. Used for both the admin preview and the live homepage.
        /// </summary>
        Task<HomepagePreviewDto> GetHomepagePreviewAsync();

        // ── Banners ─────────────────────────────────────────────────

        Task<List<BannerDto>> GetAllBannersAsync();
        Task<BannerDto> GetBannerByIdAsync(int id);
        Task<BannerDto> CreateBannerAsync(CreateBannerDto dto, string adminId);
        Task<BannerDto> UpdateBannerAsync(int id, UpdateBannerDto dto, string adminId);
        Task DeleteBannerAsync(int id, string adminId);
        Task ReorderBannersAsync(ReorderBannersDto dto, string adminId);

        // ── Featured Brands ──────────────────────────────────────────

        Task<List<FeaturedBrandDto>> GetFeaturedBrandsAsync();

        /// <summary>
        /// Replaces the entire featured brands list (max 10).
        /// Any previously featured brand not in the new list is removed.
        /// </summary>
        Task<List<FeaturedBrandDto>> SetFeaturedBrandsAsync(SetFeaturedBrandsDto dto, string adminId);

        // ── Featured Products ────────────────────────────────────────

        Task<List<FeaturedProductDto>> GetFeaturedProductsAsync();

        /// <summary>
        /// Replaces the entire featured products list (max 20).
        /// Any previously featured product not in the new list is removed.
        /// </summary>
        Task<List<FeaturedProductDto>> SetFeaturedProductsAsync(SetFeaturedProductsDto dto, string adminId);

        // ── Announcement Bar ─────────────────────────────────────────

        Task<AnnouncementBarDto?> GetActiveAnnouncementBarAsync();
        Task<AnnouncementBarDto> UpdateAnnouncementBarAsync(UpdateAnnouncementBarDto dto, string adminId);
    }
}
