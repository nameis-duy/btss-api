using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums.Others;
using Domain.Enums.Plan;
using HotChocolate.Authorization;
using Infrastructure.Constants;
using System.Linq.Expressions;
using System.Security.Claims;

namespace API.GraphQL.Queries
{
    public partial class Query
    {
        [UsePaging(MaxPageSize = 100, IncludeTotalCount = true)]
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        [Authorize(Roles = [nameof(Role.ADMIN)])]
        public IQueryable<Plan> GetPlans([Service] IPlanService planService, string? searchTerm = null)
        {
            return planService.GetPlans(searchTerm);
        }
        [UsePaging(MaxPageSize = 100, IncludeTotalCount = true)]
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        [Authorize(Roles = [nameof(Role.TRAVELER)])]
        public IQueryable<Plan> GetJoinedPlans([Service] IPlanService planService)
        {
            return planService.GetJoinedPlans();
        }
        [UsePaging(MaxPageSize = 100, IncludeTotalCount = true)]
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        [Authorize(Roles = [nameof(Role.TRAVELER)])]
        public IQueryable<Plan> GetOwnedPlans([Service] IPlanService planService)
        {
            return planService.GetOwnedPlans();
        }
        [UsePaging(MaxPageSize = 100, IncludeTotalCount = true)]
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        [Authorize(Roles = [nameof(Role.TRAVELER)])]
        public IQueryable<Plan> GetInvitations([Service] IPlanService planService)
        {
            return planService.GetInvitations();
        }
        [UsePaging(MaxPageSize = 100, IncludeTotalCount = true)]
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        [Authorize(Roles = [nameof(Role.TRAVELER)])]
        public IQueryable<Plan> GetScannablePlans([Service] IPlanService planService)
        {
            return planService.GetScannablePlans();
        }
        [UsePaging(MaxPageSize = 100, IncludeTotalCount = true)]
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        [Authorize(Roles = [nameof(Role.TRAVELER)])]
        public IQueryable<Plan> GetPublishedPlans([Service] IPlanService planService)
        {
            return planService.GetPublishedPlans();
        }
    }
}
