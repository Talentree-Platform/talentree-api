// Talentree.Service/Validators/Auth/RefreshTokenDtoValidator.cs
using FluentValidation;
using Talentree.Service.DTOs.Auth;

namespace Talentree.Service.Validators.Auth
{
    public class RefreshTokenDtoValidator : AbstractValidator<RefreshTokenDto>
    {
        public RefreshTokenDtoValidator()
        {
        }
    }
}
