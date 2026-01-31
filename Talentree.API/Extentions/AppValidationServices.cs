using FluentValidation;
using Talentree.Service.Validators.Auth;

namespace Talentree.API.Extentions;
public static class AppValidationServices
{
    public static IServiceCollection ValidationServices(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<RegisterDtoValidator>();
        return services;
    }
}
