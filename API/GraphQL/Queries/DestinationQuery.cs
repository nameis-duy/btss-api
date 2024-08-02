using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums.Others;
using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace API.GraphQL.Queries
{
    public partial class Query
    {
        [UsePaging(MaxPageSize = 100, IncludeTotalCount = true)]
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        [Authorize(Roles = [nameof(Role.TRAVELER), nameof(Role.ADMIN)])]
        public IQueryable<Destination> GetDestinations([Service] IDestinationService destinationService,
                                                       string? searchTerm = null)
        {
            return destinationService.GetDestinations(searchTerm);
        }
        [UsePaging(MaxPageSize = 100, IncludeTotalCount = true)]
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        [Authorize(Roles = [nameof(Role.TRAVELER), nameof(Role.ADMIN)])]
        public IQueryable<Province> GetProvinces([Service] IGenericService<Province> provinceService)
        {
            return provinceService.GetAll();
        }
        [UsePaging(MaxPageSize = 100, IncludeTotalCount = true)]
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        [Authorize(Roles = [nameof(Role.TRAVELER), nameof(Role.ADMIN)])]
        public async Task<IQueryable<Destination>> GetTrendingDestinations([Service] IDestinationService destinationService)
        {
            return await destinationService.GetTrendingDestinationsAsync();
        }
        //[UsePaging(MaxPageSize = 100, IncludeTotalCount = true)]
        //[UseProjection]
        //[UseFiltering]
        //[UseSorting]
        //[Authorize(Roles = [nameof(Role.TRAVELER), nameof(Role.ADMIN)])]
        //public async Task<IQueryable<Destination>> GetMonthlyTrendingDestinations([Service] IDestinationService destinationService,
        //                                                                          [UseFluentValidation, UseValidator<DestinationTrendingRequestValidator>] DestinationTrendingRequest dto)
        //{
        //    var source = await destinationService.GetMonthlyTrendingDestinations(dto.Month, dto.Year);
        //    return source;
        //}
    }
}
