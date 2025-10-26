using App.Core.DTOs;
using FluentValidation;

namespace App.Logic.Validators
{
    public class SendMessageDtoValidator : AbstractValidator<SendMessageDto>
    {
        public SendMessageDtoValidator()
        {
            RuleFor(x => x.ReceiverUserNumber)
                .NotEmpty()
                .WithMessage("Receiver user number is required.");
        }
    }
}