// ============================================================
// Talentree.Repository/Data/DataSeed/ProductReviewSeed.cs
// ============================================================
// Seeds realistic product reviews written by the seeded customer
// accounts. Covers approved products from all three brands.
// Each product with AvgRating gets matching reviews whose average
// matches the seeded score.
// Must run AFTER CategoriesSeed (products must exist).
// ============================================================
using Talentree.Core.Entities;
using Talentree.Core.Entities.Identity;

namespace Talentree.Repository.Data.DataSeed
{
    public static class ProductReviewSeed
    {
        public static async Task SeedAsync(TalentreeDbContext context)
        {
            if (context.Set<ProductReview>().Any()) return;

            // ── Resolve customer user IDs ──────────────────────────────
            // These match the customers seeded in TalentreeContextSeed
            var mona    = context.Users.FirstOrDefault(u => u.Email == "mona.ahmed@example.com");
            var khaled  = context.Users.FirstOrDefault(u => u.Email == "khaled.mahmoud@example.com");
            var nour    = context.Users.FirstOrDefault(u => u.Email == "nour.hassan@example.com");
            var yasser  = context.Users.FirstOrDefault(u => u.Email == "yasser.ali@example.com");
            var laila   = context.Users.FirstOrDefault(u => u.Email == "laila.mohamed@example.com");

            if (mona == null || khaled == null) return; // customers not seeded yet

            // ── Resolve products ──────────────────────────────────────
            var products = context.Products
                .Where(p => p.Status == Core.Enums.ProductStatus.Approved)
                .ToList();

            if (!products.Any()) return;

            Product? Get(string name) => products.FirstOrDefault(p => p.Name == name);

            var reviews = new List<ProductReview>();

            // ─────────────────────────────────────────────────────────
            // FASHION PRODUCTS
            // ─────────────────────────────────────────────────────────

            var toteBag = Get("Handwoven Cotton Tote Bag");
            if (toteBag != null)
            {
                reviews.AddRange(new[]
                {
                    new ProductReview
                    {
                        ProductId = toteBag.Id, CustomerUserId = mona!.Id,
                        CustomerName = "Mona Ahmed", Rating = 5,
                        ReviewText = "Absolutely love this bag! The craftsmanship is incredible — the embroidery is so detailed and the cotton is really durable. Been using it for grocery shopping every week. Highly recommend.",
                        OwnerResponse = "Thank you so much Mona! Your kind words mean the world to us 💛",
                        ResponseAt = DateTime.UtcNow.AddDays(-8),
                        CreatedAt = DateTime.UtcNow.AddDays(-12)
                    },
                    new ProductReview
                    {
                        ProductId = toteBag.Id, CustomerUserId = khaled!.Id,
                        CustomerName = "Khaled Mahmoud", Rating = 5,
                        ReviewText = "Bought this as a gift for my wife and she absolutely loves it. The quality is top notch and the design is uniquely Egyptian. Arrived well packaged.",
                        CreatedAt = DateTime.UtcNow.AddDays(-18)
                    },
                    new ProductReview
                    {
                        ProductId = toteBag.Id, CustomerUserId = nour!.Id,
                        CustomerName = "Nour Hassan", Rating = 4,
                        ReviewText = "Really nice bag, good quality cotton. The handles are sturdy. Only giving 4 stars because delivery took a little longer than expected, but the product itself is great.",
                        CreatedAt = DateTime.UtcNow.AddDays(-22)
                    },
                    new ProductReview
                    {
                        ProductId = toteBag.Id, CustomerUserId = laila!.Id,
                        CustomerName = "Laila Mohamed", Rating = 5,
                        ReviewText = "بايعة بصراحة! الشنطة جميلة جداً والخامة ممتازة. الصنعة اليدوية واضحة بشكل رائع. هشتريها تاني.",
                        CreatedAt = DateTime.UtcNow.AddDays(-28)
                    }
                });
            }

            var bracelet = Get("Leather Strap Bracelet Set");
            if (bracelet != null)
            {
                reviews.AddRange(new[]
                {
                    new ProductReview
                    {
                        ProductId = bracelet.Id, CustomerUserId = yasser!.Id,
                        CustomerName = "Yasser Ali", Rating = 5,
                        ReviewText = "The leather quality is exceptional. All three bracelets look amazing together. Great gift idea — my girlfriend was thrilled.",
                        CreatedAt = DateTime.UtcNow.AddDays(-10)
                    },
                    new ProductReview
                    {
                        ProductId = bracelet.Id, CustomerUserId = mona!.Id,
                        CustomerName = "Mona Ahmed", Rating = 4,
                        ReviewText = "Lovely set, the leather is genuine and smells great. The brass buckles are well finished. Would have loved a small carrying pouch to be included.",
                        OwnerResponse = "Thank you for the feedback! We'll look into adding a gift pouch. Glad you liked the leather quality!",
                        ResponseAt = DateTime.UtcNow.AddDays(-5),
                        CreatedAt = DateTime.UtcNow.AddDays(-15)
                    }
                });
            }

            var kimono = Get("Hand-Painted Silk Kimono");
            if (kimono != null)
            {
                reviews.AddRange(new[]
                {
                    new ProductReview
                    {
                        ProductId = kimono.Id, CustomerUserId = laila!.Id,
                        CustomerName = "Laila Mohamed", Rating = 5,
                        ReviewText = "Absolutely STUNNING piece. The painting is so intricate, you can tell so much care and skill went into it. It's literally wearable art. I wear it over my outfits and get so many compliments.",
                        OwnerResponse = "You've made our day! This piece took 3 days to paint ❤️",
                        ResponseAt = DateTime.UtcNow.AddDays(-3),
                        CreatedAt = DateTime.UtcNow.AddDays(-7)
                    },
                    new ProductReview
                    {
                        ProductId = kimono.Id, CustomerUserId = nour!.Id,
                        CustomerName = "Nour Hassan", Rating = 5,
                        ReviewText = "The silk is incredibly soft and the colours are vibrant. Each piece is truly one-of-a-kind. Worth every penny.",
                        CreatedAt = DateTime.UtcNow.AddDays(-14)
                    }
                });
            }

            // ─────────────────────────────────────────────────────────
            // CRAFTS PRODUCTS
            // ─────────────────────────────────────────────────────────

            var coasters = Get("Resin Art Coaster Set");
            if (coasters != null)
            {
                reviews.AddRange(new[]
                {
                    new ProductReview
                    {
                        ProductId = coasters.Id, CustomerUserId = mona!.Id,
                        CustomerName = "Mona Ahmed", Rating = 5,
                        ReviewText = "These coasters are absolutely gorgeous! The dried flowers inside the resin look so delicate and beautiful. Great quality — not a single bubble in the resin. Gift box was also lovely.",
                        OwnerResponse = "So happy you love them Mona! Each coaster takes about 3 hours to make 😊",
                        ResponseAt = DateTime.UtcNow.AddDays(-15),
                        CreatedAt = DateTime.UtcNow.AddDays(-20)
                    },
                    new ProductReview
                    {
                        ProductId = coasters.Id, CustomerUserId = khaled!.Id,
                        CustomerName = "Khaled Mahmoud", Rating = 5,
                        ReviewText = "Bought as a wedding gift. The couple loved them! Very well-made and heat-resistant as described. The colours are vibrant and each piece is unique.",
                        CreatedAt = DateTime.UtcNow.AddDays(-30)
                    },
                    new ProductReview
                    {
                        ProductId = coasters.Id, CustomerUserId = yasser!.Id,
                        CustomerName = "Yasser Ali", Rating = 4,
                        ReviewText = "Really nice coasters, great quality. The only issue was one of the four had a tiny air bubble but it's barely visible. Still love the product overall.",
                        CreatedAt = DateTime.UtcNow.AddDays(-42)
                    }
                });
            }

            var candles = Get("Scented Soy Candle Set");
            if (candles != null)
            {
                reviews.AddRange(new[]
                {
                    new ProductReview
                    {
                        ProductId = candles.Id, CustomerUserId = laila!.Id,
                        CustomerName = "Laila Mohamed", Rating = 5,
                        ReviewText = "The Oud & Sandalwood scent is heavenly! Burns clean with no black smoke. Been burning it every evening and it fills the entire room. Will definitely reorder.",
                        CreatedAt = DateTime.UtcNow.AddDays(-5)
                    },
                    new ProductReview
                    {
                        ProductId = candles.Id, CustomerUserId = nour!.Id,
                        CustomerName = "Nour Hassan", Rating = 5,
                        ReviewText = "All three scents are amazing but the Jasmine Garden is my favourite. The glass jars are also beautiful — keeping them as décor after the wax runs out.",
                        OwnerResponse = "We love hearing that! The jasmine is our bestseller for a reason 🌸",
                        ResponseAt = DateTime.UtcNow.AddDays(-12),
                        CreatedAt = DateTime.UtcNow.AddDays(-18)
                    },
                    new ProductReview
                    {
                        ProductId = candles.Id, CustomerUserId = mona!.Id,
                        CustomerName = "Mona Ahmed", Rating = 5,
                        ReviewText = "Perfect gift! I bought this for Eid and everyone loved it. The packaging is beautiful and the candles smell incredible.",
                        CreatedAt = DateTime.UtcNow.AddDays(-50)
                    },
                    new ProductReview
                    {
                        ProductId = candles.Id, CustomerUserId = khaled!.Id,
                        CustomerName = "Khaled Mahmoud", Rating = 4,
                        ReviewText = "Really nice candles. Good burn time. The only thing — I expected the scent to be stronger but it's quite subtle. Might buy again.",
                        CreatedAt = DateTime.UtcNow.AddDays(-38)
                    }
                });
            }

            var macrame = Get("Macrame Wall Hanging");
            if (macrame != null)
            {
                reviews.AddRange(new[]
                {
                    new ProductReview
                    {
                        ProductId = macrame.Id, CustomerUserId = khaled!.Id,
                        CustomerName = "Khaled Mahmoud", Rating = 5,
                        ReviewText = "My wife has been wanting a macramé wall hanging for months. This one is perfect — the knotting is tight and even, the cotton is soft and natural looking. Looks amazing above our bed.",
                        CreatedAt = DateTime.UtcNow.AddDays(-8)
                    },
                    new ProductReview
                    {
                        ProductId = macrame.Id, CustomerUserId = laila!.Id,
                        CustomerName = "Laila Mohamed", Rating = 5,
                        ReviewText = "Stunning piece! Took it to an interior designer friend and she couldn't believe it was locally made. The craftsmanship is that good.",
                        CreatedAt = DateTime.UtcNow.AddDays(-25)
                    }
                });
            }

            // ─────────────────────────────────────────────────────────
            // BEAUTY PRODUCTS
            // ─────────────────────────────────────────────────────────

            var sheaButter = Get("Shea Butter Body Cream");
            if (sheaButter != null)
            {
                reviews.AddRange(new[]
                {
                    new ProductReview
                    {
                        ProductId = sheaButter.Id, CustomerUserId = mona!.Id,
                        CustomerName = "Mona Ahmed", Rating = 5,
                        ReviewText = "This cream is a miracle worker for my dry skin! It absorbs quickly without leaving a greasy feeling. The lavender scent is calming and not overpowering. 100% natural as described.",
                        OwnerResponse = "Thank you beautiful! Your skin deserves the best 🌿",
                        ResponseAt = DateTime.UtcNow.AddDays(-20),
                        CreatedAt = DateTime.UtcNow.AddDays(-28)
                    },
                    new ProductReview
                    {
                        ProductId = sheaButter.Id, CustomerUserId = laila!.Id,
                        CustomerName = "Laila Mohamed", Rating = 5,
                        ReviewText = "I've tried so many body creams and this is hands down the best. My skin has never felt softer. The 200ml jar lasts a long time too. Reordering immediately.",
                        CreatedAt = DateTime.UtcNow.AddDays(-45)
                    },
                    new ProductReview
                    {
                        ProductId = sheaButter.Id, CustomerUserId = nour!.Id,
                        CustomerName = "Nour Hassan", Rating = 5,
                        ReviewText = "Love that it's vegan and cruelty-free. Works beautifully on my elbows and knees which are always dry. Recommend to everyone!",
                        CreatedAt = DateTime.UtcNow.AddDays(-55)
                    },
                    new ProductReview
                    {
                        ProductId = sheaButter.Id, CustomerUserId = yasser!.Id,
                        CustomerName = "Yasser Ali", Rating = 4,
                        ReviewText = "Bought for my mother and she's been using it happily. Good quality, natural ingredients. The only downside is the jar lid could be sealed better during shipping.",
                        CreatedAt = DateTime.UtcNow.AddDays(-35)
                    }
                });
            }

            var faceOil = Get("Rosehip & Argan Face Oil");
            if (faceOil != null)
            {
                reviews.AddRange(new[]
                {
                    new ProductReview
                    {
                        ProductId = faceOil.Id, CustomerUserId = laila!.Id,
                        CustomerName = "Laila Mohamed", Rating = 5,
                        ReviewText = "My skin has literally transformed in 3 weeks of use. Fine lines are visibly reduced and my complexion is glowing. I use 3 drops every night before bed. Worth every EGP.",
                        OwnerResponse = "We're so thrilled to hear that! Consistency is key and you're nailing it 🌹",
                        ResponseAt = DateTime.UtcNow.AddDays(-10),
                        CreatedAt = DateTime.UtcNow.AddDays(-22)
                    },
                    new ProductReview
                    {
                        ProductId = faceOil.Id, CustomerUserId = mona!.Id,
                        CustomerName = "Mona Ahmed", Rating = 5,
                        ReviewText = "The dropper makes it so easy to apply just the right amount. My skin absorbs it fast, no sticky feeling. Smells beautiful and natural. Already on my second bottle.",
                        CreatedAt = DateTime.UtcNow.AddDays(-40)
                    },
                    new ProductReview
                    {
                        ProductId = faceOil.Id, CustomerUserId = nour!.Id,
                        CustomerName = "Nour Hassan", Rating = 5,
                        ReviewText = "Best face oil I've ever used. Cold-pressed quality is obvious — the oil is clear and pure. My combination skin loves it.",
                        CreatedAt = DateTime.UtcNow.AddDays(-30)
                    }
                });
            }

            var soaps = Get("Natural Soap Bar Collection");
            if (soaps != null)
            {
                reviews.AddRange(new[]
                {
                    new ProductReview
                    {
                        ProductId = soaps.Id, CustomerUserId = khaled!.Id,
                        CustomerName = "Khaled Mahmoud", Rating = 5,
                        ReviewText = "The Oud & Rose bar has a gorgeous scent that lingers after washing. All four bars lather well and leave skin feeling clean without that tight, dry feeling. Great value for a set.",
                        CreatedAt = DateTime.UtcNow.AddDays(-15)
                    },
                    new ProductReview
                    {
                        ProductId = soaps.Id, CustomerUserId = yasser!.Id,
                        CustomerName = "Yasser Ali", Rating = 4,
                        ReviewText = "Nice soap bars, good ingredients. The Mint & Tea Tree one is refreshing. Giving 4 stars because one bar was slightly smaller than the others, but overall great quality.",
                        CreatedAt = DateTime.UtcNow.AddDays(-25)
                    },
                    new ProductReview
                    {
                        ProductId = soaps.Id, CustomerUserId = mona!.Id,
                        CustomerName = "Mona Ahmed", Rating = 5,
                        ReviewText = "I've switched from commercial soap to these and my skin is noticeably less irritated. The Honey & Oat bar is my favourite — so gentle.",
                        CreatedAt = DateTime.UtcNow.AddDays(-48)
                    }
                });
            }

            var essentialOils = Get("Essential Oil Blend Set");
            if (essentialOils != null)
            {
                reviews.AddRange(new[]
                {
                    new ProductReview
                    {
                        ProductId = essentialOils.Id, CustomerUserId = laila!.Id,
                        CustomerName = "Laila Mohamed", Rating = 5,
                        ReviewText = "The Egyptian Rose oil smells exactly like fresh roses — so authentic. I use the Frankincense in my diffuser every night. Pure quality, no fillers.",
                        CreatedAt = DateTime.UtcNow.AddDays(-12)
                    },
                    new ProductReview
                    {
                        ProductId = essentialOils.Id, CustomerUserId = nour!.Id,
                        CustomerName = "Nour Hassan", Rating = 4,
                        ReviewText = "Good quality oils, well packaged. The amber glass bottles are a nice touch. The peppermint oil is very strong — a little goes a long way!",
                        CreatedAt = DateTime.UtcNow.AddDays(-38)
                    }
                });
            }

            context.Set<ProductReview>().AddRange(reviews);
            await context.SaveChangesAsync();
        }
    }
}
