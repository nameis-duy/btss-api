using Application.DTOs.Generic;
using Application.DTOs.Transaction;
using Domain.Enums.Transaction;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace Infrastructure.Validators.Transaction
{
    public class TopUpCreateValidator : AbstractValidator<TopUpCreate>
    {
        private readonly AppConfig config;
        public TopUpCreateValidator(IOptionsSnapshot<AppConfig> configSnapshot)
        {
            config = configSnapshot.Value;
            RuleFor(t => t.Gateway).IsInEnum()
                                   .NotEqual(Gateway.INTERNAL)
                                   .WithMessage(AppMessage.ERR_ENUM_GATEWAY);
            RuleFor(t => t.Amount).InclusiveBetween(config.MIN_TOPUP, config.MAX_TOPUP)
                                  .WithMessage(string.Format(AppMessage.ERR_TOPUP_AMOUNT, config.MIN_TOPUP, config.MAX_TOPUP));
        }
    }
}
