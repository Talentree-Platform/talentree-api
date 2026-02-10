// Talentree.Service/Validators/Auth/ExternalLoginDtoValidator.cs
using FluentValidation;
using Talentree.Service.DTOs.Auth;

namespace Talentree.Service.Validators.Auth
{
    public class ExternalLoginDtoValidator : AbstractValidator<ExternalLoginDto>
    {
        public ExternalLoginDtoValidator()
        {
            RuleFor(x => x.IdToken)
                .NotEmpty().WithMessage("ID token is required");

            RuleFor(x => x.Provider)
                .NotEmpty().WithMessage("Provider is required")
                .Must(p => p == "Google" || p == "Facebook")
                .WithMessage("Provider must be either Google or Facebook");
        }
    }
}
