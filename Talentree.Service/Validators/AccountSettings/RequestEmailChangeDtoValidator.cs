using FluentValidation;
using Talentree.Service.DTOs.AccountSettings;

namespace Talentree.Service.Validators.AccountSettings
{
    public class RequestEmailChangeDtoValidator : AbstractValidator<RequestEmailChangeDto>
    {
        public RequestEmailChangeDtoValidator()
        {
            RuleFor(x => x.NewEmail).NotEmpty().EmailAddress();
        }
    }
}
