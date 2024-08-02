using Application.DTOs.Generic;
using Application.DTOs.Provider;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums.Others;
using Infrastructure.Constants;
using Infrastructure.Utilities;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Infrastructure.Implements.Services
{
    public class ProviderService : GenericService<Provider>, IProviderService
    {
        public ProviderService(IOptionsSnapshot<AppConfig> configSnapshot,
                               IUnitOfWork uow,
                               ITimeService timeService,
                               IClaimService claimService,
                               ICacheService cacheService) : base(configSnapshot,
                                                                  uow,
                                                                  timeService,
                                                                  claimService,
                                                                  cacheService)
        {
        }
        #region Get providers
        public IQueryable<Provider> GetProviders(string? searchTerm)
        {
            var source = uow.GetRepo<Provider>().GetAll();
            if (searchTerm != null)
            {
                var unaccent = searchTerm.RemoveDiacritics();
                source = unaccent.Length < 5
                    ? source.Where(p => EF.Functions.Unaccent(p.Name).Contains(unaccent))
                    : source.Where(p => EF.Functions.TrigramsAreSimilar(p.Name, unaccent));
            }
            var role = claimService.GetClaim(ClaimTypes.Role, Role.PROVIDER);
            switch (role)
            {
                case Role.PROVIDER:
                    var providerId = claimService.GetClaim(ClaimConstants.PROVIDER_ID, -1);
                    return source.Where(s => s.Id == providerId);
                default:
                    return source;
            }
        }
        #endregion
        #region Create provider
        public async Task<Provider> CreateProviderAsync(ProviderCreate dto)
        {
            var provider = dto.Adapt<Provider>();
            await uow.GetRepo<Provider>().AddAsync(provider);
            if (await uow.SaveChangesAsync()) return provider;
            throw new DbUpdateException(AppMessage.ERR_DB_UPDATE);
        }
        #endregion
        #region Change status
        public async Task<Provider> ChangeStatusAsync(int providerId)
        {
            var provider = await uow.GetRepo<Provider>().FindAsync(providerId)
                ?? throw new KeyNotFoundException(AppMessage.ERR_PROVIDER_NOT_FOUND);
            provider.IsActive = !provider.IsActive;
            if (await uow.SaveChangesAsync()) return provider;
            throw new DbUpdateException(AppMessage.ERR_DB_UPDATE);
        }
        #endregion
        #region Update provider
        public async Task<Provider> UpdateProviderAsync(ProviderUpdate dto)
        {
            var provider = await uow.GetRepo<Provider>().FindAsync(dto.ProviderId)
                           ?? throw new KeyNotFoundException(AppMessage.ERR_PROVIDER_NOT_FOUND);
            dto.Adapt(provider);
            if (await uow.SaveChangesAsync()) return provider;
            throw new DbUpdateException(AppMessage.ERR_DB_UPDATE);
        }
        #endregion
        #region Create multi providers
        public async Task<List<Provider>> CreateMultiProvidersAsync(List<ProviderCreate> dto)
        {
            var providers = dto.Adapt<List<Provider>>();
            await uow.GetRepo<Provider>().AddAsync(providers);
            if (await uow.SaveChangesAsync()) return providers;
            throw new DbUpdateException(AppMessage.ERR_DB_UPDATE);
        }
        #endregion
    }
}
