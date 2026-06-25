using Stripe;
using Talentree.API.Services;
using Talentree.Core;
using Talentree.Core.Repository.Contract;
using Talentree.Repository;
using Talentree.Service.Messaging;
using Talentree.Service.Messaging.Consumers;
using Talentree.Service.Contracts;
using Talentree.Service.Services;

namespace Talentree.API.Extentions
{
    public static class ApplicationServicesExtention
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient();

            services.AddSingleton<RabbitMQConnectionManager>();
            services.AddSingleton<IEventPublisher, RabbitMQEventPublisher>();
            services.AddHostedService<RabbitMQInitializer>();

            services.AddHostedService<UserInteractionConsumer>();
            services.AddHostedService<ReviewSentimentConsumer>();
            services.AddHostedService<SupportTriageConsumer>();
            services.AddHostedService<ProductAnalyticsConsumer>();
            services.AddHostedService<ProfileCompletenessConsumer>();
            services.AddHostedService<ChurnPredictionConsumer>();
            services.AddHostedService<CustomerRecommendationConsumer>();
            services.AddHostedService<OwnerProcurementConsumer>();
            services.AddHostedService<AIRetrainConsumer>();
            services.AddHostedService<FraudPredictionConsumer>();
            services.AddHostedService<RequestComputationConsumer>();
            services.AddHostedService<AnomalyPredictionConsumer>();

            services.AddScoped<IAdminOrderService, AdminOrderService>();
            services.AddScoped<IRefundService, Talentree.Service.Services.RefundService>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            // Auth Service
            services.AddScoped<IAuthService, AuthService>();
            

            // User Interaction Service
            services.AddScoped<IUserInteractionService, UserInteractionService>();
            //services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<ITokenService, Talentree.Service.Services.TokenService>();

            // Admin Services
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<ISupplierService, SupplierService>();           
            services.AddScoped<IAdminRawMaterialService, AdminRawMaterialService>();

            // product service
            services.AddScoped<IProductService, Talentree.Service.Services.ProductService>();
            services.AddScoped<IImageService, ImageService>();
            // FR-AD-09, FR-AD-10, FR-AD-11: Admin product moderation & low-stock
            services.AddScoped<IAdminProductService, AdminProductService>();


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
            services.AddScoped<INotificationHelperService, NotificationHelperService>();

            // Account Settings and Encryption Services
            services.AddScoped<IAccountSettingsService, AccountSettingsService>();
            services.AddScoped<IEncryptionService, EncryptionService>();


            services.AddScoped<ISupportService, SupportService>();
            services.AddScoped<IFileService, Talentree.Service.Services.FileService>();
            // Knowledge Base Services
            services.AddScoped<IKnowledgeService, KnowledgeService>();
            // FR-AD-40 to FR-AD-43: Admin Content Management
            services.AddScoped<IAdminKnowledgeService, AdminKnowledgeService>();
            // Review Service
            services.AddScoped<IReviewService, Talentree.Service.Services.ReviewService>();

            // Payment Service
            services.AddScoped<IPaymentService, PaymentService>();

            // Customer Services
            services.AddScoped<ICartService, CartService>();
            services.AddScoped<ICustomerOrderService, CustomerOrderService>();
            services.AddScoped<IWishlistService, WishlistService>();

            // ── Stripe ────────────────────────────────────────────────
            StripeConfiguration.ApiKey = configuration["Stripe:SecretKey"];
            services.AddScoped<Stripe.PaymentIntentService>();

            // Financial Service 
            services.AddScoped<IFinancialService, FinancialService>();

            // Payout Service
            services.AddScoped<IPayoutService, Talentree.Service.Services.PayoutService>();

            // user management service
            services.AddScoped<IUserManagementService, UserManagementService>();

            // ── Platform Settings (FR-AD-31 to FR-AD-36) ──────────────────────────
            // FR-AD-31: Category Management
            services.AddScoped<ICategoryManagementService, CategoryManagementService>();
            // FR-AD-32: Commission & Fee Configuration
            services.AddScoped<ICommissionSettingService, CommissionSettingService>();
            // FR-AD-33: Shipping Configuration
            services.AddScoped<IShippingSettingsService, ShippingSettingsService>();
            // FR-AD-34: Tax Configuration
            services.AddScoped<ITaxSettingsService, TaxSettingsService>();
            // FR-AD-35: Homepage Management
            services.AddScoped<IHomepageManagementService, HomepageManagementService>();
            // FR-AD-36: Terms & Policies
            services.AddScoped<IPolicyService, PolicyService>();

            // Register HttpClient for AI service
            services.AddHttpClient<IAIService, AIService>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            return services;
        }

    }
}
