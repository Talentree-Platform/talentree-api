using Stripe;
using Talentree.API.Services;
using Talentree.Core;
using Talentree.Core.Repository.Contract;
using Talentree.Repository;
using Talentree.Service.Contracts;
using Talentree.Service.Services;

namespace Talentree.API.Extentions
{
    public static class ApplicationServicesExtention
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            // Auth Service
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<ITokenService, Talentree.Service.Services.TokenService>();

            // Admin Services
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<ISupplierService, SupplierService>();           
            services.AddScoped<IAdminRawMaterialService, AdminRawMaterialService>();

            // product service
            services.AddScoped<IProductService, Talentree.Service.Services.ProductService>();
            services.AddScoped<IImageService, ImageService>();


            // Add AutoMapper (scans assemblies)
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            // Raw Material Store
            services.AddScoped<IRawMaterialService, RawMaterialService>();
            services.AddScoped<IMaterialBasketService, MaterialBasketService>();

            // Order Service
            services.AddScoped<IMaterialOrderService, MaterialOrderService>();

            // BO Production Request Service
            services.AddScoped<IBoProductionRequestService, BoProductionRequestService>();

            // Admin Production Request Service
            services.AddScoped<IAdminProductionRequestService, AdminProductionRequestService>();

            // Notification and Hub Services
            services.AddScoped<IHubService, HubService>();
            services.AddScoped<INotificationService, NotificationService>();

            // Account Settings and Encryption Services
            services.AddScoped<IAccountSettingsService, AccountSettingsService>();
            services.AddScoped<IEncryptionService, EncryptionService>();


            services.AddScoped<ISupportService, SupportService>();
            services.AddScoped<IFileService, Talentree.Service.Services.FileService>();
            // Knowledge Base Service
            services.AddScoped<IKnowledgeService, KnowledgeService>();
            // Review Service
            services.AddScoped<IReviewService, Talentree.Service.Services.ReviewService>();

            // Payment Service
            services.AddScoped<IPaymentService, PaymentService>();

            // ── Stripe ────────────────────────────────────────────────
            StripeConfiguration.ApiKey = configuration["Stripe:SecretKey"];
            services.AddScoped<Stripe.PaymentIntentService>();

            // Financial Service 
            services.AddScoped<IFinancialService, FinancialService>();

            // Payout Service
            services.AddScoped<IPayoutService, Talentree.Service.Services.PayoutService>();

            // user management service
            services.AddScoped<IUserManagementService, UserManagementService>();

            return services;
        }

    }
}
