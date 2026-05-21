// ============================================================
// Talentree.Repository/Data/DataSeed/CategoriesSeed.cs
// ============================================================
// Seeds categories and an enriched set of products with:
//   - Real product images (Unsplash)
//   - ViewCount / CartAddCount / PurchaseCount for trending
//   - AvgRating for display on cards
//   - IsFeatured / FeaturedOrder for homepage carousel
//   - IsVisible = true for all approved products
// ============================================================
using Talentree.Core.Entities;
using Talentree.Core.Enums;

namespace Talentree.Repository.Data.DataSeed
{
    public static class CategoriesSeed
    {
        public static async Task SeedAsync(TalentreeDbContext context)
        {
            // ═══════════════════════════════════════════════════════════
            // STEP 1 — Categories
            // ═══════════════════════════════════════════════════════════
            if (!context.Categories.Any())
            {
                var categories = new List<Category>
                {
                    new() { Name = "Fashion & Accessories", BusinessType = "Fashion",
                            Description = "Handcrafted clothing, bags, scarves, jewellery and accessories made by local artisans." },
                    new() { Name = "Handmade & Crafts",     BusinessType = "Crafts",
                            Description = "Unique home décor, resin art, macramé, candles and handcrafted gifts." },
                    new() { Name = "Natural & Beauty Products", BusinessType = "Beauty",
                            Description = "Organic skincare, cold-process soaps, argan oils and natural cosmetics." }
                };

                await context.Categories.AddRangeAsync(categories);
                await context.SaveChangesAsync();
            }

            // ═══════════════════════════════════════════════════════════
            // STEP 2 — Products (skip if already seeded)
            // ═══════════════════════════════════════════════════════════
            if (context.Products.Any()) return;

            // Use the fixed-GUID profiles from BusinessOwnerSeed
            var fashionProfile = context.BusinessOwnerProfiles
                .FirstOrDefault(b => b.UserId == BusinessOwnerSeed.FashionBoId);

            var craftsProfile = context.BusinessOwnerProfiles
                .FirstOrDefault(b => b.UserId == BusinessOwnerSeed.CraftsBoId);

            var beautyProfile = context.BusinessOwnerProfiles
                .FirstOrDefault(b => b.UserId == BusinessOwnerSeed.BeautyBoId);

            if (fashionProfile == null || craftsProfile == null || beautyProfile == null)
                return; // BOs not seeded yet — will retry on next startup

            var fashion = context.Categories.First(c => c.Name == "Fashion & Accessories");
            var crafts  = context.Categories.First(c => c.Name == "Handmade & Crafts");
            var beauty  = context.Categories.First(c => c.Name == "Natural & Beauty Products");

            // ─────────────────────────────────────────────────────────
            // FASHION & ACCESSORIES — Nour Couture (FashionBoId)
            // ─────────────────────────────────────────────────────────
            var p1 = new Product
            {
                Name              = "Handwoven Cotton Tote Bag",
                Description       = "Eco-friendly tote bag handwoven from 100% Egyptian cotton. Features double handles, inner pocket, and traditional embroidery details. Perfect for daily use and shopping. Durable and washable.",
                Price             = 189.99m,
                StockQuantity     = 45,
                Tags              = "tote,cotton,handwoven,eco-friendly,bag",
                CategoryId        = fashion.Id,
                BusinessOwnerProfileId = fashionProfile.Id,
                Status            = ProductStatus.Approved,
                ApprovedBy        = "seed",
                ApprovedAt        = DateTime.UtcNow.AddDays(-30),
                IsVisible         = true,
                IsFeatured        = true,
                FeaturedOrder     = 1,
                ViewCount         = 1240,
                CartAddCount      = 320,
                PurchaseCount     = 187,
                AvgRating         = 4.7f,
                RevenueTotal      = 35556.13m,
                Images = new List<ProductImage>
                {
                    new() { ImageUrl = "https://images.unsplash.com/photo-1553062407-98eeb64c6a62?w=800", IsMain = true,  SortOrder = 0 },
                    new() { ImageUrl = "https://images.unsplash.com/photo-1565693413579-8a3a5e26dd01?w=800", IsMain = false, SortOrder = 1 },
                    new() { ImageUrl = "https://images.unsplash.com/photo-1590874103328-eac38a683ce7?w=800", IsMain = false, SortOrder = 2 }
                }
            };

            var p2 = new Product
            {
                Name              = "Leather Strap Bracelet Set",
                Description       = "Set of 3 genuine leather strap bracelets with brass buckle closures. Handcrafted by local artisans using premium leather. Available in brown, black, and tan. Adjustable fit.",
                Price             = 149.99m,
                StockQuantity     = 60,
                Tags              = "bracelet,leather,handcrafted,accessories,jewelry",
                CategoryId        = fashion.Id,
                BusinessOwnerProfileId = fashionProfile.Id,
                Status            = ProductStatus.Approved,
                ApprovedBy        = "seed",
                ApprovedAt        = DateTime.UtcNow.AddDays(-28),
                IsVisible         = true,
                IsFeatured        = false,
                ViewCount         = 870,
                CartAddCount      = 210,
                PurchaseCount     = 134,
                AvgRating         = 4.5f,
                RevenueTotal      = 20098.66m,
                Images = new List<ProductImage>
                {
                    new() { ImageUrl = "https://images.unsplash.com/photo-1611085583191-a3b181a88577?w=800", IsMain = true,  SortOrder = 0 },
                    new() { ImageUrl = "https://images.unsplash.com/photo-1620625515032-6ed0c1790c75?w=800", IsMain = false, SortOrder = 1 }
                }
            };

            var p3 = new Product
            {
                Name              = "Embroidered Linen Scarf",
                Description       = "Lightweight linen scarf with hand-embroidered floral patterns. Inspired by traditional Egyptian designs. Suitable for all seasons. Dimensions: 180cm x 50cm. Hand wash only.",
                Price             = 229.99m,
                StockQuantity     = 30,
                Tags              = "scarf,linen,embroidered,traditional,fashion",
                CategoryId        = fashion.Id,
                BusinessOwnerProfileId = fashionProfile.Id,
                Status            = ProductStatus.Approved,
                ApprovedBy        = "seed",
                ApprovedAt        = DateTime.UtcNow.AddDays(-20),
                IsVisible         = true,
                IsFeatured        = true,
                FeaturedOrder     = 4,
                ViewCount         = 560,
                CartAddCount      = 140,
                PurchaseCount     = 89,
                AvgRating         = 4.8f,
                RevenueTotal      = 20469.11m,
                Images = new List<ProductImage>
                {
                    new() { ImageUrl = "https://images.unsplash.com/photo-1601924994987-69e26d50dc26?w=800", IsMain = true,  SortOrder = 0 },
                    new() { ImageUrl = "https://images.unsplash.com/photo-1574180045827-681f8a1a9622?w=800", IsMain = false, SortOrder = 1 }
                }
            };

            var p4 = new Product
            {
                Name              = "Denim Crossbody Bag",
                Description       = "Stylish crossbody bag made from upcycled denim with leather trim. Features adjustable strap, zipper closure, and 2 inner compartments. Unique one-of-a-kind piece.",
                Price             = 275.00m,
                StockQuantity     = 20,
                Tags              = "crossbody,denim,bag,upcycled,fashion",
                CategoryId        = fashion.Id,
                BusinessOwnerProfileId = fashionProfile.Id,
                Status            = ProductStatus.Approved,
                ApprovedBy        = "seed",
                ApprovedAt        = DateTime.UtcNow.AddDays(-15),
                IsVisible         = true,
                IsFeatured        = false,
                ViewCount         = 430,
                CartAddCount      = 95,
                PurchaseCount     = 55,
                AvgRating         = 4.3f,
                RevenueTotal      = 15125.00m,
                Images = new List<ProductImage>
                {
                    new() { ImageUrl = "https://images.unsplash.com/photo-1548036328-c9fa89d128fa?w=800", IsMain = true,  SortOrder = 0 },
                    new() { ImageUrl = "https://images.unsplash.com/photo-1584917865442-de89df76afd3?w=800", IsMain = false, SortOrder = 1 }
                }
            };

            var p5 = new Product
            {
                Name              = "Hand-Painted Silk Kimono",
                Description       = "Luxurious silk kimono with unique hand-painted botanical motifs. Each piece is a wearable work of art, painted with non-toxic fabric dyes. One size fits all. Dry clean recommended.",
                Price             = 450.00m,
                StockQuantity     = 12,
                Tags              = "kimono,silk,hand-painted,luxury,fashion",
                CategoryId        = fashion.Id,
                BusinessOwnerProfileId = fashionProfile.Id,
                Status            = ProductStatus.Approved,
                ApprovedBy        = "seed",
                ApprovedAt        = DateTime.UtcNow.AddDays(-10),
                IsVisible         = true,
                IsFeatured        = true,
                FeaturedOrder     = 2,
                ViewCount         = 2100,
                CartAddCount      = 480,
                PurchaseCount     = 62,
                AvgRating         = 4.9f,
                RevenueTotal      = 27900.00m,
                Images = new List<ProductImage>
                {
                    new() { ImageUrl = "https://images.unsplash.com/photo-1594938298603-c8148c4b4e36?w=800", IsMain = true,  SortOrder = 0 },
                    new() { ImageUrl = "https://images.unsplash.com/photo-1512436991641-6745cdb1723f?w=800", IsMain = false, SortOrder = 1 },
                    new() { ImageUrl = "https://images.unsplash.com/photo-1567401893414-76b7b1e5a7a5?w=800", IsMain = false, SortOrder = 2 }
                }
            };

            // Pending — not visible to customers
            var p6 = new Product
            {
                Name              = "Crochet Summer Hat",
                Description       = "Hand-crocheted wide-brim sun hat using raffia and cotton blend. UV protective. Foldable for travel.",
                Price             = 199.00m,
                StockQuantity     = 25,
                Tags              = "hat,crochet,summer,handmade,accessories",
                CategoryId        = fashion.Id,
                BusinessOwnerProfileId = fashionProfile.Id,
                Status            = ProductStatus.PendingApproval,
                IsVisible         = false,
                Images = new List<ProductImage>
                {
                    new() { ImageUrl = "https://images.unsplash.com/photo-1521369909029-2afed882baee?w=800", IsMain = true, SortOrder = 0 }
                }
            };

            // ─────────────────────────────────────────────────────────
            // HANDMADE & CRAFTS — Karim Craft Studio (CraftsBoId)
            // ─────────────────────────────────────────────────────────
            var p7 = new Product
            {
                Name              = "Resin Art Coaster Set",
                Description       = "Set of 4 handmade resin coasters with dried flower inclusions. Each piece is unique with swirling colour patterns. Heat resistant up to 80°C. Comes in a gift box. Perfect for home décor.",
                Price             = 199.99m,
                StockQuantity     = 35,
                Tags              = "resin,coasters,handmade,home-decor,gift",
                CategoryId        = crafts.Id,
                BusinessOwnerProfileId = craftsProfile.Id,
                Status            = ProductStatus.Approved,
                ApprovedBy        = "seed",
                ApprovedAt        = DateTime.UtcNow.AddDays(-45),
                IsVisible         = true,
                IsFeatured        = true,
                FeaturedOrder     = 3,
                ViewCount         = 1580,
                CartAddCount      = 390,
                PurchaseCount     = 243,
                AvgRating         = 4.6f,
                RevenueTotal      = 48597.57m,
                Images = new List<ProductImage>
                {
                    new() { ImageUrl = "https://images.unsplash.com/photo-1600585154340-be6161a56a0c?w=800", IsMain = true,  SortOrder = 0 },
                    new() { ImageUrl = "https://images.unsplash.com/photo-1618220179428-22790b461013?w=800", IsMain = false, SortOrder = 1 },
                    new() { ImageUrl = "https://images.unsplash.com/photo-1609948543911-abe7ebef6a1a?w=800", IsMain = false, SortOrder = 2 }
                }
            };

            var p8 = new Product
            {
                Name              = "Hand-Painted Clay Pot",
                Description       = "Traditional clay pot hand-painted with geometric patterns using natural dyes. Suitable for plants or decorative use. Each pot is unique. Height: 15cm, Diameter: 12cm. Sealed for durability.",
                Price             = 159.99m,
                StockQuantity     = 25,
                Tags              = "clay,pot,hand-painted,traditional,decor",
                CategoryId        = crafts.Id,
                BusinessOwnerProfileId = craftsProfile.Id,
                Status            = ProductStatus.Approved,
                ApprovedBy        = "seed",
                ApprovedAt        = DateTime.UtcNow.AddDays(-40),
                IsVisible         = true,
                IsFeatured        = false,
                ViewCount         = 710,
                CartAddCount      = 180,
                PurchaseCount     = 115,
                AvgRating         = 4.4f,
                RevenueTotal      = 18398.85m,
                Images = new List<ProductImage>
                {
                    new() { ImageUrl = "https://images.unsplash.com/photo-1565193566173-7a0ee3dbe261?w=800", IsMain = true,  SortOrder = 0 },
                    new() { ImageUrl = "https://images.unsplash.com/photo-1501004318641-b39e6451bec6?w=800", IsMain = false, SortOrder = 1 }
                }
            };

            var p9 = new Product
            {
                Name              = "Macrame Wall Hanging",
                Description       = "Boho-style macramé wall hanging made from natural cotton yarn. Features intricate knotting patterns with wooden dowel. Size: 40cm x 60cm. Handmade with love by local artisans.",
                Price             = 349.99m,
                StockQuantity     = 15,
                Tags              = "macrame,wall-hanging,boho,yarn,handmade",
                CategoryId        = crafts.Id,
                BusinessOwnerProfileId = craftsProfile.Id,
                Status            = ProductStatus.Approved,
                ApprovedBy        = "seed",
                ApprovedAt        = DateTime.UtcNow.AddDays(-35),
                IsVisible         = true,
                IsFeatured        = true,
                FeaturedOrder     = 5,
                ViewCount         = 940,
                CartAddCount      = 210,
                PurchaseCount     = 98,
                AvgRating         = 4.7f,
                RevenueTotal      = 34299.02m,
                Images = new List<ProductImage>
                {
                    new() { ImageUrl = "https://images.unsplash.com/photo-1614627585-700dec5e8900?w=800", IsMain = true,  SortOrder = 0 },
                    new() { ImageUrl = "https://images.unsplash.com/photo-1602513079900-8c5e4b0a1b3c?w=800", IsMain = false, SortOrder = 1 }
                }
            };

            var p10 = new Product
            {
                Name              = "Scented Soy Candle Set",
                Description       = "Set of 3 hand-poured soy wax candles in amber glass jars. Scents: Oud & Sandalwood, Jasmine Garden, Citrus Burst. Each candle burns 40+ hours. Cotton wicks, clean burn, no soot.",
                Price             = 249.99m,
                StockQuantity     = 50,
                Tags              = "candle,soy,scented,handmade,gift",
                CategoryId        = crafts.Id,
                BusinessOwnerProfileId = craftsProfile.Id,
                Status            = ProductStatus.Approved,
                ApprovedBy        = "seed",
                ApprovedAt        = DateTime.UtcNow.AddDays(-50),
                IsVisible         = true,
                IsFeatured        = true,
                FeaturedOrder     = 6,
                ViewCount         = 1900,
                CartAddCount      = 520,
                PurchaseCount     = 334,
                AvgRating         = 4.8f,
                RevenueTotal      = 83496.66m,
                Images = new List<ProductImage>
                {
                    new() { ImageUrl = "https://images.unsplash.com/photo-1603006905003-be475563bc59?w=800", IsMain = true,  SortOrder = 0 },
                    new() { ImageUrl = "https://images.unsplash.com/photo-1602874801007-e534cf0428b1?w=800", IsMain = false, SortOrder = 1 },
                    new() { ImageUrl = "https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=800", IsMain = false, SortOrder = 2 }
                }
            };

            var p11 = new Product
            {
                Name              = "Wax Melt Gift Box",
                Description       = "Collection of 6 handmade scented wax melts in a decorative box. Scents: jasmine, rose, oud, vanilla, lavender, and citrus. Each melt provides 8–10 hours of fragrance. Paraben-free.",
                Price             = 120.00m,
                StockQuantity     = 50,
                Tags              = "wax,scented,gift,handmade,home",
                CategoryId        = crafts.Id,
                BusinessOwnerProfileId = craftsProfile.Id,
                Status            = ProductStatus.Approved,
                ApprovedBy        = "seed",
                ApprovedAt        = DateTime.UtcNow.AddDays(-25),
                IsVisible         = true,
                IsFeatured        = false,
                ViewCount         = 540,
                CartAddCount      = 150,
                PurchaseCount     = 110,
                AvgRating         = 4.2f,
                RevenueTotal      = 13200.00m,
                Images = new List<ProductImage>
                {
                    new() { ImageUrl = "https://images.unsplash.com/photo-1604382354936-07c5d9983bd3?w=800", IsMain = true,  SortOrder = 0 }
                }
            };

            // ─────────────────────────────────────────────────────────
            // NATURAL & BEAUTY — Salma Naturals (BeautyBoId)
            // ─────────────────────────────────────────────────────────
            var p12 = new Product
            {
                Name              = "Shea Butter Body Cream",
                Description       = "100% natural shea butter body cream enriched with essential oils of lavender and chamomile. Deeply moisturises dry skin, reduces stretch marks, and improves skin elasticity. 200ml jar. Vegan and cruelty-free.",
                Price             = 149.99m,
                StockQuantity     = 80,
                Tags              = "shea-butter,body-cream,natural,moisturizer,vegan",
                CategoryId        = beauty.Id,
                BusinessOwnerProfileId = beautyProfile.Id,
                Status            = ProductStatus.Approved,
                ApprovedBy        = "seed",
                ApprovedAt        = DateTime.UtcNow.AddDays(-60),
                IsVisible         = true,
                IsFeatured        = true,
                FeaturedOrder     = 7,
                ViewCount         = 2300,
                CartAddCount      = 680,
                PurchaseCount     = 445,
                AvgRating         = 4.8f,
                RevenueTotal      = 66745.55m,
                Images = new List<ProductImage>
                {
                    new() { ImageUrl = "https://images.unsplash.com/photo-1570194065650-d99fb4bedf0a?w=800", IsMain = true,  SortOrder = 0 },
                    new() { ImageUrl = "https://images.unsplash.com/photo-1598440947619-2c35fc9aa908?w=800", IsMain = false, SortOrder = 1 },
                    new() { ImageUrl = "https://images.unsplash.com/photo-1631390143985-a0c2ab5ff26d?w=800", IsMain = false, SortOrder = 2 }
                }
            };

            var p13 = new Product
            {
                Name              = "Rosehip & Argan Face Oil",
                Description       = "Luxurious face oil blending cold-pressed rosehip and argan oils with vitamin E. Brightens complexion, reduces fine lines, and deeply nourishes skin. 30ml dropper bottle. Suitable for all skin types.",
                Price             = 199.99m,
                StockQuantity     = 55,
                Tags              = "face-oil,rosehip,argan,natural,skincare",
                CategoryId        = beauty.Id,
                BusinessOwnerProfileId = beautyProfile.Id,
                Status            = ProductStatus.Approved,
                ApprovedBy        = "seed",
                ApprovedAt        = DateTime.UtcNow.AddDays(-55),
                IsVisible         = true,
                IsFeatured        = true,
                FeaturedOrder     = 8,
                ViewCount         = 1760,
                CartAddCount      = 430,
                PurchaseCount     = 298,
                AvgRating         = 4.9f,
                RevenueTotal      = 59597.02m,
                Images = new List<ProductImage>
                {
                    new() { ImageUrl = "https://images.unsplash.com/photo-1608248597279-f99d160bfcbc?w=800", IsMain = true,  SortOrder = 0 },
                    new() { ImageUrl = "https://images.unsplash.com/photo-1571781926291-c477ebfd024b?w=800", IsMain = false, SortOrder = 1 }
                }
            };

            var p14 = new Product
            {
                Name              = "Natural Soap Bar Collection",
                Description       = "Set of 4 handmade cold-process soap bars using natural ingredients: olive oil, coconut oil, and essential oils. Variants: Oud & Rose, Mint & Tea Tree, Honey & Oat, Lavender & Chamomile. Each bar 100g.",
                Price             = 129.99m,
                StockQuantity     = 100,
                Tags              = "soap,natural,handmade,cold-process,skincare",
                CategoryId        = beauty.Id,
                BusinessOwnerProfileId = beautyProfile.Id,
                Status            = ProductStatus.Approved,
                ApprovedBy        = "seed",
                ApprovedAt        = DateTime.UtcNow.AddDays(-50),
                IsVisible         = true,
                IsFeatured        = false,
                ViewCount         = 1100,
                CartAddCount      = 290,
                PurchaseCount     = 210,
                AvgRating         = 4.5f,
                RevenueTotal      = 27297.90m,
                Images = new List<ProductImage>
                {
                    new() { ImageUrl = "https://images.unsplash.com/photo-1583947215259-38e31be8751f?w=800", IsMain = true,  SortOrder = 0 },
                    new() { ImageUrl = "https://images.unsplash.com/photo-1600857544200-b2f666a9a2ec?w=800", IsMain = false, SortOrder = 1 },
                    new() { ImageUrl = "https://images.unsplash.com/photo-1621498901254-8dc09fef0e1c?w=800", IsMain = false, SortOrder = 2 }
                }
            };

            var p15 = new Product
            {
                Name              = "Essential Oil Blend Set",
                Description       = "Set of 5 pure essential oils in 10ml amber glass bottles. Includes: Egyptian Rose, Jasmine, Frankincense, Eucalyptus, and Peppermint. Perfect for aromatherapy, diffusers, and DIY beauty recipes.",
                Price             = 259.99m,
                StockQuantity     = 40,
                Tags              = "essential-oils,aromatherapy,natural,pure,beauty",
                CategoryId        = beauty.Id,
                BusinessOwnerProfileId = beautyProfile.Id,
                Status            = ProductStatus.Approved,
                ApprovedBy        = "seed",
                ApprovedAt        = DateTime.UtcNow.AddDays(-42),
                IsVisible         = true,
                IsFeatured        = false,
                ViewCount         = 830,
                CartAddCount      = 200,
                PurchaseCount     = 145,
                AvgRating         = 4.6f,
                RevenueTotal      = 37698.55m,
                Images = new List<ProductImage>
                {
                    new() { ImageUrl = "https://images.unsplash.com/photo-1612197527762-8cfb4503b3ee?w=800", IsMain = true,  SortOrder = 0 },
                    new() { ImageUrl = "https://images.unsplash.com/photo-1608571423902-eed4a5ad8108?w=800", IsMain = false, SortOrder = 1 }
                }
            };

            var p16 = new Product
            {
                Name              = "Glycerin Rose & Honey Soap",
                Description       = "Single 150g luxury glycerin soap bar infused with rose water and pure honey. Deeply cleanses while maintaining moisture balance. Gentle enough for sensitive skin. Handmade in small batches.",
                Price             = 89.99m,
                StockQuantity     = 80,
                Tags              = "glycerin,soap,rose,honey,natural",
                CategoryId        = beauty.Id,
                BusinessOwnerProfileId = beautyProfile.Id,
                Status            = ProductStatus.Approved,
                ApprovedBy        = "seed",
                ApprovedAt        = DateTime.UtcNow.AddDays(-38),
                IsVisible         = true,
                IsFeatured        = false,
                ViewCount         = 650,
                CartAddCount      = 175,
                PurchaseCount     = 130,
                AvgRating         = 4.4f,
                RevenueTotal      = 11698.70m,
                Images = new List<ProductImage>
                {
                    new() { ImageUrl = "https://images.unsplash.com/photo-1576426863848-c21f53c60b19?w=800", IsMain = true,  SortOrder = 0 }
                }
            };

            var products = new List<Product>
                { p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, p16 };

            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
        }
    }
}