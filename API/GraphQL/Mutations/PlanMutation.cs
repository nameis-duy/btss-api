using AppAny.HotChocolate.FluentValidation;
using Application.DTOs.Plan;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums.Others;
using HotChocolate.Authorization;
using Infrastructure.Implements.Services;
using Infrastructure.Validators.Plan;

namespace API.GraphQL.Mutations
{
    public partial class Mutation
    {
        [Authorize(Roles = [nameof(Role.TRAVELER)])]
        public async Task<Plan> CreatePlanAsync([Service] IPlanService planService,
                                                [UseFluentValidation, UseValidator<PlanCreateValidator>] PlanCreate dto)
        {
            return await planService.CreatePlanAsync(dto);
        }
        [Authorize(Roles = [nameof(Role.TRAVELER)])]
        public async Task<Plan> JoinPlanAsync([Service] IPlanService planService,
                                              [UseFluentValidation, UseValidator<PlanJoinValidator>] PlanJoin dto)
        {
            return await planService.JoinPlanAsync(dto);
        }
        [Authorize(Roles = [nameof(Role.TRAVELER)])]
        public async Task<Plan> UpdateJoinMethodAsync([Service] IPlanService planService,
                                                      [UseFluentValidation, UseValidator<JoinMethodUpdateValidator>] JoinMethodUpdate dto)
        {
            return await planService.UpdateJoinMethodAsync(dto);
        }
        [Authorize(Roles = [nameof(Role.TRAVELER)])]
        public async Task<PlanMember> InviteToPlanAsync([Service] IPlanService planService,
                                                        [UseFluentValidation, UseValidator<PlanInviteValidator>] PlanInvite dto)
        {
            return await planService.InviteToPlanAsync(dto);
        }
        [Authorize(Roles = [nameof(Role.TRAVELER)])]
        public async Task<PlanMember> RemoveMemberAsync([Service] IPlanService planService,
                                                        [UseFluentValidation, UseValidator<MemberRemoveValidator>] MemberRemove dto)
        {
            return await planService.RemoveMemberAsync(dto);
        }
        [Authorize(Roles = [nameof(Role.TRAVELER)])]
        public async Task<Plan> ConfirmMembersAsync([Service] IPlanService planService, int planId)
        {
            return await planService.ConfirmMembersAsync(planId);
        }
        [Authorize(Roles = [nameof(Role.TRAVELER)])]
        public async Task<Plan> CancelPlanAsync([Service] IPlanService planService, int planId)
        {
            return await planService.CancelPlanAsync(planId);
        }
        [Authorize(Roles = [nameof(Role.TRAVELER)])]
        public async Task<Plan> VerifyPlanAsync([Service] IPlanService planService,
                                                [UseFluentValidation, UseValidator<PlanVerifyValidator>] PlanVerify dto)
        {
            return await planService.VerifyPlanAsync(dto);
        }
        [Authorize(Roles = [nameof(Role.TRAVELER)])]
        public async Task<Plan> UpdatePlanAsync([Service] IPlanService planService, [UseFluentValidation, UseValidator<PlanUpdateValidator>] PlanUpdate dto)
        {
            return await planService.UpdatePlanAsync(dto);
        }
        [Authorize(Roles = [nameof(Role.TRAVELER)])]
        public async Task<Surcharge> UpdateSurchargeAsync([Service] IPlanService planService, [UseFluentValidation, UseValidator<SurchargeUpdateValidator>] SurchargeUpdate dto)
        {
            return await planService.UpdateSurchargeAsync(dto);
        }
        [Authorize(Roles = [nameof(Role.TRAVELER)])]
        public async Task<Plan> ChangePlanPublishStatusAsync([Service] IPlanService planService, int planId)
        {
            return await planService.ChangePlanPublishStatusAsync(planId);
        }

    }
}
