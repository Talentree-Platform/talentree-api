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
                new() { Name = "Fashion & Accessories",      BusinessType = "Fashion" },
                new() { Name = "Handmade & Crafts",          BusinessType = "Crafts" },
                new() { Name = "Natural & Beauty Products",  BusinessType = "Beauty" }
                };

                await context.Categories.AddRangeAsync(categories);
                await context.SaveChangesAsync();
            }

            // ═══════════════════════════════════════════════════════════
            // SEED PRODUCTS (only if business owner profiles exist)
            // ═══════════════════════════════════════════════════════════
            if (context.Products.Any()) return;

            var fashionProfile = context.BusinessOwnerProfiles
                .FirstOrDefault(b => b.BusinessName == "Tech Galaxy"); // reusing seeded BO

            var craftsProfile = context.BusinessOwnerProfiles
                .FirstOrDefault(b => b.BusinessName == "Fashion House");

            var beautyProfile = context.BusinessOwnerProfiles
                .FirstOrDefault(b => b.BusinessName == "Beauty Corner");

            if (fashionProfile == null) return;

            var fashion = context.Categories.First(c => c.Name == "Fashion & Accessories");
            var crafts = context.Categories.First(c => c.Name == "Handmade & Crafts");
            var beauty = context.Categories.First(c => c.Name == "Natural & Beauty Products");

            var products = new List<Product>();

            // ─────────────────────────────────────────────────────────
            // Fashion & Accessories Products
            // ─────────────────────────────────────────────────────────
            if (fashionProfile != null)
            {
                products.AddRange(new[]
                {
        new Product
        {
            Name = "Handwoven Cotton Tote Bag",
            Description = "Eco-friendly tote bag handwoven from 100% Egyptian cotton. Features double handles, inner pocket, and traditional embroidery details. Perfect for daily use and shopping. Durable and washable.",
            Price = 189.99m,
            StockQuantity = 45,
            Tags = "tote,cotton,handwoven,eco-friendly,bag",
            CategoryId = fashion.Id,
            BusinessOwnerProfileId = fashionProfile.Id,
            Status = ProductStatus.Approved,
            ApprovedBy = "admin@talentree.com",
            ApprovedAt = DateTime.UtcNow.AddDays(-5)
        },
        new Product
        {
            Name = "Leather Strap Bracelet Set",
            Description = "Set of 3 genuine leather strap bracelets with brass buckle closures. Handcrafted by local artisans using premium leather. Available in brown, black, and tan. Adjustable fit.",
            Price = 149.99m,
            StockQuantity = 60,
            Tags = "bracelet,leather,handcrafted,accessories,jewelry",
            CategoryId = fashion.Id,
            BusinessOwnerProfileId = fashionProfile.Id,
            Status = ProductStatus.Approved,
            ApprovedBy = "admin@talentree.com",
            ApprovedAt = DateTime.UtcNow.AddDays(-4)
        },
        new Product
        {
            Name = "Embroidered Linen Scarf",
            Description = "Lightweight linen scarf with hand-embroidered floral patterns. Inspired by traditional Egyptian designs. Suitable for all seasons. Dimensions: 180cm x 50cm. Hand wash only.",
            Price = 229.99m,
            StockQuantity = 30,
            Tags = "scarf,linen,embroidered,traditional,fashion",
            CategoryId = fashion.Id,
            BusinessOwnerProfileId = fashionProfile.Id,
            Status = ProductStatus.PendingApproval,
        },
        new Product
        {
            Name = "Denim Crossbody Bag",
            Description = "Stylish crossbody bag made from upcycled denim with leather trim. Features adjustable strap, zipper closure, and 2 inner compartments. Unique one-of-a-kind piece.",
            Price = 275.00m,
            StockQuantity = 20,
            Tags = "crossbody,denim,bag,upcycled,fashion",
            CategoryId = fashion.Id,
            BusinessOwnerProfileId = fashionProfile.Id,
            Status = ProductStatus.Rejected,
            RejectionReason = "Images are blurry. Please resubmit with clearer product photos."
        }
    });
            }

            // ─────────────────────────────────────────────────────────
            // Handmade & Crafts Products
            // ─────────────────────────────────────────────────────────
            if (craftsProfile != null)
            {
                products.AddRange(new[]
                {
        new Product
        {
            Name = "Resin Art Coaster Set",
            Description = "Set of 4 handmade resin coasters with dried flower inclusions. Each piece is unique with swirling color patterns. Heat resistant up to 80°C. Comes in a gift box. Perfect for home decor.",
            Price = 199.99m,
            StockQuantity = 35,
            Tags = "resin,coasters,handmade,home-decor,gift",
            CategoryId = crafts.Id,
            BusinessOwnerProfileId = craftsProfile.Id,
            Status = ProductStatus.Approved,
            ApprovedBy = "admin@talentree.com",
            ApprovedAt = DateTime.UtcNow.AddDays(-3)
        },
        new Product
        {
            Name = "Hand-Painted Clay Pot",
            Description = "Traditional clay pot hand-painted with geometric patterns using natural dyes. Suitable for plants or decorative use. Each pot is unique. Height: 15cm, Diameter: 12cm. Sealed for durability.",
            Price = 159.99m,
            StockQuantity = 25,
            Tags = "clay,pot,hand-painted,traditional,decor",
            CategoryId = crafts.Id,
            BusinessOwnerProfileId = craftsProfile.Id,
            Status = ProductStatus.Approved,
            ApprovedBy = "admin@talentree.com",
            ApprovedAt = DateTime.UtcNow.AddDays(-2)
        },
        new Product
        {
            Name = "Macrame Wall Hanging",
            Description = "Boho-style macrame wall hanging made from natural cotton yarn. Features intricate knotting patterns with wooden dowel. Size: 40cm x 60cm. Handmade with love by local artisans.",
            Price = 349.99m,
            StockQuantity = 15,
            Tags = "macrame,wall-hanging,boho,yarn,handmade",
            CategoryId = crafts.Id,
            BusinessOwnerProfileId = craftsProfile.Id,
            Status = ProductStatus.PendingApproval,
        },
        new Product
        {
            Name = "Wax Melt Gift Box",
            Description = "Collection of 6 handmade scented wax melts in a decorative box. Scents include jasmine, rose, oud, vanilla, lavender, and citrus. Each melt provides 8-10 hours of fragrance. Paraben-free.",
            Price = 120.00m,
            StockQuantity = 50,
            Tags = "wax,scented,gift,handmade,home",
            CategoryId = crafts.Id,
            BusinessOwnerProfileId = craftsProfile.Id,
            Status = ProductStatus.Draft,
        }
    });
            }

            // ─────────────────────────────────────────────────────────
            // Natural & Beauty Products
            // ─────────────────────────────────────────────────────────
            if (beautyProfile != null)
            {
                products.AddRange(new[]
                {
        new Product
        {
            Name = "Shea Butter Body Cream",
            Description = "100% natural shea butter body cream enriched with essential oils of lavender and chamomile. Deeply moisturizes dry skin, reduces stretch marks, and improves skin elasticity. 200ml jar. Vegan and cruelty-free.",
            Price = 149.99m,
            StockQuantity = 80,
            Tags = "shea-butter,body-cream,natural,moisturizer,vegan",
            CategoryId = beauty.Id,
            BusinessOwnerProfileId = beautyProfile.Id,
            Status = ProductStatus.Approved,
            ApprovedBy = "admin@talentree.com",
            ApprovedAt = DateTime.UtcNow.AddDays(-1)
        },
        new Product
        {
            Name = "Rosehip & Argan Face Oil",
            Description = "Luxurious face oil blending cold-pressed rosehip and argan oils with vitamin E. Brightens complexion, reduces fine lines, and deeply nourishes skin. 30ml dropper bottle. Suitable for all skin types.",
            Price = 199.99m,
            StockQuantity = 55,
            Tags = "face-oil,rosehip,argan,natural,skincare",
            CategoryId = beauty.Id,
            BusinessOwnerProfileId = beautyProfile.Id,
            Status = ProductStatus.Approved,
            ApprovedBy = "admin@talentree.com",
            ApprovedAt = DateTime.UtcNow.AddDays(-1)
        },
        new Product
        {
            Name = "Natural Soap Bar Collection",
            Description = "Set of 4 handmade cold-process soap bars using natural ingredients: olive oil, coconut oil, and essential oils. Variants: Oud & Rose, Mint & Tea Tree, Honey & Oat, Lavender & Chamomile. Each bar 100g.",
            Price = 129.99m,
            StockQuantity = 100,
            Tags = "soap,natural,handmade,cold-process,skincare",
            CategoryId = beauty.Id,
            BusinessOwnerProfileId = beautyProfile.Id,
            Status = ProductStatus.PendingApproval,
        },
        new Product
        {
            Name = "Essential Oil Blend Set",
            Description = "Set of 5 pure essential oils in 10ml amber glass bottles. Includes: Egyptian Rose, Jasmine, Frankincense, Eucalyptus, and Peppermint. Perfect for aromatherapy, diffusers, and DIY beauty recipes.",
            Price = 259.99m,
            StockQuantity = 40,
            Tags = "essential-oils,aromatherapy,natural,pure,beauty",
            CategoryId = beauty.Id,
            BusinessOwnerProfileId = beautyProfile.Id,
            Status = ProductStatus.Draft,
        }
    });
            }

            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
        }
    }
}