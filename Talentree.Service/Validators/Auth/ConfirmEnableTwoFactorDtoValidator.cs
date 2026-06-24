using FluentValidation;
using Talentree.Service.DTOs.Auth;

namespace Talentree.Service.Validators.Auth
{
    public class ConfirmEnableTwoFactorDtoValidator : AbstractValidator<ConfirmEnableTwoFactorDto>
    {
        public ConfirmEnableTwoFactorDtoValidator()
        {
            RuleFor(x => x.OtpCode)
                .NotEmpty().WithMessage("OTP code is required.")
                .Length(6).WithMessage("OTP code must be exactly 6 characters.");
        }
    }
}
