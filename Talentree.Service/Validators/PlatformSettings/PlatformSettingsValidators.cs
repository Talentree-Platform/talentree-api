using FluentValidation;
using Talentree.Service.DTOs.PlatformSettings;

namespace Talentree.Service.Validators.PlatformSettings
{
    // ═══════════════════════════════════════════════════════════
    // FR-AD-31: Category Management Validators
    // ═══════════════════════════════════════════════════════════

    public class UpdateCategoryDtoValidator : AbstractValidator<UpdateCategoryDto>
    {
        public UpdateCategoryDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Category name is required.")
                .MaximumLength(100).WithMessage("Category name must not exceed 100 characters.");

            RuleFor(x => x.IconUrl)
                .MaximumLength(2048).WithMessage("Icon URL must not exceed 2048 characters.")
                .When(x => x.IconUrl != null);

            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Display order must be a non-negative number.");
        }
    }

    public class CreateSubCategoryDtoValidator : AbstractValidator<CreateSubCategoryDto>
    {
        public CreateSubCategoryDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Subcategory name is required.")
                .MaximumLength(100).WithMessage("Subcategory name must not exceed 100 characters.");

            RuleFor(x => x.ParentCategoryId)
                .GreaterThan(0).WithMessage("A valid parent category ID is required.");

            RuleFor(x => x.IconUrl)
                .MaximumLength(2048).WithMessage("Icon URL must not exceed 2048 characters.")
                .When(x => x.IconUrl != null);
        }
    }

    public class ReorderCategoriesDtoValidator : AbstractValidator<ReorderCategoriesDto>
    {
        public ReorderCategoriesDtoValidator()
        {
            RuleFor(x => x.Items)
                .NotEmpty().WithMessage("At least one category must be provided for reordering.");

            RuleForEach(x => x.Items).ChildRules(item =>
            {
                item.RuleFor(i => i.Id).GreaterThan(0).WithMessage("Category ID must be valid.");
                item.RuleFor(i => i.NewDisplayOrder).GreaterThanOrEqualTo(0).WithMessage("Display order must be non-negative.");
            });
        }
    }

    // ═══════════════════════════════════════════════════════════
    // FR-AD-32: Commission Validators
    // ═══════════════════════════════════════════════════════════

    public class UpdateCommissionSettingDtoValidator : AbstractValidator<UpdateCommissionSettingDto>
    {
        public UpdateCommissionSettingDtoValidator()
        {
            RuleFor(x => x.PlatformCommissionPercent)
                .InclusiveBetween(0, 100).WithMessage("Platform commission must be between 0 and 100.");

            RuleFor(x => x.TransactionFeeValue)
                .GreaterThanOrEqualTo(0).WithMessage("Transaction fee must be non-negative.");

            RuleFor(x => x.TransactionFeeValue)
                .InclusiveBetween(0, 100).When(x => x.IsTransactionFeePercent)
                .WithMessage("Transaction fee percentage must be between 0 and 100.");

            RuleFor(x => x.MinimumPayoutAmount)
                .GreaterThanOrEqualTo(0).WithMessage("Minimum payout amount must be non-negative.");

            RuleFor(x => x.PayoutProcessingFee)
                .GreaterThanOrEqualTo(0).WithMessage("Payout processing fee must be non-negative.");
        }
    }

    // ═══════════════════════════════════════════════════════════
    // FR-AD-33: Shipping Validators
    // ═══════════════════════════════════════════════════════════

    public class UpdateShippingSettingsDtoValidator : AbstractValidator<UpdateShippingSettingsDto>
    {
        public UpdateShippingSettingsDtoValidator()
        {
            RuleFor(x => x.FlatRate)
                .GreaterThanOrEqualTo(0).WithMessage("Flat rate must be non-negative.");

            RuleFor(x => x.FreeShippingThreshold)
                .GreaterThanOrEqualTo(0).WithMessage("Free shipping threshold must be non-negative.");

            RuleFor(x => x.EstimatedDeliveryDomesticDays)
                .GreaterThan(0).WithMessage("Domestic delivery estimate must be at least 1 day.");

            RuleFor(x => x.EstimatedDeliveryInternationalDays)
                .GreaterThan(0).WithMessage("International delivery estimate must be at least 1 day.")
                .When(x => x.InternationalShippingEnabled);
        }
    }

    // ═══════════════════════════════════════════════════════════
    // FR-AD-34: Tax Validators
    // ═══════════════════════════════════════════════════════════

    public class UpdateTaxSettingsDtoValidator : AbstractValidator<UpdateTaxSettingsDto>
    {
        public UpdateTaxSettingsDtoValidator()
        {
            RuleFor(x => x.TaxRate)
                .InclusiveBetween(0, 100).WithMessage("Tax rate must be between 0 and 100.")
                .When(x => x.TaxEnabled);
        }
    }

    // ═══════════════════════════════════════════════════════════
    // FR-AD-35: Homepage Banner Validators
    // ═══════════════════════════════════════════════════════════

    public class CreateBannerDtoValidator : AbstractValidator<CreateBannerDto>
    {
        public CreateBannerDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Banner title is required.")
                .MaximumLength(200).WithMessage("Banner title must not exceed 200 characters.");

            RuleFor(x => x.ImageUrl)
                .NotEmpty().WithMessage("Banner image URL is required.")
                .MaximumLength(2048).WithMessage("Image URL must not exceed 2048 characters.");

            RuleFor(x => x.ScheduleEnd)
                .GreaterThan(x => x.ScheduleStart).WithMessage("Schedule end must be after schedule start.")
                .When(x => x.ScheduleStart.HasValue && x.ScheduleEnd.HasValue);

            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Display order must be non-negative.");
        }
    }

    public class UpdateBannerDtoValidator : AbstractValidator<UpdateBannerDto>
    {
        public UpdateBannerDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Banner title is required.")
                .MaximumLength(200).WithMessage("Banner title must not exceed 200 characters.");

            RuleFor(x => x.ScheduleEnd)
                .GreaterThan(x => x.ScheduleStart).WithMessage("Schedule end must be after schedule start.")
                .When(x => x.ScheduleStart.HasValue && x.ScheduleEnd.HasValue);
        }
    }

    public class SetFeaturedBrandsDtoValidator : AbstractValidator<SetFeaturedBrandsDto>
    {
        public SetFeaturedBrandsDtoValidator()
        {
            RuleFor(x => x.Brands)
                .Must(b => b.Count <= 10).WithMessage("A maximum of 10 featured brands is allowed.");

            RuleForEach(x => x.Brands).ChildRules(item =>
            {
                item.RuleFor(i => i.BusinessOwnerId)
                    .NotEmpty().WithMessage("Business owner ID is required.");
            });
        }
    }

    public class SetFeaturedProductsDtoValidator : AbstractValidator<SetFeaturedProductsDto>
    {
        public SetFeaturedProductsDtoValidator()
        {
            RuleFor(x => x.Products)
                .Must(p => p.Count <= 20).WithMessage("A maximum of 20 featured products is allowed.");

            RuleForEach(x => x.Products).ChildRules(item =>
            {
                item.RuleFor(i => i.ProductId)
                    .GreaterThan(0).WithMessage("A valid product ID is required.");

                item.RuleFor(i => i.ScheduleEnd)
                    .GreaterThan(i => i.ScheduleStart).WithMessage("Schedule end must be after schedule start.")
                    .When(i => i.ScheduleStart.HasValue && i.ScheduleEnd.HasValue);
            });
        }
    }

    // ═══════════════════════════════════════════════════════════
    // FR-AD-36: Policy Validators
    // ═══════════════════════════════════════════════════════════

    public class UpdatePolicyDocumentDtoValidator : AbstractValidator<UpdatePolicyDocumentDto>
    {
        public UpdatePolicyDocumentDtoValidator()
        {
            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("Policy document content is required.");
        }
    }
}
