using Application.DTOs.Plan;
using Application.Interfaces.Services;
using Domain.Enums.Provider;
using FluentValidation;
using Infrastructure.Constants;
using Infrastructure.Utilities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Validators.Plan
{
    public class PlanCreateValidator : AbstractValidator<PlanCreate>
    {
        public PlanCreateValidator(ITimeService timeService,
                                   IDestinationService destinationService,
                                   IPlanService planService,
                                   IProviderService providerService,
                                   ICacheService cacheService,
                                   IClaimService claimService,
                                   ScheduleEntryValidator entryValidator,
                                   SurchargeCreateValidator surchargeValidator)
        {
            RuleFor(p => p.Name).Length(ValidationConstants.PLAN_NAME_MIN_LENGTH,
                                        ValidationConstants.PLAN_NAME_MAX_LENGTH)
                                .WithMessage(string.Format(AppMessage.ERR_PLAN_NAME_LENGTH,
                                                           ValidationConstants.PLAN_NAME_MIN_LENGTH,
                                                           ValidationConstants.PLAN_NAME_MAX_LENGTH));
            var now = timeService.Now;
            RuleFor(p => p.DepartAt).Custom((localDepartAt, context) =>
            {
                var isPersonalPlan = context.InstanceToValidate.MaxMemberCount == 1;
                var minUtcDepartAt = now.AddDays(isPersonalPlan ? ValidationConstants.PLAN_PERSONAL_DEPART_DATE_MIN_DIFF
                                                                : ValidationConstants.PLAN_DEPART_DATE_MIN_DIFF);
                var maxUtcDepartAt = now.AddDays(ValidationConstants.PLAN_DEPART_DATE_MAX_DIFF);
                if (localDepartAt.UtcDateTime > maxUtcDepartAt || localDepartAt.UtcDateTime < minUtcDepartAt)
                {
                    context.AddFailure(string.Format(AppMessage.ERR_PLAN_DEPART_DATE,
                                                     ValidationConstants.PLAN_DEPART_DATE_MIN_DIFF,
                                                     ValidationConstants.PLAN_DEPART_DATE_MAX_DIFF));
                    return;
                }
            });
            When(p => p.MaxMemberCount == 1, () =>
            {
                RuleFor(p => p.MaxMemberWeight).Equal(1).WithMessage(AppMessage.ERR_PLAN_PERSONAL_MEMBER_WEIGHT);
            }).Otherwise(() =>
            {

                RuleFor(p => p.MaxMemberCount).InclusiveBetween(ValidationConstants.PLAN_GROUP_MEMBER_MIN,
                                                                ValidationConstants.PLAN_GROUP_MEMBER_MAX)
                                              .WithMessage(string.Format(AppMessage.ERR_PLAN_MEMBER_RANGE,
                                                                         ValidationConstants.PLAN_GROUP_MEMBER_MIN,
                                                                         ValidationConstants.PLAN_GROUP_MEMBER_MAX));
                When(p => p.MaxMemberCount >= ValidationConstants.PLAN_MEMBER_WEIGHT_CAL_FLOOR, () =>
                {
                    RuleFor(p => p.MaxMemberWeight).GreaterThanOrEqualTo(ValidationConstants.PLAN_MEMBER_WEIGHT_MIN)
                                                   .WithMessage(string.Format(AppMessage.ERR_PLAN_MEMBER_MIN_WEIGHT,
                                                                              ValidationConstants.PLAN_MEMBER_WEIGHT_MIN - 1))
                                                   .Custom((weight, context) =>
                                                   {
                                                       var plan = context.InstanceToValidate;
                                                       var maxWeight = (int)Math.Floor(ValidationConstants.PLAN_MEMBER_WEIGHT_PCT * plan.MaxMemberCount);
                                                       if (weight > maxWeight)
                                                       {
                                                           context.AddFailure(string.Format(AppMessage.ERR_PLAN_MEMBER_MAX_WEIGHT, maxWeight));
                                                           return;
                                                       }
                                                   }); ;
                }).Otherwise(() =>
                {
                    RuleFor(p => p.MaxMemberWeight).Equal(ValidationConstants.PLAN_MEMBER_FIXED_WEIGHT)
                                                   .WithMessage(string.Format(AppMessage.ERR_PLAN_FIXED_MEMBER_WEIGHT,
                                                                              ValidationConstants.PLAN_MEMBER_WEIGHT_CAL_FLOOR,
                                                                              ValidationConstants.PLAN_MEMBER_FIXED_WEIGHT));
                });
            });
            //RuleFor(p => p.TravelDuration).InclusiveBetween()
            RuleFor(p => p.PeriodCount).InclusiveBetween(ValidationConstants.PLAN_PERIOD_MIN,
                                                         ValidationConstants.PLAN_PERIOD_MAX)
                                       .WithMessage(string.Format(AppMessage.ERR_PLAN_PERIOD,
                                                                  ValidationConstants.PLAN_PERIOD_MIN / 2,
                                                                  ValidationConstants.PLAN_PERIOD_MAX / 2));
            RuleFor(p => p.DepartureAddress).Length(ValidationConstants.ADDRESS_MIN_LENGTH,
                                                 ValidationConstants.ADDRESS_MAX_LENGTH)
                                         .WithMessage(string.Format(AppMessage.ERR_ADDRESS_LENGTH,
                                                                    ValidationConstants.ADDRESS_MIN_LENGTH,
                                                                    ValidationConstants.ADDRESS_MAX_LENGTH));
            RuleFor(p => p.Departure).IsInsideGeometry(ValidationConstants.REGION)
                                     .WithMessage(AppMessage.ERR_COORDINATE_INVALID);

            RuleFor(p => p.Schedule).Custom((schedule, context) =>
            {
                var plan = context.InstanceToValidate;
                var arrivedAt = plan.DepartAt.Add(plan.TravelDuration);
                var arrivalTime = arrivedAt.TimeOfDay;
                var arrivedAtNight = arrivalTime >= ValidationConstants.HALF_EVENING;
                var arrivedAtEvening = !arrivedAtNight && arrivalTime >= ValidationConstants.HALF_AFTERNOON;
                var startAt = arrivedAtNight ? arrivedAt.Date.AddDays(1).Add(GlobalConstants.MORNING_START) : arrivedAt;
                var dayEqualNight = plan.PeriodCount % 2 == 0;
                var maxDateLength = (int)Math.Ceiling(plan.PeriodCount * 1.0 / 2);
                var isEndAtNoon = (arrivedAtEvening && dayEqualNight) || (!arrivedAtEvening && !dayEqualNight);
                var endAt = startAt.AddDays(maxDateLength - (arrivedAtEvening && dayEqualNight ? 0 : 1)).Date
                                   .Add(isEndAtNoon ? GlobalConstants.AFTERNOON_START : GlobalConstants.EVENING_END);
                var maxIndex = (endAt.Date - startAt.Date).TotalDays;
                if (schedule.Count != maxIndex + 1)
                {
                    context.AddFailure(AppMessage.ERR_PLAN_SCHEDULE_LENGTH);
                    return;
                }
                if (!arrivedAtNight)
                {
                    var minStartHour = arrivalTime > GlobalConstants.MORNING_START ? arrivalTime : GlobalConstants.MORNING_START;
                    var maxFirstDayDuration = GlobalConstants.EVENING_END - minStartHour;
                    var duration = TimeSpan.Zero;
                    foreach (var ev in schedule[0])
                    {
                        duration += ev.Duration;
                        if (ev.TempOrder == null || ev.TempOrder.Period >= startAt.TimeOfDay.GetPeriod()) continue;
                        context.AddFailure(AppMessage.ERR_EVENT_ORDER_PERIOD);
                        return;
                    }
                    if (duration > maxFirstDayDuration)
                    {
                        context.AddFailure(AppMessage.ERR_PLAN_SCHEDULE_DURATION_FIRST);
                        return;
                    }
                }
                if (isEndAtNoon)
                {
                    if (schedule[^1].Any(e => e.TempOrder != null && e.TempOrder.Period >= Period.AFTERNOON))
                    {
                        context.AddFailure(AppMessage.ERR_EVENT_ORDER_PERIOD);
                        return;
                    }
                    if (schedule[^1].Sum(e => e.Duration.TotalSeconds) > ValidationConstants.HALF_DAY_LENGTH.TotalSeconds)
                    {
                        context.AddFailure(AppMessage.ERR_PLAN_SCHEDULE_DURATION_LAST);
                        return;
                    }
                }
                var i = !arrivedAtNight ? 1 : 0;
                var lastFullDayIndex = schedule.Count - (isEndAtNoon ? 2 : 1);
                for (; i <= lastFullDayIndex; i++)
                {
                    TimeSpan totalDuration = TimeSpan.Zero;
                    for (int j = 0; j < schedule[i].Count; j++)
                    {
                        totalDuration += schedule[i][j].Duration;
                        if (schedule[i][j].Duration >= ValidationConstants.EVENT_MIN_DURATION
                            && schedule[i][j].Duration <= ValidationConstants.EVENT_MAX_DURATION)
                            continue;
                        context.AddFailure($"{nameof(PlanCreate.Schedule)}[{i}][{j}]",
                                           string.Format(AppMessage.ERR_PLAN_EVENT_DURATION,
                                                         $"{ValidationConstants.EVENT_MIN_DURATION:hh\\:mm}",
                                                         $"{ValidationConstants.FULL_DAY_LENGTH:hh\\:mm}"));
                        return;
                    }
                    if (totalDuration < ValidationConstants.FULL_DAY_LENGTH) continue;
                    context.AddFailure($"{nameof(PlanCreate.Schedule)}[{i}]",
                                       string.Format(AppMessage.ERR_PLAN_SCHEDULE_DURATION_MID,
                                                     $"{ValidationConstants.FULL_DAY_LENGTH:hh\\:mm}"));
                    return;

                }
            });
            RuleForEach(p => p.Schedule).SetValidator(entryValidator);
            RuleFor(p => p.SavedProviderIds).CustomAsync(async (providerIds, context, ct) =>
            {
                var providers = await providerService.GetAll()
                                                     .Where(p => providerIds.Contains(p.Id) && p.IsActive)
                                                     .ToListAsync(ct);
                if (providerIds.Count != providers.Count)
                {
                    context.AddFailure(AppMessage.ERR_PROVIDER_NOT_FOUND_SOME);
                    return;
                }
                if (!providers.Any(p => p.Type == ProviderType.EMERGENCY))
                {
                    context.AddFailure(AppMessage.ERR_PLAN_CONTACT_EMERGENCY);
                    return;
                }
                var dto = context.InstanceToValidate;
                var destination = await destinationService.FindAsync(dto.DestinationId);
                if (destination == null || !destination.IsVisible)
                {
                    context.AddFailure(nameof(PlanCreate.DestinationId), AppMessage.ERR_DESTINATION_NOT_FOUND);
                    return;
                }
                if (providers.Any(p => p.Type != ProviderType.EMERGENCY
                                       && !p.Coordinate.IsWithinHaversineDistance(destination.Coordinate,
                                                                                  ValidationConstants.PROVIDER_MAX_DISTANCE_METER_DIFF)))
                {
                    context.AddFailure(string.Format(AppMessage.ERR_PLAN_PROVIDER_DISTANCE,
                                                     ValidationConstants.PROVIDER_MAX_DISTANCE_METER_DIFF));
                    return;
                }
                if (providers.Any(p => p.Type == ProviderType.EMERGENCY
                                       && !p.Coordinate.IsWithinHaversineDistance(destination.Coordinate,
                                                                                  ValidationConstants.EMERGENCY_MAX_DISTANCE_METER_DIFF)))
                {
                    context.AddFailure(string.Format(AppMessage.ERR_PLAN_EMERGENCY_DISTANCE,
                                                     ValidationConstants.EMERGENCY_MAX_DISTANCE_METER_DIFF));
                    return;
                }
                //var key = string.Format(CacheConstants.PLAN_PROVIDERS_FORMAT, claimService.GetUniqueRequestId());
                //await cacheService.SetDataAsync(key, providers, CacheConstants.DEFAULT_VALID_MINUTE);
            }).Count(ValidationConstants.PLAN_CONTACT_MIN_COUNT,
                     ValidationConstants.PLAN_CONTACT_MAX_COUNT)
              .WithMessage(string.Format(AppMessage.ERR_PLAN_CONTACT_COUNT,
                                         ValidationConstants.PLAN_CONTACT_MIN_COUNT,
                                         ValidationConstants.PLAN_CONTACT_MAX_COUNT));
            RuleFor(p => p.Surcharges).MaxCount(ValidationConstants.PLAN_SURCHARGE_MAX_COUNT)
                                      .WithMessage(string.Format(AppMessage.ERR_PLAN_SURCHARGE_MAX_COUNT, ValidationConstants.PLAN_SURCHARGE_MAX_COUNT))
                                      .ForEach(s => s.SetValidator(surchargeValidator));
            RuleFor(p => p.Note).MaximumLength(ValidationConstants.PLAN_NOTE_MAX_LENGTH)
                                .WithMessage(string.Format(AppMessage.ERR_PLAN_NOTE_LENGTH,
                                                           ValidationConstants.PLAN_NOTE_MAX_LENGTH));
            RuleFor(p => p.SourceId).MustAsync(async (plan, sourceId, ct) =>
            {
                return await planService.GetAll().AnyAsync(p => p.Id == sourceId
                                                                && p.IsPublished
                                                                && p.DestinationId == plan.DestinationId, ct);
            }).WithMessage(AppMessage.ERR_PLAN_SOURCE_NOT_FOUND).When(p => p.SourceId.HasValue);
        }
    }
}
