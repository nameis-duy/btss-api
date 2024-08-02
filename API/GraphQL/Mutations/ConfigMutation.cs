using AppAny.HotChocolate.FluentValidation;
using Application.DTOs.Generic;
using Application.Interfaces.Services;
using Domain.Enums.Others;
using HotChocolate.Authorization;
using Infrastructure.Validators.Generic;

namespace API.GraphQL.Mutations
{
    public partial class Mutation
    {
        [Authorize(Roles = [nameof(Role.ADMIN)])]
        public DateTime SetSystemDateTime([Service] IConfigService configService, DateTime dateTime)
        {
            return configService.SetSystemDateTime(dateTime);
        }
        [Authorize(Roles = [nameof(Role.ADMIN)])]
        public DateTime ResetSystemDateTime([Service] IConfigService configService)
        {
            return configService.ResetSystemDateTime();
        }
        [Authorize(Roles = [nameof(Role.ADMIN)])]
        public bool UpdateConfig([Service] IConfigService configService,
                                 [UseFluentValidation, UseValidator<AppConfigValidator>] AppConfig dto)
        {
            return configService.UpdateConfig(dto);
        }
    }
}
