using App.Core.DTOs;
using FluentValidation;

namespace App.Logic.Validators
{
    public class LoginDtoValidator : AbstractValidator<LoginDto>
    {
        public LoginDtoValidator()
        {
            RuleFor(x => x.UserNumber)
                .NotEmpty().WithMessage("User number is required.")
                .Length(11).WithMessage("User number must be exactly 11 characters long.");

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("Password is required.");
        }
    }
}