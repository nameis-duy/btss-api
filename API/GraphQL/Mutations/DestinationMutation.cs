using AppAny.HotChocolate.FluentValidation;
using Application.DTOs.Destination;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums.Others;
using HotChocolate.Authorization;
using Infrastructure.Validators.Destination;

namespace API.GraphQL.Mutations
{
    public partial class Mutation
    {
        [Authorize(Roles = [nameof(Role.ADMIN)])]
        public async Task<Destination> CreateDestinationAsync([Service] IDestinationService destinationService,
                                                              [UseFluentValidation, UseValidator<DestinationCreateValidator>] DestinationCreate dto)
        {
            return await destinationService.CreateDestinationAsync(dto);
        }

        [Authorize(Roles = [nameof(Role.ADMIN)])]
        public async Task<Destination> ChangeDestinationStatusAsync([Service] IDestinationService service,
                                                                    int destinationId)
        {
            var dest = await service.ChangeStatusAsync(destinationId);
            return dest;
        }
        [Authorize(Roles = [nameof(Role.ADMIN)])]
        public async Task<Destination> UpdateDestinationAsync([Service] IDestinationService destinationService,
                                                              [UseFluentValidation, UseValidator<DestinationUpdateValidator>] DestinationUpdate dto)
        {
            return await destinationService.UpdateDestinationAsync(dto);
        }
        [Authorize(Roles = [nameof(Role.ADMIN)])]
        public async Task<List<Destination>> CreateMultiDestinationAsync([Service] IDestinationService destinationService,
                                                                         [UseFluentValidation, UseValidator<ListDestinationCreateValidator>] List<DestinationCreate> dtos)
        {
            return await destinationService.CreateMultiDestinationAsync(dtos);
        }
        [Authorize(Roles = [nameof(Role.TRAVELER)])]
        public async Task<DestinationComment> AddDestinationCommentAsync([Service] IDestinationService destinationService,
                                                                         [UseFluentValidation, UseValidator<DestinationCommentCreateValidator>] DestinationCommentCreate dto)
        {
            return await destinationService.AddDestinationCommentAsync(dto);
        }
    }
}
