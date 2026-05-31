// ============================================================
// Talentree.Repository/Data/DataSeed/CustomerFeaturesSeed.cs
// ============================================================
// Seeds realistic, rich customer active carts, wishlists, orders,
// order items, and order status histories for frontend testing.
// Must run AFTER CategoriesSeed (products must exist).
// ============================================================
using Microsoft.EntityFrameworkCore;
using Talentree.Core.Entities;
using Talentree.Core.Entities.Identity;
using Talentree.Core.Enums;

namespace Talentree.Repository.Data.DataSeed
{
    public static class CustomerFeaturesSeed
    {
        public static async Task SeedAsync(TalentreeDbContext context)
        {
            // ── Resolve customer user IDs ──────────────────────────────
            var mona    = await context.Users.FirstOrDefaultAsync(u => u.Email == "mona.ahmed@example.com");
            var khaled  = await context.Users.FirstOrDefaultAsync(u => u.Email == "khaled.mahmoud@example.com");
            var nour    = await context.Users.FirstOrDefaultAsync(u => u.Email == "nour.hassan@example.com");
            var yasser  = await context.Users.FirstOrDefaultAsync(u => u.Email == "yasser.ali@example.com");
            var laila   = await context.Users.FirstOrDefaultAsync(u => u.Email == "laila.mohamed@example.com");

            if (mona == null || khaled == null || nour == null || yasser == null || laila == null)
            {
                return; // customers not seeded in identity yet
            }

            // ── Resolve products ──────────────────────────────────────
            var products = await context.Products
                .Include(p => p.BusinessOwner)
                .Include(p => p.Images)
                .Where(p => p.Status == ProductStatus.Approved)
                .ToListAsync();

            if (!products.Any()) return;

            Product? Get(string name) => products.FirstOrDefault(p => p.Name == name);

            var toteBag = Get("Handwoven Cotton Tote Bag");
            var bracelet = Get("Leather Strap Bracelet Set");
            var scarf = Get("Embroidered Linen Scarf");
            var coasters = Get("Resin Art Coaster Set");
            var candles = Get("Scented Soy Candle Set");
            var sheaButter = Get("Shea Butter Body Cream");
            var faceOil = Get("Rosehip & Argan Face Oil");
            var soaps = Get("Natural Soap Bar Collection");
            var kimono = Get("Hand-Painted Silk Kimono");

            // ============================================================
            // 1. SEED CUSTOMER CARTS
            // ============================================================
            if (!await context.Set<CustomerCart>().AnyAsync())
            {
                // Mona's Cart: Handwoven Cotton Tote Bag (x1), Shea Butter Body Cream (x2)
                if (toteBag != null && sheaButter != null)
                {
                    var monaCart = new CustomerCart
                    {
                        CustomerId = mona.Id,
                        CreatedAt = DateTime.UtcNow.AddHours(-2),
                        CreatedBy = mona.Id,
                        Items = new List<CustomerCartItem>
                        {
                            new() { ProductId = toteBag.Id, Quantity = 1, AddedAt = DateTime.UtcNow.AddHours(-2) },
                            new() { ProductId = sheaButter.Id, Quantity = 2, AddedAt = DateTime.UtcNow.AddHours(-1.5) }
                        }
                    };
                    await context.Set<CustomerCart>().AddAsync(monaCart);
                }

                // Khaled's Cart: Natural Soap Bar Collection (x1)
                if (soaps != null)
                {
                    var khaledCart = new CustomerCart
                    {
                        CustomerId = khaled.Id,
                        CreatedAt = DateTime.UtcNow.AddHours(-4),
                        CreatedBy = khaled.Id,
                        Items = new List<CustomerCartItem>
                        {
                            new() { ProductId = soaps.Id, Quantity = 1, AddedAt = DateTime.UtcNow.AddHours(-4) }
                        }
                    };
                    await context.Set<CustomerCart>().AddAsync(khaledCart);
                }
            }

            // ============================================================
            // 2. SEED CUSTOMER WISHLISTS
            // ============================================================
            if (!await context.Set<CustomerWishlist>().AnyAsync())
            {
                // Mona's Wishlist: Embroidered Linen Scarf, Resin Art Coaster Set
                if (scarf != null && coasters != null)
                {
                    var monaWishlist = new CustomerWishlist
                    {
                        CustomerId = mona.Id,
                        CreatedAt = DateTime.UtcNow.AddDays(-3),
                        CreatedBy = mona.Id,
                        Items = new List<CustomerWishlistItem>
                        {
                            new() { ProductId = scarf.Id, AddedAt = DateTime.UtcNow.AddDays(-3) },
                            new() { ProductId = coasters.Id, AddedAt = DateTime.UtcNow.AddDays(-2) }
                        }
                    };
                    await context.Set<CustomerWishlist>().AddAsync(monaWishlist);
                }

                // Khaled's Wishlist: Scented Soy Candle Set
                if (candles != null)
                {
                    var khaledWishlist = new CustomerWishlist
                    {
                        CustomerId = khaled.Id,
                        CreatedAt = DateTime.UtcNow.AddDays(-1),
                        CreatedBy = khaled.Id,
                        Items = new List<CustomerWishlistItem>
                        {
                            new() { ProductId = candles.Id, AddedAt = DateTime.UtcNow.AddDays(-1) }
                        }
                    };
                    await context.Set<CustomerWishlist>().AddAsync(khaledWishlist);
                }
            }

            // ============================================================
            // 3. SEED CUSTOMER ORDERS & ORDER ITEMS & STATUS HISTORY
            // ============================================================
            if (!await context.Set<CustomerOrder>().AnyAsync())
            {
                // ORDER 1 (Yasser Ali) ➔ Delivered, Paid via CreditCard
                if (toteBag != null && bracelet != null)
                {
                    var subtotal = (toteBag.Price * 1) + (bracelet.Price * 2);
                    var shipping = 50.00m;
                    var total = subtotal + shipping;

                    var yasserOrder = new CustomerOrder
                    {
                        CustomerId = yasser.Id,
                        FullName = "Yasser Ali",
                        PhoneNumber = "01234567444",
                        Street = "12 El-Galaa Street",
                        City = "Cairo",
                        PostalCode = "11511",
                        Country = "Egypt",
                        SubtotalAmount = subtotal,
                        ShippingAmount = shipping,
                        TotalAmount = total,
                        Status = CustomerOrderStatus.Delivered,
                        PaymentStatus = PaymentStatus.Paid,
                        PaymentMethod = PaymentMethod.CreditCard,
                        StripePaymentIntentId = "pi_mock_yasser_1001",
                        TrackingNumber = "TLT-87654321",
                        EstimatedDeliveryDate = DateTime.UtcNow.AddDays(-2),
                        ActualDeliveryDate = DateTime.UtcNow.AddDays(-2),
                        CreatedAt = DateTime.UtcNow.AddDays(-5),
                        CreatedBy = yasser.Id,
                        Items = new List<CustomerOrderItem>
                        {
                            new()
                            {
                                ProductId = toteBag.Id,
                                ProductName = toteBag.Name,
                                ProductImageUrl = toteBag.Images.FirstOrDefault(i => i.IsMain)?.ImageUrl ?? toteBag.Images.FirstOrDefault()?.ImageUrl,
                                SellerName = toteBag.BusinessOwner?.BusinessName ?? "Nour Couture",
                                UnitPrice = toteBag.Price,
                                Quantity = 1
                            },
                            new()
                            {
                                ProductId = bracelet.Id,
                                ProductName = bracelet.Name,
                                ProductImageUrl = bracelet.Images.FirstOrDefault(i => i.IsMain)?.ImageUrl ?? bracelet.Images.FirstOrDefault()?.ImageUrl,
                                SellerName = bracelet.BusinessOwner?.BusinessName ?? "Nour Couture",
                                UnitPrice = bracelet.Price,
                                Quantity = 2
                            }
                        },
                        StatusHistory = new List<OrderStatusHistory>
                        {
                            new() { Status = CustomerOrderStatus.Pending, ChangedAt = DateTime.UtcNow.AddDays(-5), ChangedBy = "system", Notes = "Order submitted successfully." },
                            new() { Status = CustomerOrderStatus.Processing, ChangedAt = DateTime.UtcNow.AddDays(-4), ChangedBy = "system", Notes = "Payment confirmed by Stripe." },
                            new() { Status = CustomerOrderStatus.Shipped, ChangedAt = DateTime.UtcNow.AddDays(-3), ChangedBy = "system", Notes = "Order shipped via Aramex courier." },
                            new() { Status = CustomerOrderStatus.Delivered, ChangedAt = DateTime.UtcNow.AddDays(-2), ChangedBy = "system", Notes = "Package successfully delivered to customer." }
                        }
                    };
                    await context.Set<CustomerOrder>().AddAsync(yasserOrder);
                }

                // ORDER 2 (Laila Mohamed) ➔ Processing, Paid via CreditCard
                if (kimono != null)
                {
                    var subtotal = kimono.Price * 1;
                    var shipping = 40.00m;
                    var total = subtotal + shipping;

                    var lailaOrder = new CustomerOrder
                    {
                        CustomerId = laila.Id,
                        FullName = "Laila Mohamed",
                        PhoneNumber = "01234567555",
                        Street = "45 Al-Horreya Avenue",
                        City = "Alexandria",
                        PostalCode = "21500",
                        Country = "Egypt",
                        SubtotalAmount = subtotal,
                        ShippingAmount = shipping,
                        TotalAmount = total,
                        Status = CustomerOrderStatus.Processing,
                        PaymentStatus = PaymentStatus.Paid,
                        PaymentMethod = PaymentMethod.CreditCard,
                        StripePaymentIntentId = "pi_mock_laila_2002",
                        CreatedAt = DateTime.UtcNow.AddDays(-1),
                        CreatedBy = laila.Id,
                        Items = new List<CustomerOrderItem>
                        {
                            new()
                            {
                                ProductId = kimono.Id,
                                ProductName = kimono.Name,
                                ProductImageUrl = kimono.Images.FirstOrDefault(i => i.IsMain)?.ImageUrl ?? kimono.Images.FirstOrDefault()?.ImageUrl,
                                SellerName = kimono.BusinessOwner?.BusinessName ?? "Nour Couture",
                                UnitPrice = kimono.Price,
                                Quantity = 1
                            }
                        },
                        StatusHistory = new List<OrderStatusHistory>
                        {
                            new() { Status = CustomerOrderStatus.Pending, ChangedAt = DateTime.UtcNow.AddDays(-1), ChangedBy = "system", Notes = "Order submitted successfully." },
                            new() { Status = CustomerOrderStatus.Processing, ChangedAt = DateTime.UtcNow.AddHours(-12), ChangedBy = "system", Notes = "Payment confirmed by Stripe." }
                        }
                    };
                    await context.Set<CustomerOrder>().AddAsync(lailaOrder);
                }

                // ORDER 3 (Nour Hassan) ➔ Shipped, Unpaid via CashOnDelivery
                if (candles != null)
                {
                    var subtotal = candles.Price * 1;
                    var shipping = 50.00m;
                    var total = subtotal + shipping;

                    var nourOrder = new CustomerOrder
                    {
                        CustomerId = nour.Id,
                        FullName = "Nour Hassan",
                        PhoneNumber = "01234567333",
                        Street = "88 Ramses Street",
                        City = "Cairo",
                        PostalCode = "11511",
                        Country = "Egypt",
                        SubtotalAmount = subtotal,
                        ShippingAmount = shipping,
                        TotalAmount = total,
                        Status = CustomerOrderStatus.Shipped,
                        PaymentStatus = PaymentStatus.Unpaid,
                        PaymentMethod = PaymentMethod.CashOnDelivery,
                        TrackingNumber = "TLT-98765432",
                        EstimatedDeliveryDate = DateTime.UtcNow.AddDays(2),
                        CreatedAt = DateTime.UtcNow.AddDays(-2),
                        CreatedBy = nour.Id,
                        Items = new List<CustomerOrderItem>
                        {
                            new()
                            {
                                ProductId = candles.Id,
                                ProductName = candles.Name,
                                ProductImageUrl = candles.Images.FirstOrDefault(i => i.IsMain)?.ImageUrl ?? candles.Images.FirstOrDefault()?.ImageUrl,
                                SellerName = candles.BusinessOwner?.BusinessName ?? "Karim Craft Studio",
                                UnitPrice = candles.Price,
                                Quantity = 1
                            }
                        },
                        StatusHistory = new List<OrderStatusHistory>
                        {
                            new() { Status = CustomerOrderStatus.Pending, ChangedAt = DateTime.UtcNow.AddDays(-2), ChangedBy = "system", Notes = "Order placed via Cash on Delivery." },
                            new() { Status = CustomerOrderStatus.Processing, ChangedAt = DateTime.UtcNow.AddDays(-1), ChangedBy = "system", Notes = "Order is being prepared by the artisan." },
                            new() { Status = CustomerOrderStatus.Shipped, ChangedAt = DateTime.UtcNow.AddHours(-6), ChangedBy = "system", Notes = "Order handed over to courier. Tracking number assigned." }
                        }
                    };
                    await context.Set<CustomerOrder>().AddAsync(nourOrder);
                }
            }

            await context.SaveChangesAsync();
        }
    }
}
