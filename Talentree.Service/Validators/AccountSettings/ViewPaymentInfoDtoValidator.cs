using FluentValidation;
using Talentree.Service.DTOs.AccountSettings;

namespace Talentree.Service.Validators.AccountSettings
{
    public class ViewPaymentInfoDtoValidator : AbstractValidator<ViewPaymentInfoDto>
    {
        public ViewPaymentInfoDtoValidator()
        {
            RuleFor(x => x.CurrentPassword).NotEmpty();
        }
    }
}
