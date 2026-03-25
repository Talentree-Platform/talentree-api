// ============================================================
// Talentree.Repository/Data/Seed/MaterialOrderSeed.cs
// ============================================================
// Prerequisites: RawMaterialSeed must have run first.
// BO user IDs below are placeholder GUIDs — replace with real
// seeded BusinessOwner identity IDs from your AspNetUsers table.
// ============================================================
using Talentree.Core.Entities;
using Talentree.Core.Enums;

namespace Talentree.Repository.Data.DataSeed
{
    public static class MaterialOrderSeed
    {
        // ── Sourced from BusinessOwnerSeed — do not change independently ──
        private const string FashionBoId = BusinessOwnerSeed.FashionBoId;
        private const string CraftsBoId = BusinessOwnerSeed.CraftsBoId;
        private const string BeautyBoId = BusinessOwnerSeed.BeautyBoId;

        public static async Task SeedAsync(TalentreeDbContext context)
        {
            // Only seed if no material orders exist yet
            if (context.Set<MaterialOrder>().Any()) return;

            // ── Load seeded raw materials by name ─────────────────
            var cotton = context.Set<RawMaterial>().First(r => r.Name == "Cotton Fabric");
            var linen = context.Set<RawMaterial>().First(r => r.Name == "Linen Fabric");
            var denim = context.Set<RawMaterial>().First(r => r.Name == "Denim Fabric");
            var thread = context.Set<RawMaterial>().First(r => r.Name == "Sewing Thread (Polyester)");
            var zippers = context.Set<RawMaterial>().First(r => r.Name == "Metal Zippers (20cm)");
            var buttons = context.Set<RawMaterial>().First(r => r.Name == "Assorted Buttons (Pack of 50)");
            var fauxLeather = context.Set<RawMaterial>().First(r => r.Name == "Faux Leather Sheet");
            var elastic = context.Set<RawMaterial>().First(r => r.Name == "Elastic Band (2cm wide)");

            var soyWax = context.Set<RawMaterial>().First(r => r.Name == "Soy Wax Flakes");
            var resin = context.Set<RawMaterial>().First(r => r.Name == "Epoxy Resin Kit (A+B)");
            var clay = context.Set<RawMaterial>().First(r => r.Name == "Air Dry Clay");
            var macrame = context.Set<RawMaterial>().First(r => r.Name == "Macramé Cord (3mm)");
            var wicks = context.Set<RawMaterial>().First(r => r.Name == "Candle Wicks (Pack of 50)");
            var fragrance = context.Set<RawMaterial>().First(r => r.Name == "Candle Fragrance Oil");

            var sheaButter = context.Set<RawMaterial>().First(r => r.Name == "Shea Butter (Raw)");
            var coconutOil = context.Set<RawMaterial>().First(r => r.Name == "Coconut Oil (Fractionated)");
            var arganOil = context.Set<RawMaterial>().First(r => r.Name == "Argan Oil");
            var lavender = context.Set<RawMaterial>().First(r => r.Name == "Lavender Essential Oil");
            var soapBase = context.Set<RawMaterial>().First(r => r.Name == "Soap Base (Melt & Pour)");
            var beeswax = context.Set<RawMaterial>().First(r => r.Name == "Beeswax (Pellets)");

            var kraftBoxes = context.Set<RawMaterial>().First(r => r.Name == "Kraft Paper Boxes (Small)");
            var glassJar = context.Set<RawMaterial>().First(r => r.Name == "Glass Jar (50ml)");
            var labels = context.Set<RawMaterial>().First(r => r.Name == "Custom Label Paper (A4)");

            // ══════════════════════════════════════════════════════
            // FASHION BO — 4 orders across different statuses
            // ══════════════════════════════════════════════════════

            var fashionOrder1 = new MaterialOrder
            {
                BusinessOwnerId = FashionBoId,
                DeliveryAddress = "14 Al-Tahrir Square, Apt 3",
                DeliveryCity = "Cairo",
                DeliveryCountry = "Egypt",
                ContactPhone = "+20-100-123-4567",
                PaymentMethod = "CashOnDelivery",
                Status = MaterialOrderStatus.Delivered,
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                Items = new List<MaterialOrderItem>
                {
                    new() { RawMaterialId = cotton.Id,  Quantity = 20, UnitPriceAtPurchase = cotton.Price },
                    new() { RawMaterialId = thread.Id,  Quantity = 30, UnitPriceAtPurchase = thread.Price },
                    new() { RawMaterialId = buttons.Id, Quantity = 10, UnitPriceAtPurchase = buttons.Price },
                }
            };
            fashionOrder1.TotalAmount = fashionOrder1.Items.Sum(i => i.LineTotal);

            var fashionOrder2 = new MaterialOrder
            {
                BusinessOwnerId = FashionBoId,
                DeliveryAddress = "14 Al-Tahrir Square, Apt 3",
                DeliveryCity = "Cairo",
                DeliveryCountry = "Egypt",
                ContactPhone = "+20-100-123-4567",
                PaymentMethod = "CashOnDelivery",
                Status = MaterialOrderStatus.Shipped,
                CreatedAt = DateTime.UtcNow.AddDays(-12),
                Items = new List<MaterialOrderItem>
                {
                    new() { RawMaterialId = denim.Id,      Quantity = 10, UnitPriceAtPurchase = denim.Price },
                    new() { RawMaterialId = zippers.Id,    Quantity = 50, UnitPriceAtPurchase = zippers.Price },
                    new() { RawMaterialId = fauxLeather.Id, Quantity = 5, UnitPriceAtPurchase = fauxLeather.Price },
                }
            };
            fashionOrder2.TotalAmount = fashionOrder2.Items.Sum(i => i.LineTotal);

            var fashionOrder3 = new MaterialOrder
            {
                BusinessOwnerId = FashionBoId,
                DeliveryAddress = "14 Al-Tahrir Square, Apt 3",
                DeliveryCity = "Cairo",
                DeliveryCountry = "Egypt",
                ContactPhone = "+20-100-123-4567",
                PaymentMethod = "CashOnDelivery",
                Status = MaterialOrderStatus.Processing,
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                Items = new List<MaterialOrderItem>
                {
                    new() { RawMaterialId = linen.Id,   Quantity = 15, UnitPriceAtPurchase = linen.Price },
                    new() { RawMaterialId = elastic.Id, Quantity = 30, UnitPriceAtPurchase = elastic.Price },
                    new() { RawMaterialId = thread.Id,  Quantity = 20, UnitPriceAtPurchase = thread.Price },
                }
            };
            fashionOrder3.TotalAmount = fashionOrder3.Items.Sum(i => i.LineTotal);

            var fashionOrder4 = new MaterialOrder
            {
                BusinessOwnerId = FashionBoId,
                DeliveryAddress = "14 Al-Tahrir Square, Apt 3",
                DeliveryCity = "Cairo",
                DeliveryCountry = "Egypt",
                ContactPhone = "+20-100-123-4567",
                PaymentMethod = "CashOnDelivery",
                Status = MaterialOrderStatus.Pending,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                Items = new List<MaterialOrderItem>
                {
                    new() { RawMaterialId = cotton.Id,  Quantity = 30, UnitPriceAtPurchase = cotton.Price },
                    new() { RawMaterialId = buttons.Id, Quantity = 15, UnitPriceAtPurchase = buttons.Price },
                }
            };
            fashionOrder4.TotalAmount = fashionOrder4.Items.Sum(i => i.LineTotal);

            // ══════════════════════════════════════════════════════
            // CRAFTS BO — 3 orders
            // ══════════════════════════════════════════════════════

            var craftsOrder1 = new MaterialOrder
            {
                BusinessOwnerId = CraftsBoId,
                DeliveryAddress = "7 Corniche El-Nil, Floor 2",
                DeliveryCity = "Alexandria",
                DeliveryCountry = "Egypt",
                ContactPhone = "+20-110-987-6543",
                PaymentMethod = "CashOnDelivery",
                Status = MaterialOrderStatus.Delivered,
                CreatedAt = DateTime.UtcNow.AddDays(-45),
                Items = new List<MaterialOrderItem>
                {
                    new() { RawMaterialId = soyWax.Id,   Quantity = 5,  UnitPriceAtPurchase = soyWax.Price },
                    new() { RawMaterialId = wicks.Id,    Quantity = 10, UnitPriceAtPurchase = wicks.Price },
                    new() { RawMaterialId = fragrance.Id, Quantity = 6, UnitPriceAtPurchase = fragrance.Price },
                    new() { RawMaterialId = kraftBoxes.Id, Quantity = 100, UnitPriceAtPurchase = kraftBoxes.Price },
                }
            };
            craftsOrder1.TotalAmount = craftsOrder1.Items.Sum(i => i.LineTotal);

            var craftsOrder2 = new MaterialOrder
            {
                BusinessOwnerId = CraftsBoId,
                DeliveryAddress = "7 Corniche El-Nil, Floor 2",
                DeliveryCity = "Alexandria",
                DeliveryCountry = "Egypt",
                ContactPhone = "+20-110-987-6543",
                PaymentMethod = "CashOnDelivery",
                Status = MaterialOrderStatus.Refunded,
                CreatedAt = DateTime.UtcNow.AddDays(-20),
                Items = new List<MaterialOrderItem>
                {
                    new() { RawMaterialId = resin.Id,  Quantity = 3, UnitPriceAtPurchase = resin.Price },
                    new() { RawMaterialId = clay.Id,   Quantity = 4, UnitPriceAtPurchase = clay.Price },
                }
            };
            craftsOrder2.TotalAmount = craftsOrder2.Items.Sum(i => i.LineTotal);

            var craftsOrder3 = new MaterialOrder
            {
                BusinessOwnerId = CraftsBoId,
                DeliveryAddress = "7 Corniche El-Nil, Floor 2",
                DeliveryCity = "Alexandria",
                DeliveryCountry = "Egypt",
                ContactPhone = "+20-110-987-6543",
                PaymentMethod = "CashOnDelivery",
                Status = MaterialOrderStatus.Pending,
                CreatedAt = DateTime.UtcNow.AddHours(-6),
                Items = new List<MaterialOrderItem>
                {
                    new() { RawMaterialId = macrame.Id,   Quantity = 6,  UnitPriceAtPurchase = macrame.Price },
                    new() { RawMaterialId = soyWax.Id,    Quantity = 4,  UnitPriceAtPurchase = soyWax.Price },
                    new() { RawMaterialId = labels.Id,    Quantity = 20, UnitPriceAtPurchase = labels.Price },
                }
            };
            craftsOrder3.TotalAmount = craftsOrder3.Items.Sum(i => i.LineTotal);

            // ══════════════════════════════════════════════════════
            // BEAUTY BO — 3 orders
            // ══════════════════════════════════════════════════════

            var beautyOrder1 = new MaterialOrder
            {
                BusinessOwnerId = BeautyBoId,
                DeliveryAddress = "22 Hassan Allam Street",
                DeliveryCity = "Giza",
                DeliveryCountry = "Egypt",
                ContactPhone = "+20-122-456-7890",
                PaymentMethod = "CashOnDelivery",
                Status = MaterialOrderStatus.Delivered,
                CreatedAt = DateTime.UtcNow.AddDays(-60),
                Items = new List<MaterialOrderItem>
                {
                    new() { RawMaterialId = sheaButter.Id,  Quantity = 5,  UnitPriceAtPurchase = sheaButter.Price },
                    new() { RawMaterialId = coconutOil.Id,  Quantity = 4,  UnitPriceAtPurchase = coconutOil.Price },
                    new() { RawMaterialId = lavender.Id,    Quantity = 10, UnitPriceAtPurchase = lavender.Price },
                    new() { RawMaterialId = glassJar.Id,    Quantity = 48, UnitPriceAtPurchase = glassJar.Price },
                    new() { RawMaterialId = labels.Id,      Quantity = 15, UnitPriceAtPurchase = labels.Price },
                }
            };
            beautyOrder1.TotalAmount = beautyOrder1.Items.Sum(i => i.LineTotal);

            var beautyOrder2 = new MaterialOrder
            {
                BusinessOwnerId = BeautyBoId,
                DeliveryAddress = "22 Hassan Allam Street",
                DeliveryCity = "Giza",
                DeliveryCountry = "Egypt",
                ContactPhone = "+20-122-456-7890",
                PaymentMethod = "CashOnDelivery",
                Status = MaterialOrderStatus.Shipped,
                CreatedAt = DateTime.UtcNow.AddDays(-7),
                Items = new List<MaterialOrderItem>
                {
                    new() { RawMaterialId = arganOil.Id,  Quantity = 10, UnitPriceAtPurchase = arganOil.Price },
                    new() { RawMaterialId = beeswax.Id,   Quantity = 3,  UnitPriceAtPurchase = beeswax.Price },
                    new() { RawMaterialId = soapBase.Id,  Quantity = 6,  UnitPriceAtPurchase = soapBase.Price },
                }
            };
            beautyOrder2.TotalAmount = beautyOrder2.Items.Sum(i => i.LineTotal);

            var beautyOrder3 = new MaterialOrder
            {
                BusinessOwnerId = BeautyBoId,
                DeliveryAddress = "22 Hassan Allam Street",
                DeliveryCity = "Giza",
                DeliveryCountry = "Egypt",
                ContactPhone = "+20-122-456-7890",
                PaymentMethod = "CashOnDelivery",
                Status = MaterialOrderStatus.Processing,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                Items = new List<MaterialOrderItem>
                {
                    new() { RawMaterialId = sheaButter.Id, Quantity = 3,  UnitPriceAtPurchase = sheaButter.Price },
                    new() { RawMaterialId = lavender.Id,   Quantity = 8,  UnitPriceAtPurchase = lavender.Price },
                    new() { RawMaterialId = glassJar.Id,   Quantity = 72, UnitPriceAtPurchase = glassJar.Price },
                    new() { RawMaterialId = labels.Id,     Quantity = 10, UnitPriceAtPurchase = labels.Price },
                }
            };
            beautyOrder3.TotalAmount = beautyOrder3.Items.Sum(i => i.LineTotal);

            // ── Persist all orders ────────────────────────────────
            context.Set<MaterialOrder>().AddRange(
                fashionOrder1, fashionOrder2, fashionOrder3, fashionOrder4,
                craftsOrder1, craftsOrder2, craftsOrder3,
                beautyOrder1, beautyOrder2, beautyOrder3
            );

            await context.SaveChangesAsync();
        }
    }
}