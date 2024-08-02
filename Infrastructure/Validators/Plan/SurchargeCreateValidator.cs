using Application.DTOs.Plan;
using FluentValidation;
using Infrastructure.Constants;

namespace Infrastructure.Validators.Plan
{
    public class SurchargeCreateValidator : AbstractValidator<SurchargeCreate>
    {
        public SurchargeCreateValidator()
        {
            RuleFor(s => s.Amount).InclusiveBetween(ValidationConstants.PLAN_SURCHARGE_MIN,
                                                         ValidationConstants.PLAN_SURCHARGE_MAX)
                                       .WithMessage(string.Format(AppMessage.ERR_PLAN_SURCHARGE_RANGE,
                                                                  $"{ValidationConstants.PLAN_SURCHARGE_MIN:#,##0}",
                                                                  $"{ValidationConstants.PLAN_SURCHARGE_MAX:#,##0}"));
            RuleFor(s => s.Note).Length(ValidationConstants.PLAN_SURCHARGE_NOTE_MIN_LENGTH,
                                        ValidationConstants.PLAN_SURCHARGE_NOTE_MAX_LENGTH)
                                .WithMessage(string.Format(AppMessage.ERR_PLAN_SURCHARGE_NOTE_LENGTH,
                                                           ValidationConstants.PLAN_SURCHARGE_NOTE_MIN_LENGTH,
                                                           ValidationConstants.PLAN_SURCHARGE_NOTE_MAX_LENGTH));
        }
    }
}
