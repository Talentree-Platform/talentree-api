using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Talentree.Service.DTOs.Basket
{
    public class MaterialBasketDto
    {
        public int Id { get; set; }
        public List<MaterialBasketItemDto> Items { get; set; } = new();

        // Computed — not stored in DB
        public decimal Subtotal => Items.Sum(i => i.LineTotal);
        public int TotalItemCount => Items.Sum(i => i.Quantity);
    }

    public class MaterialBasketItemDto
    {
        public int Id { get; set; }
        public int RawMaterialId { get; set; }
        public string MaterialName { get; set; } = null!;
        public string? PictureUrl { get; set; }
        public string Unit { get; set; } = null!;
        public string SupplierName { get; set; } = null!;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public int MinimumOrderQuantity { get; set; }

        // Computed — not stored in DB
        public decimal LineTotal => UnitPrice * Quantity;
    }
}
