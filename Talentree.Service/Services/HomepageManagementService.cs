using AutoMapper;
using Talentree.Core;
using Talentree.Core.Entities;
using Talentree.Core.Entities.Identity;
using Talentree.Core.Specifications.CategorySpecifications;
using Talentree.Service.Contracts;
using Talentree.Service.DTOs.PlatformSettings;

namespace Talentree.Service.Services
{
    /// <summary>
    /// FR-AD-35: Admin management of homepage featured content.
    /// Banners, featured brands, featured products, and announcement bar.
    /// </summary>
    public class HomepageManagementService : IHomepageManagementService
    {
        private const int MaxFeaturedBrands = 10;
        private const int MaxFeaturedProducts = 20;

        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public HomepageManagementService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // ── Preview ─────────────────────────────────────────────────

        /// <inheritdoc/>
        public async Task<HomepagePreviewDto> GetHomepagePreviewAsync()
        {
            var now = DateTime.UtcNow;

            var banners = await GetActiveBannersAsync(now);
            var brands = await GetActiveFeaturedBrandsAsync(now);
            var products = await GetActiveFeaturedProductsAsync(now);
            var announcement = await GetActiveAnnouncementBarInternalAsync(now);

            return new HomepagePreviewDto
            {
                HeroBanners = _mapper.Map<List<BannerDto>>(banners.Where(b => b.IsHero).ToList()),
                PromotionalBanners = _mapper.Map<List<BannerDto>>(banners.Where(b => !b.IsHero).ToList()),
                FeaturedBrands = _mapper.Map<List<FeaturedBrandDto>>(brands),
                FeaturedProducts = _mapper.Map<List<FeaturedProductDto>>(products),
                ActiveAnnouncementBar = announcement != null
                    ? _mapper.Map<AnnouncementBarDto>(announcement)
                    : null
            };
        }

        // ── Banners ─────────────────────────────────────────────────

        /// <inheritdoc/>
        public async Task<List<BannerDto>> GetAllBannersAsync()
        {
            var banners = await _unitOfWork.Repository<HomepageBanner>()
                .GetAllAsync();
            return _mapper.Map<List<BannerDto>>(banners.OrderBy(b => b.DisplayOrder).ToList());
        }

        /// <inheritdoc/>
        public async Task<BannerDto> GetBannerByIdAsync(int id)
        {
            var banner = await LoadBannerAsync(id);
            return _mapper.Map<BannerDto>(banner);
        }

        /// <inheritdoc/>
        public async Task<BannerDto> CreateBannerAsync(CreateBannerDto dto, string adminId)
        {
            var banner = _mapper.Map<HomepageBanner>(dto);
            banner.CreatedBy = adminId;
            banner.CreatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<HomepageBanner>().Add(banner);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<BannerDto>(banner);
        }

        /// <inheritdoc/>
        public async Task<BannerDto> UpdateBannerAsync(int id, UpdateBannerDto dto, string adminId)
        {
            var banner = await LoadBannerAsync(id);

            banner.Title = dto.Title;
            banner.Subtitle = dto.Subtitle;
            if (!string.IsNullOrWhiteSpace(dto.ImageUrl))
                banner.ImageUrl = dto.ImageUrl;
            banner.LinkUrl = dto.LinkUrl;
            banner.TextOverlay = dto.TextOverlay;
            banner.DisplayOrder = dto.DisplayOrder;
            banner.IsHero = dto.IsHero;
            banner.IsActive = dto.IsActive;
            banner.ScheduleStart = dto.ScheduleStart;
            banner.ScheduleEnd = dto.ScheduleEnd;
            banner.UpdatedAt = DateTime.UtcNow;
            banner.UpdatedBy = adminId;

            _unitOfWork.Repository<HomepageBanner>().Update(banner);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<BannerDto>(banner);
        }

        /// <inheritdoc/>
        public async Task DeleteBannerAsync(int id, string adminId)
        {
            var banner = await LoadBannerAsync(id);
            _unitOfWork.Repository<HomepageBanner>().Delete(banner);
            await _unitOfWork.CompleteAsync();
        }

        /// <inheritdoc/>
        public async Task ReorderBannersAsync(ReorderBannersDto dto, string adminId)
        {
            var ids = dto.Items.Select(i => i.Id).ToList();
            var banners = (await _unitOfWork.Repository<HomepageBanner>()
                .FindAsync(b => ids.Contains(b.Id)))
                .ToList();

            foreach (var item in dto.Items)
            {
                var banner = banners.FirstOrDefault(b => b.Id == item.Id);
                if (banner == null) continue;
                banner.DisplayOrder = item.NewDisplayOrder;
                banner.UpdatedAt = DateTime.UtcNow;
                banner.UpdatedBy = adminId;
                _unitOfWork.Repository<HomepageBanner>().Update(banner);
            }

            await _unitOfWork.CompleteAsync();
        }

        // ── Featured Brands ──────────────────────────────────────────

        /// <inheritdoc/>
        public async Task<List<FeaturedBrandDto>> GetFeaturedBrandsAsync()
        {
            var brands = await _unitOfWork.Repository<HomepageFeaturedBrand>()
                .GetAllWithSpecificationsAsync(new Core.Specifications.PlatformSettingsSpecifications.FeaturedBrandsWithProfileSpecification(onlyActive: true));
            return _mapper.Map<List<FeaturedBrandDto>>(brands.OrderBy(b => b.DisplayOrder).ToList());
        }

        /// <inheritdoc/>
        public async Task<List<FeaturedBrandDto>> SetFeaturedBrandsAsync(SetFeaturedBrandsDto dto, string adminId)
        {
            if (dto.Brands.Count > MaxFeaturedBrands)
                throw new InvalidOperationException($"A maximum of {MaxFeaturedBrands} featured brands is allowed.");

            var existing = (await _unitOfWork.Repository<HomepageFeaturedBrand>()
                .GetAllAsync()).ToList();

            foreach (var brand in existing)
                _unitOfWork.Repository<HomepageFeaturedBrand>().Delete(brand);

            // Insert new set
            foreach (var item in dto.Brands)
            {
                _unitOfWork.Repository<HomepageFeaturedBrand>().Add(new HomepageFeaturedBrand
                {
                    BusinessOwnerId = item.BusinessOwnerId,
                    DisplayOrder = item.DisplayOrder,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = adminId
                });
            }

            await _unitOfWork.CompleteAsync();
            return await GetFeaturedBrandsAsync();
        }

        // ── Featured Products ────────────────────────────────────────

        /// <inheritdoc/>
        public async Task<List<FeaturedProductDto>> GetFeaturedProductsAsync()
        {
            var products = await _unitOfWork.Repository<HomepageFeaturedProduct>()
                .GetAllWithSpecificationsAsync(new Core.Specifications.PlatformSettingsSpecifications.FeaturedProductsWithProductSpecification(onlyActive: true));
            return _mapper.Map<List<FeaturedProductDto>>(products.OrderBy(p => p.DisplayOrder).ToList());
        }

        /// <inheritdoc/>
        public async Task<List<FeaturedProductDto>> SetFeaturedProductsAsync(SetFeaturedProductsDto dto, string adminId)
        {
            if (dto.Products.Count > MaxFeaturedProducts)
                throw new InvalidOperationException($"A maximum of {MaxFeaturedProducts} featured products is allowed.");

            var existing = (await _unitOfWork.Repository<HomepageFeaturedProduct>()
                .GetAllAsync()).ToList();

            foreach (var product in existing)
                _unitOfWork.Repository<HomepageFeaturedProduct>().Delete(product);

            // Insert new set
            foreach (var item in dto.Products)
            {
                _unitOfWork.Repository<HomepageFeaturedProduct>().Add(new HomepageFeaturedProduct
                {
                    ProductId = item.ProductId,
                    DisplayOrder = item.DisplayOrder,
                    IsActive = true,
                    ScheduleStart = item.ScheduleStart,
                    ScheduleEnd = item.ScheduleEnd,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = adminId
                });
            }

            await _unitOfWork.CompleteAsync();
            return await GetFeaturedProductsAsync();
        }

        // ── Announcement Bar ─────────────────────────────────────────

        /// <inheritdoc/>
        public async Task<AnnouncementBarDto?> GetActiveAnnouncementBarAsync()
        {
            var bar = await GetActiveAnnouncementBarInternalAsync(DateTime.UtcNow);
            return bar != null ? _mapper.Map<AnnouncementBarDto>(bar) : null;
        }

        /// <inheritdoc/>
        public async Task<AnnouncementBarDto> UpdateAnnouncementBarAsync(UpdateAnnouncementBarDto dto, string adminId)
        {
            var all = (await _unitOfWork.Repository<AnnouncementBar>()
                .GetAllAsync()).ToList();

            AnnouncementBar bar;
            if (all.Count > 0)
            {
                bar = all.First();
                bar.Message = dto.Message;
                bar.LinkUrl = dto.LinkUrl;
                bar.IsActive = dto.IsActive;
                bar.ScheduleStart = dto.ScheduleStart;
                bar.ScheduleEnd = dto.ScheduleEnd;
                bar.UpdatedAt = DateTime.UtcNow;
                bar.UpdatedBy = adminId;
                _unitOfWork.Repository<AnnouncementBar>().Update(bar);
            }
            else
            {
                bar = new AnnouncementBar
                {
                    Message = dto.Message,
                    LinkUrl = dto.LinkUrl,
                    IsActive = dto.IsActive,
                    ScheduleStart = dto.ScheduleStart,
                    ScheduleEnd = dto.ScheduleEnd,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = adminId
                };
                _unitOfWork.Repository<AnnouncementBar>().Add(bar);
            }

            await _unitOfWork.CompleteAsync();
            return _mapper.Map<AnnouncementBarDto>(bar);
        }

        // ── Private helpers ──────────────────────────────────────────

        private async Task<HomepageBanner> LoadBannerAsync(int id)
        {
            return await _unitOfWork.Repository<HomepageBanner>().GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Banner #{id} not found.");
        }

        private async Task<List<HomepageBanner>> GetActiveBannersAsync(DateTime now)
        {
            var all = await _unitOfWork.Repository<HomepageBanner>()
                .GetAllAsync();

            return all.Where(b =>
                b.IsActive &&
                (b.ScheduleStart == null || b.ScheduleStart <= now) &&
                (b.ScheduleEnd == null || b.ScheduleEnd > now))
                .OrderBy(b => b.DisplayOrder)
                .ToList();
        }

        private async Task<List<HomepageFeaturedBrand>> GetActiveFeaturedBrandsAsync(DateTime now)
        {
            var all = await _unitOfWork.Repository<HomepageFeaturedBrand>()
                .GetAllWithSpecificationsAsync(new Core.Specifications.PlatformSettingsSpecifications.FeaturedBrandsWithProfileSpecification());
            return all.Where(b => b.IsActive).OrderBy(b => b.DisplayOrder).ToList();
        }

        private async Task<List<HomepageFeaturedProduct>> GetActiveFeaturedProductsAsync(DateTime now)
        {
            var all = await _unitOfWork.Repository<HomepageFeaturedProduct>()
                .GetAllWithSpecificationsAsync(new Core.Specifications.PlatformSettingsSpecifications.FeaturedProductsWithProductSpecification());

            return all.Where(p =>
                p.IsActive &&
                (p.ScheduleStart == null || p.ScheduleStart <= now) &&
                (p.ScheduleEnd == null || p.ScheduleEnd > now))
                .OrderBy(p => p.DisplayOrder)
                .ToList();
        }

        private async Task<AnnouncementBar?> GetActiveAnnouncementBarInternalAsync(DateTime now)
        {
            var all = await _unitOfWork.Repository<AnnouncementBar>()
                .GetAllAsync();

            return all.FirstOrDefault(a =>
                a.IsActive &&
                (a.ScheduleStart == null || a.ScheduleStart <= now) &&
                (a.ScheduleEnd == null || a.ScheduleEnd > now));
        }
    }
}
