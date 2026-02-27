using Talentree.Core.Entities;

namespace Talentree.Core.Specifications.Basket
{
    /// <summary>
    /// Specification for fetching a BO's basket with full nested item details.
    /// Uses string-based include to load Items → RawMaterial → Supplier in one query.
    /// </summary>
    public class MaterialBasketWithItemsSpec : BaseSpecifications<MaterialBasket>
    {
        public MaterialBasketWithItemsSpec(string businessOwnerId)
            : base(b => b.BusinessOwnerId == businessOwnerId)
        {
            // String include handles the nested ThenInclude chain
            AddInclude("Items.RawMaterial.Supplier");
        }
    }
}