using App.Core.Jwt;
using FluentValidation;

namespace App.Logic.Validators
{
    public class RefreshTokenDtoValidator : AbstractValidator<RefreshTokenDto>
    {
        public RefreshTokenDtoValidator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("Refresh token is required.")
                 .WithMessage("Refresh token must be valid.");

            RuleFor(x => x.UserNumber)
                .NotEmpty().WithMessage("User number is required.")
                .Length(11).WithMessage("User number must be exactly 11 characters long.");
        }
    }
}