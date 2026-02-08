using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talentree.Service.DTOs.Auth;

namespace Talentree.Service.Validators.Auth
{
    public class BusinessOwnerRegisterDtoValidator : AbstractValidator<BusinessOwnerRegisterDto>
    {

        public BusinessOwnerRegisterDtoValidator() {

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .MinimumLength(3).WithMessage("First name must be at least 3 characters")
                .MaximumLength(50).WithMessage("First name cannot exceed 50 characters");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .MinimumLength(3).WithMessage("Last name must be at least 3 characters")
                .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%?&])[A-Za-z\d@$!%?&]{8,}$")
                .WithMessage("Password must be at least 8 characters and contain uppercase, lowercase, number and special character");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("Confirm password is required")
                .Equal(x => x.Password).WithMessage("Passwords do not match");

            // Phone is optional but if provided, validate format
            RuleFor(x => x.PhoneNumber)
                .Matches(@"^\+?\d{7,15}$")
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
                .WithMessage("Invalid phone number format");
            // Business Name 
            RuleFor(x => x.BusinessName)
              .NotEmpty().WithMessage("Business name is required")
              .MinimumLength(3).WithMessage("Business name must be at least 3 characters")
              .MaximumLength(100).WithMessage("Business name cannot exceed 100 characters");

            RuleFor(x => x.BusinessCategory)
                .NotEmpty().WithMessage("Business category is required");

            RuleFor(x => x.BusinessDescription)
                .NotEmpty().WithMessage("Business description is required")
                .MinimumLength(20).WithMessage("Description should be at least 20 characters for better visibility");

          
            RuleFor(x => x.WebsiteLink)
                .Must(LinkMustBeAttribute).WithMessage("Invalid Website URL")
                .When(x => !string.IsNullOrEmpty(x.WebsiteLink));
        }

    
        private bool LinkMustBeAttribute(string? link)
        {
            if (string.IsNullOrWhiteSpace(link)) return true;
            return Uri.TryCreate(link, UriKind.Absolute, out _);
        }
    }
}
