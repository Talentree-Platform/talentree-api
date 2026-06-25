using Talentree.Service.DTOs.Admin.Product;
using Talentree.Service.DTOs.Common;

namespace Talentree.Service.Contracts
{
    /// <summary>
    /// Admin service for product approval queue (FR-AD-09), moderation (FR-AD-10),
    /// and low-stock monitoring (FR-AD-11).
    /// </summary>
    public interface IAdminProductService
    {
        // ═══════════════════════════════════════════════════════════
        // FR-AD-09: Product Approval Queue
        // ═══════════════════════════════════════════════════════════

        /// <summary>Get full product detail for the review panel (any product, including soft-deleted).</summary>
        Task<AdminProductDetailDto> GetProductDetailAsync(int productId);

        /// <summary>
        /// Bulk-approve multiple pending products.
        /// Sends a notification to each seller.
        /// </summary>
        Task BulkApproveAsync(BulkProductActionDto dto, string adminId);

        /// <summary>
        /// Bulk-reject multiple pending products with a shared reason.
        /// Sends a notification to each seller.
        /// </summary>
        Task BulkRejectAsync(BulkProductActionDto dto, string adminId);

        /// <summary>
        /// Mark product as requiring changes (RequestedChanges status).
        /// Sends a notification to the seller with the reason.
        /// </summary>
        Task RequestChangesAsync(RejectProductDto dto, string adminId);

        // ═══════════════════════════════════════════════════════════
        // FR-AD-10: Product Moderation
        // ═══════════════════════════════════════════════════════════

        /// <summary>Get all products with rich filters for the moderation table.</summary>
        Task<Pagination<AdminProductDto>> GetAllProductsAsync(AdminProductFilterDto filter);

        /// <summary>Hide an approved product from the marketplace (sets IsVisible = false).</summary>
        Task HideProductAsync(int productId, string adminId);

        /// <summary>Restore a hidden product back to visible (sets IsVisible = true).</summary>
        Task RestoreProductAsync(int productId, string adminId);

        /// <summary>Feature a product on the homepage (sets IsFeatured = true).</summary>
        Task FeatureProductAsync(int productId, string adminId);

        /// <summary>Remove a product from the homepage feature list (sets IsFeatured = false).</summary>
        Task UnfeatureProductAsync(int productId, string adminId);

        /// <summary>Re-assign a product to a different category (FR-AD-10 re-categorize).</summary>
        Task ChangeProductCategoryAsync(ChangeProductCategoryDto dto, string adminId);

        /// <summary>Get per-product analytics for the moderation detail panel.</summary>
        Task<AdminProductAnalyticsDto> GetProductAnalyticsAsync(int productId);

        // ═══════════════════════════════════════════════════════════
        // FR-AD-11: Low Stock Alerts
        // ═══════════════════════════════════════════════════════════

        /// <summary>Get paginated list of low-stock products.</summary>
        Task<Pagination<LowStockProductDto>> GetLowStockProductsAsync(LowStockFilterDto filter);

        /// <summary>Manually notify the seller of a single low-stock product.</summary>
        Task NotifySellerLowStockAsync(int productId, string adminId);

        /// <summary>Send low-stock notifications to all sellers with at-threshold products.</summary>
        Task BulkNotifySellersLowStockAsync(string adminId);
    }
}
