using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Talentree.Service.DTOs.Admin.RawMaterial
{
    public class AdminRawMaterialDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Price { get; set; }
        public string Unit { get; set; } = null!;
        public int MinimumOrderQuantity { get; set; }
        public int StockQuantity { get; set; }
        public bool IsAvailable { get; set; }
        public string Category { get; set; } = null!;
        public string? PictureUrl { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }

    public class CreateRawMaterialDto
    {
        [Required][MaxLength(200)] public string Name { get; set; } = null!;
        [Required][MaxLength(1000)] public string Description { get; set; } = null!;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Required][MaxLength(50)] public string Unit { get; set; } = null!;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Minimum order quantity must be at least 1")]
        public int MinimumOrderQuantity { get; set; } = 1;

        [Required]
        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        /// <summary>
        /// Must match one of the supported categories:
        /// "Fashion & Accessories" | "Handmade Crafts" | "Natural & Beauty Products"
        /// A material can belong to multiple categories — store as comma-separated
        /// e.g. "Fashion & Accessories,Handmade Crafts"
        /// </summary>
        [Required][MaxLength(300)] public string Category { get; set; } = null!;

        [Required] public int SupplierId { get; set; }

        /// <summary>
        /// Optional — admin can upload image separately via /upload-image endpoint
        /// </summary>
        public string? PictureUrl { get; set; }
    }

    public class UpdateRawMaterialDto
    {
        [MaxLength(200)] public string? Name { get; set; }
        [MaxLength(1000)] public string? Description { get; set; }
        [Range(0.01, double.MaxValue)] public decimal? Price { get; set; }
        [MaxLength(50)] public string? Unit { get; set; }
        [Range(1, int.MaxValue)] public int? MinimumOrderQuantity { get; set; }
        [Range(0, int.MaxValue)] public int? StockQuantity { get; set; }
        [MaxLength(300)] public string? Category { get; set; }
        public int? SupplierId { get; set; }
        public bool? IsAvailable { get; set; }
        public string? PictureUrl { get; set; }
    }

    public class RestockMaterialDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int QuantityToAdd { get; set; }
    }
}
