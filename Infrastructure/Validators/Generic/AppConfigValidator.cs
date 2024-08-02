using Application.DTOs.Generic;
using Application.Interfaces.Services;
using FluentValidation;
using Infrastructure.Constants;

namespace Infrastructure.Validators.Generic
{
    public class AppConfigValidator : AbstractValidator<AppConfig>
    {
        public AppConfigValidator(ITimeService timeService)
        {
            RuleFor(c => c.USE_FIXED_OTP);
            RuleFor(c => c.DEFAULT_PRESTIGE_POINT)
                .InclusiveBetween(ValidationConstants.MIN_DEFAULT_PRESTIGE_POINT, ValidationConstants.MAX_DEFAULT_PRESTIGE_POINT)
                .WithMessage(string.Format(AppMessage.ERR_CONFIG_DEFAULT_PRESTIGE_POINT, ValidationConstants.MIN_DEFAULT_PRESTIGE_POINT, ValidationConstants.MAX_DEFAULT_PRESTIGE_POINT));
            RuleFor(c => c.MIN_TOPUP)
                .InclusiveBetween(ValidationConstants.MIN_TOPUP_VALID_VALUE, ValidationConstants.MAX_TOPUP_VALID_VALUE)
                .WithMessage(string.Format(AppMessage.ERR_CONFIG_TOPUP_VALUE, ValidationConstants.MIN_TOPUP_VALID_VALUE, ValidationConstants.MAX_TOPUP_VALID_VALUE))
                .Custom((value, context) =>
                {
                    var dto = context.InstanceToValidate;
                    if (value >= dto.MAX_TOPUP)
                    {
                        context.AddFailure(AppMessage.ERR_CONFIG_TOPUP_OUT_OF_RANGE);
                        return;
                    }
                });
            RuleFor(c => c.MAX_TOPUP)
                .InclusiveBetween(ValidationConstants.MIN_TOPUP_VALID_VALUE, ValidationConstants.MAX_TOPUP_VALID_VALUE)
                .WithMessage(string.Format(AppMessage.ERR_CONFIG_TOPUP_VALUE, ValidationConstants.MIN_TOPUP_VALID_VALUE, ValidationConstants.MAX_TOPUP_VALID_VALUE))
                .Custom((value, context) =>
                {
                    var dto = context.InstanceToValidate;
                    if (value <= dto.MIN_TOPUP)
                    {
                        context.AddFailure(AppMessage.ERR_CONFIG_TOPUP_OUT_OF_RANGE);
                        return;
                    }
                });
            RuleFor(c => c.BUDGET_ASSURED_PCT)
                .InclusiveBetween(ValidationConstants.MIN_BUDGET_ASSURED_PCT, ValidationConstants.MAX_BUDGET_ASSURED_PCT)
                .WithMessage(string.Format(AppMessage.ERR_CONFIG_BUDGET_ASSURED_PCT, ValidationConstants.MIN_BUDGET_ASSURED_PCT, ValidationConstants.MAX_BUDGET_ASSURED_PCT));
            RuleFor(c => c.HOLIDAY_MEAL_UP_PCT)
                .InclusiveBetween(ValidationConstants.MIN_HOLIDAY_MEAL_UP_PCT, ValidationConstants.MAX_HOLIDAY_MEAL_UP_PCT)
                .WithMessage(string.Format(AppMessage.ERR_CONFIG_HOLIDAY_MEAL_UP_PCT, ValidationConstants.MIN_HOLIDAY_MEAL_UP_PCT, ValidationConstants.MAX_BUDGET_ASSURED_PCT));
            RuleFor(c => c.HOLIDAY_LODGING_UP_PCT)
                .InclusiveBetween(ValidationConstants.MIN_HOLIDAY_LODGING_UP_PCT, ValidationConstants.MAX_HOLIDAY_LODGING_UP_PCT)
                .WithMessage(string.Format(AppMessage.ERR_CONFIG_HOLIDAY_LODGING_UP_PCT, ValidationConstants.MIN_HOLIDAY_LODGING_UP_PCT, ValidationConstants.MAX_HOLIDAY_LODGING_UP_PCT));
            RuleFor(c => c.HOLIDAY_RIDING_UP_PCT)
                .InclusiveBetween(ValidationConstants.MIN_HOLIDAY_RIDING_UP_PCT, ValidationConstants.MAX_HOLIDAY_RIDING_UP_PCT)
                .WithMessage(string.Format(AppMessage.ERR_CONFIG_HOLIDAY_RIDING_UP_PCT, ValidationConstants.MIN_HOLIDAY_RIDING_UP_PCT, ValidationConstants.MAX_HOLIDAY_RIDING_UP_PCT));
            RuleFor(c => c.ORDER_DATE_MIN_DIFF)
                .InclusiveBetween(ValidationConstants.MIN_ORDER_DATE_MIN_DIFF, ValidationConstants.MAX_ORDER_DATE_MIN_DIFF)
                .WithMessage(string.Format(AppMessage.ERR_CONFIG_ORDER_DATE_MIN_DIFF, ValidationConstants.MIN_ORDER_DATE_MIN_DIFF, ValidationConstants.MAX_ORDER_DATE_MIN_DIFF));
        }
    }
}
