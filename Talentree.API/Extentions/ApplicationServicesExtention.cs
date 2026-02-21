using Talentree.Core;
using Talentree.Core.Repository.Contract;
using Talentree.Repository;
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
            services.AddScoped<IAdminService, AdminService>();

            // product service
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IImageService, ImageService>();


            // Add AutoMapper (scans assemblies)
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            return services;
        }

    }
}
