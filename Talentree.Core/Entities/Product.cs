namespace Talentree.Core.Entities
{
    /// <summary>
    /// Represents a product in the Talentree e-commerce catalog
    /// </summary>
    /// <remarks>
    /// This is a core domain entity that represents physical or digital products
    /// that can be sold on the platform. Each product has basic information
    /// like name, description, price, and image.
    /// </remarks>
    public class Product : BaseEntity
    {
        /// <summary>
        /// Product name (e.g., "iPhone 15 Pro", "Nike Air Max 90")
        /// </summary>
        /// <remarks>
        /// Required field - every product must have a name
        /// Maximum length: 200 characters (configured in ProductConfiguration)
        /// </remarks>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Detailed product description
        /// </summary>
        /// <remarks>
        /// Required field - provides detailed information about the product
        /// Maximum length: 1000 characters (configured in ProductConfiguration)
        /// Can include features, specifications, materials, etc.
        /// </remarks>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Product price in USD
        /// </summary>
        /// <remarks>
        /// Required field - must be greater than 0
        /// Stored as decimal(18,2) in database
        /// Always use decimal for money to avoid floating-point precision issues
        /// </remarks>
        public decimal Price { get; set; }

        /// <summary>
        /// URL or path to the product image
        /// </summary>
        /// <remarks>
        /// Required field - visual representation is crucial for e-commerce
        /// Maximum length: 500 characters
        /// Can be:
        /// - Relative path: "images/products/iphone15.png"
        /// - Absolute URL: "https://cdn.talentree.com/products/iphone15.png"
        /// </remarks>
        public string PictureUrl { get; set; } = string.Empty;

        // ===============================
        // Future Relationships (Commented for now)
        // ===============================
        // These will be added in future iterations

        // /// <summary>
        // /// Foreign key to ProductType (Category)
        // /// </summary>
        // public int ProductTypeId { get; set; }

        // /// <summary>
        // /// Navigation property to ProductType (Category)
        // /// </summary>
        // public ProductType ProductType { get; set; } = null!;

        // /// <summary>
        // /// Foreign key to ProductBrand
        // /// </summary>
        // public int ProductBrandId { get; set; }

        // /// <summary>
        // /// Navigation property to ProductBrand
        // /// </summary>
        // public ProductBrand ProductBrand { get; set; } = null!;

        // /// <summary>
        // /// Quantity available in stock
        // /// </summary>
        // public int QuantityInStock { get; set; }
    }
}