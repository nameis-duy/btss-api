﻿using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums.Others;
using HotChocolate.Authorization;
using Infrastructure.Constants;
using System.Security.Claims;

namespace API.GraphQL.Queries
{
    public partial class Query
    {
        [UsePaging(MaxPageSize = 100, IncludeTotalCount = true)]
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        [Authorize(Roles = [nameof(Role.TRAVELER), nameof(Role.PROVIDER), nameof(Role.STAFF)])]
        public IQueryable<Product> GetProducts([Service] IProductService productService, string? searchTerm = null)
        {
            return productService.GetProducts(searchTerm);
        }
    }
}