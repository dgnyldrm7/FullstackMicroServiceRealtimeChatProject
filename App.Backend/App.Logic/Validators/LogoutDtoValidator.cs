using App.Core.DTOs;
using FluentValidation;

namespace App.Logic.Validators
{
    public class LogoutDtoValidator : AbstractValidator<LogoutDto>
    {
        public LogoutDtoValidator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("Refresh token is required.")
                .Length(43, 44).WithMessage("Refresh token must be valid and between 43 to 44 characters long.");

            RuleFor(x => x.UserNumber)
                .NotEmpty().WithMessage("User number is required.")
                .Length(11).WithMessage("User number must be exactly 11 characters long.");
        }
    }
}