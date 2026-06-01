using FluentValidation;
using Talentree.Service.DTOs.AccountSettings;

namespace Talentree.Service.Validators.AccountSettings
{
    public class ConfirmEmailChangeDtoValidator : AbstractValidator<ConfirmEmailChangeDto>
    {
        public ConfirmEmailChangeDtoValidator()
        {
            RuleFor(x => x.OtpCode).NotEmpty().Length(6);
            RuleFor(x => x.NewEmail).NotEmpty().EmailAddress();
        }
    }
}
