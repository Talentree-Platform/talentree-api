using FluentValidation;
using Talentree.Service.DTOs.Auth;

namespace Talentree.Service.Validators.Auth
{
    public class DisableTwoFactorDtoValidator : AbstractValidator<DisableTwoFactorDto>
    {
        public DisableTwoFactorDtoValidator()
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage("Current password is required.");
        }
    }
}
