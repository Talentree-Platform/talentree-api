using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talentree.Core.Entities;

namespace Talentree.Repository.Data.DataSeed
{
    public static class RawMaterialSeed
    {
        public static async Task SeedAsync(TalentreeDbContext context)
        {
            // Only seed if no suppliers exist yet
            if (context.Set<Supplier>().Any()) return;

            // ── Suppliers ─────────────────────────────────────────

            var fabricSupplier = new Supplier
            {
                Name = "Cairo Fabric House",
                Description = "Premium fabric wholesaler specializing in cotton, linen and denim",
                Email = "orders@cairofabric.eg",
                Phone = "+20-2-1234-5678",
                Address = "15 Al-Gomhoreya Street",
                City = "Cairo",
                Country = "Egypt",
                ContactPerson = "Ahmed Hassan",
                IsActive = true,
                CreatedBy = "seed"
            };

            var craftSupplier = new Supplier
            {
                Name = "ArtCraft Supplies Egypt",
                Description = "Wholesale art and craft materials including resin, wax, and clay",
                Email = "sales@artcrafteg.com",
                Phone = "+20-2-8765-4321",
                Address = "42 Al-Ahrar Street",
                City = "Alexandria",
                Country = "Egypt",
                ContactPerson = "Layla Ibrahim",
                IsActive = true,
                CreatedBy = "seed"
            };

            var beautySupplier = new Supplier
            {
                Name = "NaturalSource Egypt",
                Description = "Organic ingredients and packaging for natural beauty products",
                Email = "info@naturalsource.eg",
                Phone = "+20-2-5555-9999",
                Address = "8 Nile Corniche",
                City = "Giza",
                Country = "Egypt",
                ContactPerson = "Sara Mostafa",
                IsActive = true,
                CreatedBy = "seed"
            };

            var packagingSupplier = new Supplier
            {
                Name = "PackPro Egypt",
                Description = "Packaging materials for all business types — boxes, bags, labels",
                Email = "contact@packproeg.com",
                Phone = "+20-2-3333-7777",
                Address = "22 Industrial Zone",
                City = "10th of Ramadan City",
                Country = "Egypt",
                ContactPerson = "Omar Khalil",
                IsActive = true,
                CreatedBy = "seed"
            };

            context.Set<Supplier>().AddRange(fabricSupplier, craftSupplier, beautySupplier, packagingSupplier);
            await context.SaveChangesAsync();

            // ── Raw Materials — Fashion & Accessories ─────────────

            var fashionMaterials = new List<RawMaterial>
            {
                new()
                {
                    Name = "Cotton Fabric",
                    Description = "High-quality 100% cotton fabric, suitable for shirts, dresses and light garments",
                    Price = 45.00m,
                    Unit = "meter",
                    MinimumOrderQuantity = 5,
                    StockQuantity = 500,
                    Category = "Fashion & Accessories",
                    IsAvailable = true,
                    SupplierId = fabricSupplier.Id
                },
                new()
                {
                    Name = "Linen Fabric",
                    Description = "Natural linen fabric, breathable and durable for summer wear",
                    Price = 65.00m,
                    Unit = "meter",
                    MinimumOrderQuantity = 3,
                    StockQuantity = 300,
                    Category = "Fashion & Accessories",
                    IsAvailable = true,
                    SupplierId = fabricSupplier.Id
                },
                new()
                {
                    Name = "Denim Fabric",
                    Description = "Medium-weight denim, ideal for jeans, jackets and bags",
                    Price = 80.00m,
                    Unit = "meter",
                    MinimumOrderQuantity = 3,
                    StockQuantity = 250,
                    Category = "Fashion & Accessories",
                    IsAvailable = true,
                    SupplierId = fabricSupplier.Id
                },
                new()
                {
                    Name = "Sewing Thread (Polyester)",
                    Description = "Strong polyester thread, 500m spool, available in 20+ colors",
                    Price = 12.00m,
                    Unit = "spool",
                    MinimumOrderQuantity = 10,
                    StockQuantity = 1000,
                    Category = "Fashion & Accessories",
                    IsAvailable = true,
                    SupplierId = fabricSupplier.Id
                },
                new()
                {
                    Name = "Metal Zippers (20cm)",
                    Description = "Heavy-duty metal zippers, suitable for bags and jackets",
                    Price = 8.50m,
                    Unit = "piece",
                    MinimumOrderQuantity = 20,
                    StockQuantity = 2000,
                    Category = "Fashion & Accessories",
                    IsAvailable = true,
                    SupplierId = fabricSupplier.Id
                },
                new()
                {
                    Name = "Assorted Buttons (Pack of 50)",
                    Description = "Mixed buttons — plastic and metal, various sizes",
                    Price = 15.00m,
                    Unit = "pack",
                    MinimumOrderQuantity = 5,
                    StockQuantity = 500,
                    Category = "Fashion & Accessories",
                    IsAvailable = true,
                    SupplierId = fabricSupplier.Id
                },
                new()
                {
                    Name = "Faux Leather Sheet",
                    Description = "PU faux leather, 140cm wide, great for bags and accessories",
                    Price = 95.00m,
                    Unit = "meter",
                    MinimumOrderQuantity = 2,
                    StockQuantity = 150,
                    Category = "Fashion & Accessories",
                    IsAvailable = true,
                    SupplierId = fabricSupplier.Id
                },
                new()
                {
                    Name = "Elastic Band (2cm wide)",
                    Description = "Flat elastic band, sold per meter, suitable for waistbands",
                    Price = 5.00m,
                    Unit = "meter",
                    MinimumOrderQuantity = 20,
                    StockQuantity = 800,
                    Category = "Fashion & Accessories",
                    IsAvailable = true,
                    SupplierId = fabricSupplier.Id
                }
            };

            // ── Raw Materials — Handmade Crafts ───────────────────

            var craftMaterials = new List<RawMaterial>
            {
                new()
                {
                    Name = "Soy Wax Flakes",
                    Description = "Natural soy wax for candle making, burns clean and slow",
                    Price = 55.00m,
                    Unit = "kg",
                    MinimumOrderQuantity = 2,
                    StockQuantity = 200,
                    Category = "Handmade Crafts",
                    IsAvailable = true,
                    SupplierId = craftSupplier.Id
                },
                new()
                {
                    Name = "Epoxy Resin Kit (A+B)",
                    Description = "Crystal clear epoxy resin for art, jewelry and coatings",
                    Price = 120.00m,
                    Unit = "kg",
                    MinimumOrderQuantity = 1,
                    StockQuantity = 100,
                    Category = "Handmade Crafts",
                    IsAvailable = true,
                    SupplierId = craftSupplier.Id
                },
                new()
                {
                    Name = "Air Dry Clay",
                    Description = "Smooth white air-dry clay, no kiln required",
                    Price = 35.00m,
                    Unit = "kg",
                    MinimumOrderQuantity = 2,
                    StockQuantity = 150,
                    Category = "Handmade Crafts",
                    IsAvailable = true,
                    SupplierId = craftSupplier.Id
                },
                new()
                {
                    Name = "Macramé Cord (3mm)",
                    Description = "Natural cotton macramé cord for wall hangings and plant hangers",
                    Price = 40.00m,
                    Unit = "100m roll",
                    MinimumOrderQuantity = 3,
                    StockQuantity = 200,
                    Category = "Handmade Crafts",
                    IsAvailable = true,
                    SupplierId = craftSupplier.Id
                },
                new()
                {
                    Name = "Candle Wicks (Pack of 50)",
                    Description = "Pre-tabbed cotton wicks, suitable for container candles",
                    Price = 25.00m,
                    Unit = "pack",
                    MinimumOrderQuantity = 5,
                    StockQuantity = 300,
                    Category = "Handmade Crafts",
                    IsAvailable = true,
                    SupplierId = craftSupplier.Id
                },
                new()
                {
                    Name = "Candle Fragrance Oil",
                    Description = "High-load fragrance oils for candles, 30+ scents available",
                    Price = 85.00m,
                    Unit = "100ml",
                    MinimumOrderQuantity = 3,
                    StockQuantity = 180,
                    Category = "Handmade Crafts",
                    IsAvailable = true,
                    SupplierId = craftSupplier.Id
                },
                new()
                {
                    Name = "Acrylic Paint Set",
                    Description = "Non-toxic acrylic paints, 24 colors, artist grade",
                    Price = 95.00m,
                    Unit = "set",
                    MinimumOrderQuantity = 2,
                    StockQuantity = 100,
                    Category = "Handmade Crafts",
                    IsAvailable = true,
                    SupplierId = craftSupplier.Id
                },
                new()
                {
                    Name = "Silicone Mold — Geometric",
                    Description = "Flexible silicone molds for resin and soap casting, geometric shapes",
                    Price = 60.00m,
                    Unit = "piece",
                    MinimumOrderQuantity = 2,
                    StockQuantity = 120,
                    Category = "Handmade Crafts",
                    IsAvailable = true,
                    SupplierId = craftSupplier.Id
                }
            };

            // ── Raw Materials — Natural & Beauty Products ─────────

            var beautyMaterials = new List<RawMaterial>
            {
                new()
                {
                    Name = "Shea Butter (Raw)",
                    Description = "Unrefined raw shea butter, rich in vitamins A and E",
                    Price = 90.00m,
                    Unit = "kg",
                    MinimumOrderQuantity = 1,
                    StockQuantity = 100,
                    Category = "Natural & Beauty Products",
                    IsAvailable = true,
                    SupplierId = beautySupplier.Id
                },
                new()
                {
                    Name = "Coconut Oil (Fractionated)",
                    Description = "Liquid coconut carrier oil, lightweight, great for skin and hair",
                    Price = 75.00m,
                    Unit = "liter",
                    MinimumOrderQuantity = 2,
                    StockQuantity = 150,
                    Category = "Natural & Beauty Products",
                    IsAvailable = true,
                    SupplierId = beautySupplier.Id
                },
                new()
                {
                    Name = "Argan Oil",
                    Description = "Pure Moroccan argan oil, cold-pressed, suitable for face and hair",
                    Price = 250.00m,
                    Unit = "100ml",
                    MinimumOrderQuantity = 5,
                    StockQuantity = 80,
                    Category = "Natural & Beauty Products",
                    IsAvailable = true,
                    SupplierId = beautySupplier.Id
                },
                new()
                {
                    Name = "Lavender Essential Oil",
                    Description = "Pure Bulgarian lavender essential oil, therapeutic grade",
                    Price = 180.00m,
                    Unit = "30ml",
                    MinimumOrderQuantity = 5,
                    StockQuantity = 100,
                    Category = "Natural & Beauty Products",
                    IsAvailable = true,
                    SupplierId = beautySupplier.Id
                },
                new()
                {
                    Name = "Soap Base (Melt & Pour)",
                    Description = "Clear glycerin soap base, easy to use, accepts colors and fragrances",
                    Price = 50.00m,
                    Unit = "kg",
                    MinimumOrderQuantity = 3,
                    StockQuantity = 200,
                    Category = "Natural & Beauty Products",
                    IsAvailable = true,
                    SupplierId = beautySupplier.Id
                },
                new()
                {
                    Name = "Beeswax (Pellets)",
                    Description = "Natural yellow beeswax pellets for lip balms, creams and candles",
                    Price = 110.00m,
                    Unit = "kg",
                    MinimumOrderQuantity = 1,
                    StockQuantity = 80,
                    Category = "Natural & Beauty Products",
                    IsAvailable = true,
                    SupplierId = beautySupplier.Id
                },
                new()
                {
                    Name = "Rose Water",
                    Description = "Pure distilled rose water, ideal as a facial toner base",
                    Price = 35.00m,
                    Unit = "liter",
                    MinimumOrderQuantity = 5,
                    StockQuantity = 200,
                    Category = "Natural & Beauty Products",
                    IsAvailable = true,
                    SupplierId = beautySupplier.Id
                },
                new()
                {
                    Name = "Vitamin E Oil",
                    Description = "Pure vitamin E tocopherol, natural preservative and skin conditioner",
                    Price = 140.00m,
                    Unit = "100ml",
                    MinimumOrderQuantity = 5,
                    StockQuantity = 100,
                    Category = "Natural & Beauty Products",
                    IsAvailable = true,
                    SupplierId = beautySupplier.Id
                }
            };

            // ── Shared Packaging Materials (all categories) ───────

            var packagingMaterials = new List<RawMaterial>
            {
                new()
                {
                    Name = "Kraft Paper Boxes (Small)",
                    Description = "Eco-friendly kraft paper gift boxes, 10x10x5cm",
                    Price = 6.00m,
                    Unit = "piece",
                    MinimumOrderQuantity = 50,
                    StockQuantity = 5000,
                    Category = "Fashion & Accessories,Handmade Crafts,Natural & Beauty Products",
                    IsAvailable = true,
                    SupplierId = packagingSupplier.Id
                },
                new()
                {
                    Name = "Glass Jar (50ml)",
                    Description = "Clear glass jar with aluminum lid, food-safe, great for creams",
                    Price = 8.50m,
                    Unit = "piece",
                    MinimumOrderQuantity = 24,
                    StockQuantity = 3000,
                    Category = "Natural & Beauty Products,Handmade Crafts",
                    IsAvailable = true,
                    SupplierId = packagingSupplier.Id
                },
                new()
                {
                    Name = "Amber Dropper Bottle (30ml)",
                    Description = "UV-protective amber glass bottle with dropper, ideal for serums and oils",
                    Price = 9.00m,
                    Unit = "piece",
                    MinimumOrderQuantity = 24,
                    StockQuantity = 2000,
                    Category = "Natural & Beauty Products",
                    IsAvailable = true,
                    SupplierId = packagingSupplier.Id
                },
                new()
                {
                    Name = "Custom Label Paper (A4)",
                    Description = "Waterproof matte label paper for printing product labels",
                    Price = 20.00m,
                    Unit = "10-sheet pack",
                    MinimumOrderQuantity = 10,
                    StockQuantity = 1000,
                    Category = "Fashion & Accessories,Handmade Crafts,Natural & Beauty Products",
                    IsAvailable = true,
                    SupplierId = packagingSupplier.Id
                }
            };

            context.Set<RawMaterial>().AddRange(fashionMaterials);
            context.Set<RawMaterial>().AddRange(craftMaterials);
            context.Set<RawMaterial>().AddRange(beautyMaterials);
            context.Set<RawMaterial>().AddRange(packagingMaterials);

            await context.SaveChangesAsync();
        }
    }
}
