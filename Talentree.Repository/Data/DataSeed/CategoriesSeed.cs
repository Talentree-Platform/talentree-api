using Talentree.Core.Entities;
using Talentree.Core.Enums;

namespace Talentree.Repository.Data.DataSeed
{
    public static class CategoriesSeed
    {
        public static async Task SeedAsync(TalentreeDbContext context)
        {
            // ═══════════════════════════════════════════════════════════
            // SEED CATEGORIES
            // ═══════════════════════════════════════════════════════════
            if (!context.Categories.Any())
            {
                var categories = new List<Category>
                {
                    new() { Name = "Fashion" },
                    new() { Name = "Electronics" },
                    new() { Name = "Food" },
                    new() { Name = "Home & Garden" },
                    new() { Name = "Beauty" },
                    new() { Name = "Sports" },
                    new() { Name = "Books" },
                    new() { Name = "Handicrafts" },
                    new() { Name = "Jewelry" },
                    new() { Name = "Other" }
                };

                await context.Categories.AddRangeAsync(categories);
                await context.SaveChangesAsync();
            }

            // ═══════════════════════════════════════════════════════════
            // SEED PRODUCTS (only if business owner profiles exist)
            // ═══════════════════════════════════════════════════════════
            if (context.Products.Any()) return;

            // Get seeded business owner profiles
            var techGalaxyProfile = context.BusinessOwnerProfiles
                .FirstOrDefault(b => b.BusinessName == "Tech Galaxy");

            var fashionHouseProfile = context.BusinessOwnerProfiles
                .FirstOrDefault(b => b.BusinessName == "Fashion House");

            var beautyCorderProfile = context.BusinessOwnerProfiles
                .FirstOrDefault(b => b.BusinessName == "Beauty Corner");

            var sportsZoneProfile = context.BusinessOwnerProfiles
                .FirstOrDefault(b => b.BusinessName == "Sports Zone");

            if (techGalaxyProfile == null) return; // no business owners seeded yet

            // Get category IDs
            var electronics = context.Categories.First(c => c.Name == "Electronics");
            var fashion = context.Categories.First(c => c.Name == "Fashion");
            var beauty = context.Categories.First(c => c.Name == "Beauty");
            var sports = context.Categories.First(c => c.Name == "Sports");

            var products = new List<Product>();

            // ─────────────────────────────────────────────────────────
            // Tech Galaxy Products (Approved)
            // ─────────────────────────────────────────────────────────
            if (techGalaxyProfile != null)
            {
                products.AddRange(new[]
                {
                    new Product
                    {
                        Name = "Wireless Bluetooth Headphones",
                        Description = "Premium over-ear wireless headphones with active noise cancellation, 30-hour battery life, and crystal clear sound quality. Perfect for music lovers and professionals.",
                        Price = 899.99m,
                        StockQuantity = 50,
                        Tags = "headphones,wireless,bluetooth,noise-cancellation,audio",
                        CategoryId = electronics.Id,
                        BusinessOwnerProfileId = techGalaxyProfile.Id,
                        Status = ProductStatus.Approved,
                        ApprovedBy = "admin@talentree.com",
                        ApprovedAt = DateTime.UtcNow.AddDays(-8)
                    },
                    new Product
                    {
                        Name = "Smart Watch Series X",
                        Description = "Advanced smartwatch with health monitoring, GPS tracking, sleep analysis, and 7-day battery life. Compatible with both Android and iOS devices.",
                        Price = 1299.99m,
                        StockQuantity = 30,
                        Tags = "smartwatch,fitness,GPS,health,wearable",
                        CategoryId = electronics.Id,
                        BusinessOwnerProfileId = techGalaxyProfile.Id,
                        Status = ProductStatus.Approved,
                        ApprovedBy = "admin@talentree.com",
                        ApprovedAt = DateTime.UtcNow.AddDays(-7)
                    },
                    new Product
                    {
                        Name = "Portable Power Bank 20000mAh",
                        Description = "High-capacity portable charger with fast charging support, dual USB ports, and USB-C input/output. Charge your phone up to 5 times on a single charge.",
                        Price = 349.99m,
                        StockQuantity = 100,
                        Tags = "powerbank,charger,portable,fast-charging,USB-C",
                        CategoryId = electronics.Id,
                        BusinessOwnerProfileId = techGalaxyProfile.Id,
                        Status = ProductStatus.PendingApproval,
                    },
                    new Product
                    {
                        Name = "Mechanical Gaming Keyboard",
                        Description = "RGB backlit mechanical keyboard with blue switches, anti-ghosting technology, and aluminum frame. Designed for professional gamers and heavy typists.",
                        Price = 649.99m,
                        StockQuantity = 25,
                        Tags = "keyboard,gaming,mechanical,RGB,peripheral",
                        CategoryId = electronics.Id,
                        BusinessOwnerProfileId = techGalaxyProfile.Id,
                        Status = ProductStatus.Rejected,
                        RejectionReason = "Product images do not meet quality standards. Please resubmit with clearer photos."
                    }
                });
            }

            // ─────────────────────────────────────────────────────────
            // Fashion House Products (Pending)
            // ─────────────────────────────────────────────────────────
            if (fashionHouseProfile != null)
            {
                products.AddRange(new[]
                {
                    new Product
                    {
                        Name = "Classic Linen Summer Dress",
                        Description = "Elegant linen summer dress with a relaxed fit, available in multiple colors. Breathable fabric perfect for warm weather. Hand wash recommended.",
                        Price = 299.99m,
                        StockQuantity = 40,
                        Tags = "dress,summer,linen,women,fashion",
                        CategoryId = fashion.Id,
                        BusinessOwnerProfileId = fashionHouseProfile.Id,
                        Status = ProductStatus.PendingApproval,
                    },
                    new Product
                    {
                        Name = "Handcrafted Leather Handbag",
                        Description = "Genuine leather handbag handcrafted by local artisans. Features multiple compartments, gold-tone hardware, and an adjustable shoulder strap.",
                        Price = 549.99m,
                        StockQuantity = 15,
                        Tags = "handbag,leather,handcrafted,women,accessories",
                        CategoryId = fashion.Id,
                        BusinessOwnerProfileId = fashionHouseProfile.Id,
                        Status = ProductStatus.PendingApproval,
                    },
                    new Product
                    {
                        Name = "Embroidered Casual Shirt",
                        Description = "Cotton casual shirt with traditional Egyptian embroidery details. Comfortable everyday wear that blends modern style with cultural heritage.",
                        Price = 189.99m,
                        StockQuantity = 60,
                        Tags = "shirt,embroidered,cotton,casual,unisex",
                        CategoryId = fashion.Id,
                        BusinessOwnerProfileId = fashionHouseProfile.Id,
                        Status = ProductStatus.Draft,
                    }
                });
            }

            // ─────────────────────────────────────────────────────────
            // Beauty Corner Products (Approved)
            // ─────────────────────────────────────────────────────────
            if (beautyCorderProfile != null)
            {
                products.AddRange(new[]
                {
                    new Product
                    {
                        Name = "Argan Oil Hair Treatment",
                        Description = "100% pure Moroccan argan oil hair treatment. Repairs damaged hair, eliminates frizz, and adds brilliant shine. Suitable for all hair types.",
                        Price = 179.99m,
                        StockQuantity = 80,
                        Tags = "hair,argan-oil,treatment,natural,beauty",
                        CategoryId = beauty.Id,
                        BusinessOwnerProfileId = beautyCorderProfile.Id,
                        Status = ProductStatus.Approved,
                        ApprovedBy = "admin@talentree.com",
                        ApprovedAt = DateTime.UtcNow.AddDays(-1)
                    },
                    new Product
                    {
                        Name = "Natural Rose Water Toner",
                        Description = "Pure Bulgarian rose water facial toner. Hydrates, balances skin pH, and reduces redness. Free from alcohol, parabens, and artificial fragrances.",
                        Price = 89.99m,
                        StockQuantity = 120,
                        Tags = "toner,rosewater,skincare,natural,face",
                        CategoryId = beauty.Id,
                        BusinessOwnerProfileId = beautyCorderProfile.Id,
                        Status = ProductStatus.Approved,
                        ApprovedBy = "admin@talentree.com",
                        ApprovedAt = DateTime.UtcNow.AddDays(-1)
                    },
                    new Product
                    {
                        Name = "Vitamin C Brightening Serum",
                        Description = "Advanced vitamin C serum with 20% concentration. Fades dark spots, evens skin tone, and boosts collagen production for younger-looking skin.",
                        Price = 229.99m,
                        StockQuantity = 45,
                        Tags = "serum,vitamin-c,brightening,skincare,anti-aging",
                        CategoryId = beauty.Id,
                        BusinessOwnerProfileId = beautyCorderProfile.Id,
                        Status = ProductStatus.PendingApproval,
                    }
                });
            }

            // ─────────────────────────────────────────────────────────
            // Sports Zone Products (Pending - new account)
            // ─────────────────────────────────────────────────────────
            if (sportsZoneProfile != null)
            {
                products.AddRange(new[]
                {
                    new Product
                    {
                        Name = "Professional Yoga Mat",
                        Description = "Extra thick 6mm non-slip yoga mat made from eco-friendly TPE material. Includes carrying strap and comes in 5 colors. Perfect for yoga, pilates and floor exercises.",
                        Price = 199.99m,
                        StockQuantity = 70,
                        Tags = "yoga,mat,fitness,exercise,eco-friendly",
                        CategoryId = sports.Id,
                        BusinessOwnerProfileId = sportsZoneProfile.Id,
                        Status = ProductStatus.PendingApproval,
                    },
                    new Product
                    {
                        Name = "Adjustable Dumbbell Set",
                        Description = "Space-saving adjustable dumbbell set ranging from 2kg to 24kg. Quick-change weight selector mechanism. Includes storage stand.",
                        Price = 1499.99m,
                        StockQuantity = 20,
                        Tags = "dumbbells,weights,fitness,gym,strength-training",
                        CategoryId = sports.Id,
                        BusinessOwnerProfileId = sportsZoneProfile.Id,
                        Status = ProductStatus.PendingApproval,
                    }
                });
            }

            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
        }
    }
}