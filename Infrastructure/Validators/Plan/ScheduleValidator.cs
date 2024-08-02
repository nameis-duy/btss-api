using Domain.JsonEntities;
using FluentValidation;
using Infrastructure.Constants;
using Infrastructure.Validators.Order;

namespace Infrastructure.Validators.Plan
{
    public class ScheduleEntryValidator : AbstractValidator<List<Event>>
    {
        public ScheduleEntryValidator(EventValidator eventValidator)
        {
            RuleForEach(entry => entry).SetValidator(eventValidator);
        }
    }
    public class EventValidator : AbstractValidator<Event>
    {
        public EventValidator(TempOrderValidator orderValidator)
        {
            RuleFor(e => e.ShortDescription).Length(ValidationConstants.EVENT_SHORT_MIN_LENGTH,
                                                    ValidationConstants.EVENT_SHORT_MAX_LENGTH)
                                            .WithMessage(string.Format(AppMessage.ERR_PLAN_EVENT_SHORT_LENGTH,
                                                                       ValidationConstants.EVENT_SHORT_MIN_LENGTH,
                                                                       ValidationConstants.EVENT_SHORT_MAX_LENGTH));
            RuleFor(e => e.Description).Length(ValidationConstants.EVENT_DESCRIPTION_MIN_LENGTH,
                                               ValidationConstants.EVENT_DESCRIPTION_MAX_LENGTH)
                                       .WithMessage(string.Format(AppMessage.ERR_PLAN_EVENT_DESC_LENGTH,
                                                                  ValidationConstants.EVENT_DESCRIPTION_MIN_LENGTH,
                                                                  ValidationConstants.EVENT_DESCRIPTION_MAX_LENGTH));
            RuleFor(e => e.Type).IsInEnum().WithMessage(AppMessage.ERR_PLAN_EVENT_TYPE_INVALID);
            RuleFor(e => e.TempOrder).SetValidator(orderValidator!).When(e => e.TempOrder != null);
        }
    }
}
