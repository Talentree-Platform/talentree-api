// ============================================================
// Talentree.Repository/Data/DataSeed/SupportTicketSeed.cs
// ============================================================
// Seeds realistic support tickets in various states so the
// Admin support panel has meaningful data to display.
// Must run AFTER BusinessOwnerSeed (BO user IDs needed).
// ============================================================
using Talentree.Core.Entities;
using Talentree.Core.Enums;

namespace Talentree.Repository.Data.DataSeed
{
    public static class SupportTicketSeed
    {
        public static async Task SeedAsync(TalentreeDbContext context)
        {
            if (context.Set<SupportTicket>().Any()) return;

            var fashionBoId = BusinessOwnerSeed.FashionBoId;
            var craftsBoId  = BusinessOwnerSeed.CraftsBoId;
            var beautyBoId  = BusinessOwnerSeed.BeautyBoId;

            // Verify BOs exist before seeding
            if (!context.Users.Any(u => u.Id == fashionBoId)) return;

            var adminId = context.Users
                .FirstOrDefault(u => u.Email == "admin@talentree.com")?.Id;

            var tickets = new List<SupportTicket>
            {
                // ── Ticket 1: Open, High Priority — Payment issue (Fashion BO) ──────
                new()
                {
                    TicketNumber        = "TKT-2025-001",
                    Subject             = "Payment not reflecting after successful Stripe transaction",
                    Description         = "I placed a material order (Order #3) 3 days ago and Stripe shows the payment as successful, but my order status still shows as Unpaid. I've attached the Stripe receipt. Please investigate urgently.",
                    Category            = TicketCategory.Payment,
                    Status              = TicketStatus.Open,
                    Priority            = TicketPriority.High,
                    BusinessOwnerUserId = fashionBoId,
                    CreatedAt           = DateTime.UtcNow.AddDays(-3),
                    Messages = new List<TicketMessage>
                    {
                        new()
                        {
                            SenderId       = fashionBoId,
                            Content        = "I placed a material order (Order #3) 3 days ago and Stripe shows the payment as successful, but my order status still shows as Unpaid. Please investigate urgently.",
                            IsAdminMessage = false,
                            CreatedAt      = DateTime.UtcNow.AddDays(-3)
                        }
                    }
                },

                // ── Ticket 2: InProgress, Normal — Product approval delay (Crafts BO) ──
                new()
                {
                    TicketNumber        = "TKT-2025-002",
                    Subject             = "Product pending approval for over 5 days",
                    Description         = "My Macrame Wall Hanging has been in 'Pending Approval' status for 5 days. I submitted all required photos and description. Is there an issue with my listing? Please advise.",
                    Category            = TicketCategory.Product,
                    Status              = TicketStatus.InProgress,
                    Priority            = TicketPriority.Normal,
                    BusinessOwnerUserId = craftsBoId,
                    AssignedToAdminId   = adminId,
                    AssignedAt          = DateTime.UtcNow.AddDays(-6),
                    CreatedAt           = DateTime.UtcNow.AddDays(-7),
                    Messages = new List<TicketMessage>
                    {
                        new()
                        {
                            SenderId       = craftsBoId,
                            Content        = "My Macrame Wall Hanging has been in pending approval for 5 days. Is there an issue with my listing?",
                            IsAdminMessage = false,
                            CreatedAt      = DateTime.UtcNow.AddDays(-7)
                        },
                        new()
                        {
                            SenderId       = adminId ?? fashionBoId,
                            Content        = "Hi Karim, we've received your ticket and are reviewing the product listing. We'll get back to you within 24 hours with an update.",
                            IsAdminMessage = true,
                            CreatedAt      = DateTime.UtcNow.AddDays(-6)
                        },
                        new()
                        {
                            SenderId       = craftsBoId,
                            Content        = "Thank you for the quick response. I'm happy to provide any additional information if needed.",
                            IsAdminMessage = false,
                            CreatedAt      = DateTime.UtcNow.AddDays(-5)
                        }
                    }
                },

                // ── Ticket 3: Resolved — Account login issue (Beauty BO) ────────────
                new()
                {
                    TicketNumber        = "TKT-2025-003",
                    Subject             = "Unable to login after password reset",
                    Description         = "I requested a password reset 2 hours ago and received the email, but the reset link returns a 'token invalid' error. I've tried multiple times. Please help me regain access to my account.",
                    Category            = TicketCategory.Account,
                    Status              = TicketStatus.Resolved,
                    Priority            = TicketPriority.High,
                    BusinessOwnerUserId = beautyBoId,
                    AssignedToAdminId   = adminId,
                    AssignedAt          = DateTime.UtcNow.AddDays(-13),
                    ResolvedAt          = DateTime.UtcNow.AddDays(-12),
                    ResolvedBy          = "admin@talentree.com",
                    CreatedAt           = DateTime.UtcNow.AddDays(-14),
                    Messages = new List<TicketMessage>
                    {
                        new()
                        {
                            SenderId       = beautyBoId,
                            Content        = "I'm unable to login after password reset. The link in the email gives a 'token invalid' error.",
                            IsAdminMessage = false,
                            CreatedAt      = DateTime.UtcNow.AddDays(-14)
                        },
                        new()
                        {
                            SenderId       = adminId ?? fashionBoId,
                            Content        = "Hi Salma, we've manually reset your password. Your temporary password is: TempPass@2025. Please log in and change it immediately. Apologies for the inconvenience.",
                            IsAdminMessage = true,
                            CreatedAt      = DateTime.UtcNow.AddDays(-13)
                        },
                        new()
                        {
                            SenderId       = beautyBoId,
                            Content        = "That worked! Thank you so much for the quick resolution.",
                            IsAdminMessage = false,
                            CreatedAt      = DateTime.UtcNow.AddDays(-12)
                        }
                    }
                },

                // ── Ticket 4: Open, Low — General inquiry (Fashion BO) ──────────────
                new()
                {
                    TicketNumber        = "TKT-2025-004",
                    Subject             = "How do I update my business bank account details?",
                    Description         = "I recently opened a new bank account and need to update my bank details for payout requests. I cannot find this option in my dashboard settings. Could you guide me?",
                    Category            = TicketCategory.Account,
                    Status              = TicketStatus.Open,
                    Priority            = TicketPriority.Low,
                    BusinessOwnerUserId = fashionBoId,
                    CreatedAt           = DateTime.UtcNow.AddDays(-1),
                    Messages = new List<TicketMessage>
                    {
                        new()
                        {
                            SenderId       = fashionBoId,
                            Content        = "How do I update my bank account details for payout requests? I cannot find the option in my settings.",
                            IsAdminMessage = false,
                            CreatedAt      = DateTime.UtcNow.AddDays(-1)
                        }
                    }
                },

                // ── Ticket 5: Closed — Technical (Crafts BO) ────────────────────────
                new()
                {
                    TicketNumber        = "TKT-2025-005",
                    Subject             = "Image upload failing for product listing",
                    Description         = "When I try to upload product images for my new candle listing, the upload spinner runs for a few seconds and then shows 'Upload failed'. I've tried different image formats (JPG, PNG) and sizes.",
                    Category            = TicketCategory.Technical,
                    Status              = TicketStatus.Closed,
                    Priority            = TicketPriority.Normal,
                    BusinessOwnerUserId = craftsBoId,
                    AssignedToAdminId   = adminId,
                    AssignedAt          = DateTime.UtcNow.AddDays(-20),
                    ResolvedAt          = DateTime.UtcNow.AddDays(-18),
                    ResolvedBy          = "admin@talentree.com",
                    ClosedAt            = DateTime.UtcNow.AddDays(-17),
                    ClosedBy            = adminId,
                    CreatedAt           = DateTime.UtcNow.AddDays(-21),
                    Messages = new List<TicketMessage>
                    {
                        new()
                        {
                            SenderId       = craftsBoId,
                            Content        = "Image upload is failing for my new candle listing. Tried JPG and PNG, same error.",
                            IsAdminMessage = false,
                            CreatedAt      = DateTime.UtcNow.AddDays(-21)
                        },
                        new()
                        {
                            SenderId       = adminId ?? fashionBoId,
                            Content        = "Hi Karim, we identified a temporary issue with our image storage service. It has been fixed. Please try uploading again. Your images should now upload successfully.",
                            IsAdminMessage = true,
                            CreatedAt      = DateTime.UtcNow.AddDays(-18)
                        },
                        new()
                        {
                            SenderId       = craftsBoId,
                            Content        = "Working perfectly now. Thank you for fixing it so quickly!",
                            IsAdminMessage = false,
                            CreatedAt      = DateTime.UtcNow.AddDays(-17)
                        }
                    }
                },

                // ── Ticket 6: Open, Urgent — Payout not received (Beauty BO) ────────
                new()
                {
                    TicketNumber        = "TKT-2025-006",
                    Subject             = "Approved payout not received after 7 business days",
                    Description         = "My payout request of 8,000 EGP was approved 9 days ago but I still haven't received the money in my NBE account. The bank confirmed no incoming transfer. Please urgently investigate.",
                    Category            = TicketCategory.Payment,
                    Status              = TicketStatus.Open,
                    Priority            = TicketPriority.Urgent,
                    BusinessOwnerUserId = beautyBoId,
                    CreatedAt           = DateTime.UtcNow.AddHours(-5),
                    Messages = new List<TicketMessage>
                    {
                        new()
                        {
                            SenderId       = beautyBoId,
                            Content        = "My payout of 8,000 EGP was approved 9 days ago but I haven't received it. My bank confirmed no incoming transfer. Please investigate urgently.",
                            IsAdminMessage = false,
                            CreatedAt      = DateTime.UtcNow.AddHours(-5)
                        }
                    }
                }
            };

            context.Set<SupportTicket>().AddRange(tickets);
            await context.SaveChangesAsync();
        }
    }
}
