using AppAny.HotChocolate.FluentValidation;
using Application.DTOs.Provider;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums.Others;
using HotChocolate.Authorization;
using Infrastructure.Validators.Provider;

namespace API.GraphQL.Mutations
{
    public partial class Mutation
    {
        [Authorize(Roles = [nameof(Role.STAFF)])]
        public async Task<Provider> CreateProviderAsync([Service] IProviderService providerService,
                                                        [UseFluentValidation, UseValidator<ProviderCreateValidator>] ProviderCreate dto)
        {
            return await providerService.CreateProviderAsync(dto);
        }

        [Authorize(Roles = [nameof(Role.STAFF)])]
        public async Task<Provider> ChangeProviderStatusAsync([Service] IProviderService providerService,
                                                              int providerId)
        {
            var provider = await providerService.ChangeStatusAsync(providerId);
            return provider;
        }
        [Authorize(Roles = [nameof(Role.STAFF), nameof(Role.PROVIDER)])]
        public async Task<Provider> UpdateProviderAsync([Service] IProviderService providerService,
                                                        [UseFluentValidation, UseValidator<ProviderUpdateValidator>] ProviderUpdate dto)
        {
            return await providerService.UpdateProviderAsync(dto);
        }
        [Authorize(Roles = [nameof(Role.STAFF)])]
        public async Task<List<Provider>> CreateMultiProvidersAsync([Service] IProviderService providerService, [UseFluentValidation, UseValidator<ListProviderCreateValidator>] List<ProviderCreate> dto)
        {
            return await providerService.CreateMultiProvidersAsync(dto);
        }
    }
}
