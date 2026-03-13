namespace Talentree.Core.Entities
{
    /// <summary>
    /// A single raw-material line inside a confirmed material order.
    /// The unit price is snapshotted at checkout and never changes,
    /// even if the raw-material price is updated later.
    /// </summary>
    public class MaterialOrderItem : BaseEntity
    {
        /// <summary>Foreign key to the parent <see cref="MaterialOrder"/>.</summary>
        public int MaterialOrderId { get; set; }

        /// <summary>Navigation property to the parent order.</summary>
        public MaterialOrder Order { get; set; } = null!;

        /// <summary>Foreign key to the raw material that was ordered.</summary>
        public int RawMaterialId { get; set; }

        /// <summary>Navigation property to the raw material — used for display.</summary>
        public RawMaterial RawMaterial { get; set; } = null!;

        /// <summary>Quantity ordered.</summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Unit price of the raw material at the moment of checkout.
        /// Locked permanently — changes to the catalogue do not affect this value.
        /// </summary>
        public decimal UnitPriceAtPurchase { get; set; }

        /// <summary>
        /// Computed line total: <see cref="Quantity"/> × <see cref="UnitPriceAtPurchase"/>.
        /// Not stored — ignored by EF Core.
        /// </summary>
        public decimal LineTotal => Quantity * UnitPriceAtPurchase;
    }
}