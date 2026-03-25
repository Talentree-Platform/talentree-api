namespace Talentree.Core.Entities
{
    /// <summary>
    /// A single production line inside a <see cref="BoProductionRequest"/>.
    /// Describes one type of product to manufacture, the desired quantity,
    /// and optionally which raw material from Talentree's catalogue to use.
    /// </summary>
    public class BoProductionRequestItem : AuditableEntity
    {
        /// <summary>Foreign key to the parent <see cref="BoProductionRequest"/>.</summary>
        public int BoProductionRequestId { get; set; }

        /// <summary>Navigation property to the parent request.</summary>
        public BoProductionRequest Request { get; set; } = null!;

        /// <summary>
        /// Type of product to be manufactured. Free text entered by the BO.
        /// Example: "Hoodie", "Crossbody Bag", "Scented Candle"
        /// </summary>
        public string ProductType { get; set; } = null!;

        /// <summary>Number of units to produce for this line.</summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Optional: ID of the raw material from Talentree's store that the BO wants used.
        /// When null, Talentree selects the most suitable available material.
        /// </summary>
        public int? PreferredRawMaterialId { get; set; }

        /// <summary>Navigation property to the preferred raw material — null when none selected.</summary>
        public RawMaterial? PreferredRawMaterial { get; set; }

        /// <summary>
        /// Optional item-level specifications from the BO:
        /// colour, size, finish, design reference, etc.
        /// </summary>
        public string? Specifications { get; set; }
    }
}