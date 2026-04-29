using Talentree.Core.Entities;
using Talentree.Repository.Data;

namespace Talentree.Repository.Data.SeedData
{
    public static class FAQSeeder
    {
        public static async Task SeedFAQsAsync(TalentreeDbContext context)
        {
            if (context.FAQs.Any()) return;

            var faqs = new List<FAQ>
            {
                // Getting Started
                new FAQ
                {
                    Question = "How do I create my first product?",
                    Answer = "<p>To create your first product:</p><ol><li>Complete your business owner profile</li><li>Wait for admin approval</li><li>Go to Products > Add Product</li><li>Fill in product details and upload images</li><li>Submit for approval</li></ol>",
                    Category = "Getting Started",
                    DisplayOrder = 1,
                    IsPublished = true,
                    Keywords = "product, create, first, new, add",
                    CreatedAt = DateTime.UtcNow
                },
                new FAQ
                {
                    Question = "How long does profile approval take?",
                    Answer = "<p>Business owner profile approval typically takes 1-2 business days. You'll receive an email notification once your profile is reviewed.</p>",
                    Category = "Getting Started",
                    DisplayOrder = 2,
                    IsPublished = true,
                    Keywords = "approval, profile, time, wait, how long",
                    CreatedAt = DateTime.UtcNow
                },

                // Products
                new FAQ
                {
                    Question = "What image requirements are needed for products?",
                    Answer = "<p>Product images must:</p><ul><li>Be in JPEG, PNG, or WebP format</li><li>Maximum 5MB per image</li><li>Minimum resolution: 800x800 pixels</li><li>Show clear view of the product</li><li>No watermarks or text overlays</li></ul>",
                    Category = "Products",
                    DisplayOrder = 1,
                    IsPublished = true,
                    Keywords = "image, photo, upload, size, format, requirements",
                    CreatedAt = DateTime.UtcNow
                },
                new FAQ
                {
                    Question = "Why was my product rejected?",
                    Answer = "<p>Common rejection reasons:</p><ul><li>Poor quality images</li><li>Incomplete product description</li><li>Prohibited items</li><li>Wrong category selection</li></ul><p>Check the rejection reason in your notification and make necessary updates.</p>",
                    Category = "Products",
                    DisplayOrder = 2,
                    IsPublished = true,
                    Keywords = "rejected, denied, why, reason, product approval",
                    CreatedAt = DateTime.UtcNow
                },

                // Orders
                new FAQ
                {
                    Question = "How do I track my orders?",
                    Answer = "<p>Go to Orders section in your dashboard to view all orders. You can filter by status and see detailed order information including customer details and shipping address.</p>",
                    Category = "Orders",
                    DisplayOrder = 1,
                    IsPublished = true,
                    Keywords = "orders, track, view, status, customer",
                    CreatedAt = DateTime.UtcNow
                },

                // Payments
                new FAQ
                {
                    Question = "When will I receive my payment?",
                    Answer = "<p>Payments are processed within 3-5 business days after order completion. You can view your payout history in the Financial Dashboard.</p>",
                    Category = "Payments",
                    DisplayOrder = 1,
                    IsPublished = true,
                    Keywords = "payment, payout, money, receive, when, transfer",
                    CreatedAt = DateTime.UtcNow
                },
                new FAQ
                {
                    Question = "What payment methods are supported?",
                    Answer = "<p>We support:</p><ul><li>Bank transfers</li><li>PayPal</li><li>Stripe</li></ul><p>You can set up your preferred payment method in Account Settings.</p>",
                    Category = "Payments",
                    DisplayOrder = 2,
                    IsPublished = true,
                    Keywords = "payment methods, bank, paypal, stripe, how to receive",
                    CreatedAt = DateTime.UtcNow
                },

                // Technical
                new FAQ
                {
                    Question = "I'm having trouble uploading images",
                    Answer = "<p>Try these solutions:</p><ol><li>Check file size (max 5MB)</li><li>Use supported formats (JPEG, PNG, WebP)</li><li>Clear browser cache</li><li>Try a different browser</li></ol><p>If issue persists, contact support.</p>",
                    Category = "Technical",
                    DisplayOrder = 1,
                    IsPublished = true,
                    Keywords = "upload, image, problem, error, technical, issue",
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.FAQs.AddRangeAsync(faqs);
            await context.SaveChangesAsync();
        }
    }
}