namespace Talentree.Service.DTOs.PlatformSettings
{
    // ═══════════════════════════════════════════════════════════
    // FR-AD-35: Homepage Management DTOs
    // ═══════════════════════════════════════════════════════════

    // ── Banners ─────────────────────────────────────────────────

    public class BannerDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Subtitle { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string? LinkUrl { get; set; }
        public string? TextOverlay { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsHero { get; set; }
        public bool IsActive { get; set; }
        public DateTime? ScheduleStart { get; set; }
        public DateTime? ScheduleEnd { get; set; }
    }

    public class CreateBannerDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Subtitle { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string? LinkUrl { get; set; }
        public string? TextOverlay { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsHero { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? ScheduleStart { get; set; }
        public DateTime? ScheduleEnd { get; set; }
    }

    public class UpdateBannerDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Subtitle { get; set; }
        public string? ImageUrl { get; set; }
        public string? LinkUrl { get; set; }
        public string? TextOverlay { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsHero { get; set; }
        public bool IsActive { get; set; }
        public DateTime? ScheduleStart { get; set; }
        public DateTime? ScheduleEnd { get; set; }
    }

    public class ReorderBannersDto
    {
        public List<BannerOrderItemDto> Items { get; set; } = new();
    }

    public class BannerOrderItemDto
    {
        public int Id { get; set; }
        public int NewDisplayOrder { get; set; }
    }

    // ── Featured Brands ──────────────────────────────────────────

    public class FeaturedBrandDto
    {
        public int Id { get; set; }
        public string BusinessOwnerId { get; set; } = string.Empty;
        public string BusinessOwnerName { get; set; } = string.Empty;
        public string? BusinessOwnerLogoUrl { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>Replace the entire featured brands list (up to 10 entries).</summary>
    public class SetFeaturedBrandsDto
    {
        public List<FeaturedBrandItemDto> Brands { get; set; } = new();
    }

    public class FeaturedBrandItemDto
    {
        public string BusinessOwnerId { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
    }

    // ── Featured Products ────────────────────────────────────────

    public class FeaturedProductDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductImageUrl { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime? ScheduleStart { get; set; }
        public DateTime? ScheduleEnd { get; set; }
    }

    /// <summary>Replace the entire featured products list (up to 20 entries).</summary>
    public class SetFeaturedProductsDto
    {
        public List<FeaturedProductItemDto> Products { get; set; } = new();
    }

    public class FeaturedProductItemDto
    {
        public int ProductId { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime? ScheduleStart { get; set; }
        public DateTime? ScheduleEnd { get; set; }
    }

    // ── Announcement Bar ─────────────────────────────────────────

    public class AnnouncementBarDto
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? LinkUrl { get; set; }
        public bool IsActive { get; set; }
        public DateTime? ScheduleStart { get; set; }
        public DateTime? ScheduleEnd { get; set; }
    }

    public class UpdateAnnouncementBarDto
    {
        public string Message { get; set; } = string.Empty;
        public string? LinkUrl { get; set; }
        public bool IsActive { get; set; }
        public DateTime? ScheduleStart { get; set; }
        public DateTime? ScheduleEnd { get; set; }
    }

    // ── Homepage Preview Aggregate ───────────────────────────────

    /// <summary>
    /// Aggregate read model for the homepage preview endpoint.
    /// Returns all active/scheduled elements in a single response.
    /// </summary>
    public class HomepagePreviewDto
    {
        public List<BannerDto> HeroBanners { get; set; } = new();
        public List<BannerDto> PromotionalBanners { get; set; } = new();
        public List<FeaturedBrandDto> FeaturedBrands { get; set; } = new();
        public List<FeaturedProductDto> FeaturedProducts { get; set; } = new();
        public AnnouncementBarDto? ActiveAnnouncementBar { get; set; }
    }
}
