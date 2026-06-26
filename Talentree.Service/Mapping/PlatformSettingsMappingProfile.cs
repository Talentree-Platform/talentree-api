using AutoMapper;
using Talentree.Core.Entities;
using Talentree.Core.Entities.Identity;
using Talentree.Core.Enums;
using Talentree.Service.DTOs.PlatformSettings;

namespace Talentree.Service.Mapping
{
    /// <summary>
    /// AutoMapper profile for Platform Settings entities (FR-AD-31 to FR-AD-36).
    /// Kept separate from MappingProfile to manage file size.
    /// </summary>
    public class PlatformSettingsMappingProfile : Profile
    {
        public PlatformSettingsMappingProfile()
        {
            // ═══════════════════════════════════════════════════════════
            // FR-AD-31: Category Management
            // ═══════════════════════════════════════════════════════════

            CreateMap<Category, PlatformCategoryDto>()
                .ForMember(d => d.SubCategories,
                    o => o.MapFrom(s => s.SubCategories));

            CreateMap<Category, PlatformCategorySummaryDto>()
                .ForMember(d => d.SubCategoryCount,
                    o => o.MapFrom(s => s.SubCategories != null ? s.SubCategories.Count : 0));

            // ═══════════════════════════════════════════════════════════
            // FR-AD-32: Commission Settings
            // ═══════════════════════════════════════════════════════════

            CreateMap<CommissionSetting, CommissionSettingDto>()
                .ForMember(d => d.UpdatedAt,
                    o => o.MapFrom(s => s.UpdatedAt ?? s.CreatedAt))
                .ForMember(d => d.UpdatedBy,
                    o => o.MapFrom(s => s.UpdatedBy));

            // ═══════════════════════════════════════════════════════════
            // FR-AD-33: Shipping Settings
            // ═══════════════════════════════════════════════════════════

            CreateMap<ShippingSettings, ShippingSettingsDto>()
                .ForMember(d => d.UpdatedAt,
                    o => o.MapFrom(s => s.UpdatedAt ?? s.CreatedAt))
                .ForMember(d => d.UpdatedBy,
                    o => o.MapFrom(s => s.UpdatedBy));

            // ═══════════════════════════════════════════════════════════
            // FR-AD-35: Homepage Management
            // ═══════════════════════════════════════════════════════════

            CreateMap<HomepageBanner, BannerDto>();
            CreateMap<CreateBannerDto, HomepageBanner>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.CreatedAt, o => o.Ignore())
                .ForMember(d => d.UpdatedAt, o => o.Ignore())
                .ForMember(d => d.CreatedBy, o => o.Ignore())
                .ForMember(d => d.UpdatedBy, o => o.Ignore());

            CreateMap<HomepageFeaturedBrand, FeaturedBrandDto>()
                .ForMember(d => d.BusinessOwnerName,
                    o => o.MapFrom(s => s.BusinessOwner != null ? s.BusinessOwner.DisplayName : string.Empty))
                .ForMember(d => d.BusinessOwnerLogoUrl,
                    o => o.MapFrom(s =>
                        s.BusinessOwner != null && s.BusinessOwner.BusinessOwnerProfile != null
                            ? s.BusinessOwner.BusinessOwnerProfile.BusinessLogoUrl
                            : null));

            CreateMap<HomepageFeaturedProduct, FeaturedProductDto>()
                .ForMember(d => d.ProductName,
                    o => o.MapFrom(s => s.Product != null ? s.Product.Name : string.Empty))
                .ForMember(d => d.ProductImageUrl,
                    o => o.MapFrom(s =>
                        s.Product != null && s.Product.Images != null && s.Product.Images.Any()
                            ? s.Product.Images.OrderBy(i => i.SortOrder).First().ImageUrl
                            : null));

            CreateMap<AnnouncementBar, AnnouncementBarDto>();

            // ═══════════════════════════════════════════════════════════
            // FR-AD-36: Terms & Policies
            // ═══════════════════════════════════════════════════════════

            CreateMap<PlatformPolicy, PolicyDocumentDto>()
                .ForMember(d => d.DocumentTypeName,
                    o => o.MapFrom(s => s.DocumentType.ToString()));

            CreateMap<PlatformPolicy, PolicyDocumentSummaryDto>()
                .ForMember(d => d.DocumentTypeName,
                    o => o.MapFrom(s => s.DocumentType.ToString()));

            CreateMap<PlatformPolicy, PolicyVersionHistoryDto>();
        }
    }
}
