﻿using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums.Others;
using HotChocolate.Authorization;

namespace API.GraphQL.Queries
{
    public partial class Query
    {
        [UsePaging(MaxPageSize = 100, IncludeTotalCount = true)]
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        [Authorize(Roles = [nameof(Role.TRAVELER), nameof(Role.PROVIDER), nameof(Role.STAFF), nameof(Role.ADMIN)])]
        public IQueryable<Announcement> GetAnnouncements([Service] IAnnouncementService announcementService)
        {
            return announcementService.GetAnnouncements();
        }
    }
}