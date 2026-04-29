using FluentValidation;
using Talentree.Service.DTOs.Reviews;

namespace Talentree.Service.Validators.Reviews
{
    public class RespondToReviewDtoValidator : AbstractValidator<RespondToReviewDto>
    {
        public RespondToReviewDtoValidator()
        {
            RuleFor(x => x.Response)
                .NotEmpty().WithMessage("Response text is required")
                .MaximumLength(500).WithMessage("Response cannot exceed 500 characters");
        }
    }
}