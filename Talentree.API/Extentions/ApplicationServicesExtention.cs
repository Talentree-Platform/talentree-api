using Talentree.API.Services;
using Talentree.Core;
using Talentree.Core.Repository.Contract;
using Talentree.Repository;
using Talentree.Service.Contracts;
using Talentree.Service.Contracts;
using Talentree.Service.Services;

namespace Talentree.API.Extentions
{
    public static class ApplicationServicesExtention
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            // Auth Service
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<ITokenService, TokenService>();

            // Admin Services
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<ISupplierService, SupplierService>();           
            services.AddScoped<IAdminRawMaterialService, AdminRawMaterialService>();

            // product service
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IImageService, ImageService>();


            // Add AutoMapper (scans assemblies)
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            // Raw Material Store
            services.AddScoped<IRawMaterialService, RawMaterialService>();
            services.AddScoped<IMaterialBasketService, MaterialBasketService>();

            services.AddScoped<IHubService, HubService>();
            services.AddScoped<INotificationService, NotificationService>();

            return services;
        }

    }
}
