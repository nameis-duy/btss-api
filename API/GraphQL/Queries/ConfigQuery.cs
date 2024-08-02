using Application.DTOs.Generic;
using Domain.Enums.Others;
using HotChocolate.Authorization;
using Microsoft.Extensions.Options;

namespace API.GraphQL.Queries
{
    public partial class Query
    {
        [Authorize(Roles = [nameof(Role.TRAVELER), nameof(Role.ADMIN)])]
        public AppConfig GetConfigurations([Service] IOptionsSnapshot<AppConfig> snapshot)
        {
            return snapshot.Value;
        }
        [Authorize(Roles = [nameof(Role.ADMIN)])]
        public List<string> GetConfigurationsName([Service] IOptionsSnapshot<AppConfig> snapshot)
        {
            return snapshot.Value.GetType().GetProperties().Select(p => p.Name).ToList();
        }
    }
}
