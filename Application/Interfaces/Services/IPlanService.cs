using Application.DTOs.Plan;
using Domain.Entities;
using Domain.Enums.Plan;

namespace Application.Interfaces.Services
{
    public interface IPlanService : IGenericService<Plan>
    {
        IQueryable<Plan> GetOwnedPlans();
        IQueryable<Plan> GetJoinedPlans();
        IQueryable<Plan> GetInvitations();
        IQueryable<Plan> GetPublishedPlans();
        IQueryable<Plan> GetScannablePlans();
        Task<Plan> CreatePlanAsync(PlanCreate dto);
        Task<Plan> JoinPlanAsync(PlanJoin dto);
        Task<Plan> UpdateJoinMethodAsync(JoinMethodUpdate dto);
        Task<PlanMember> InviteToPlanAsync(PlanInvite dto);
        Task<PlanMember> RemoveMemberAsync(MemberRemove dto);
        Task<Plan> ConfirmMembersAsync(int planId);
        Task<Plan> CancelPlanAsync(int planId);
        Task<Plan> VerifyPlanAsync(PlanVerify dto);
        Task<Plan> UpdatePlanAsync(PlanUpdate dto);
        Task<Surcharge> UpdateSurchargeAsync(SurchargeUpdate dto);
        Task<Plan> ChangePlanPublishStatusAsync(int planId);
        IQueryable<Plan> GetPlans(string? searchTerm);
    }
}
