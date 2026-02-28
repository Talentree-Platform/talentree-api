namespace Talentree.Core.Entities
{
    public class MaterialBasketItem: BaseEntity
    {
        public int BasketId { get; set; }
        public MaterialBasket Basket { get; set; } = null!;

        public int RawMaterialId { get; set; }
        public RawMaterial RawMaterial { get; set; } = null!;

        public int Quantity { get; set; }
    }
}