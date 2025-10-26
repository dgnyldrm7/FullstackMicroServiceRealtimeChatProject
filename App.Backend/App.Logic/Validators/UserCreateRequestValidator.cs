using App.Core.DTOs;
using FluentValidation;

namespace App.Logic.Validators
{
    public class UserCreateRequestValidator : AbstractValidator<CreateUserDto>
    {
        public UserCreateRequestValidator()
        {
            RuleFor(x => x.UserName)
                .NotEmpty()
                .WithMessage("Username is required.")
                .Length(3, 20)
                .WithMessage("Username must be between 3 and 20 characters.");            

            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email is required.")
                .EmailAddress()
                .WithMessage("Invalid email format.");

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("Password is required.")
                .MinimumLength(6)
                .WithMessage("Password must be at least 6 characters long.");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty()
                .WithMessage("Phone number is required.")
                .Length(11, 11)
                .WithMessage("Phone number must be 11 characters long.");
        }
    }
}
