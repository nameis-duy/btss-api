using Application.DTOs.Destination;
using Application.DTOs.Generic;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums.Others;
using Infrastructure.Constants;
using Infrastructure.Utilities;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NetTopologySuite.Noding;
using Newtonsoft.Json;

namespace Infrastructure.Implements.Services
{
    public class DestinationService : GenericService<Destination>, IDestinationService
    {
        public DestinationService(IOptionsSnapshot<AppConfig> configSnapshot,
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
        #region Get destinations
        public IQueryable<Destination> GetDestinations(string? searchTerm)
        {
            var source = uow.GetRepo<Destination>().GetAll();
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var unaccent = searchTerm.RemoveDiacritics();
                source = source.Where(l => l.NameVector.Matches(searchTerm) ||
                                           EF.Functions.TrigramsAreSimilar(l.UnaccentName, unaccent));
            }    
            return source;
        }
        public async Task<IQueryable<Destination>> GetTrendingDestinationsAsync()
        {
            var key = BackgroundConstants.TRENDING_DESTINATIONS;
            var data = await uow.GetRepo<StatisticalData>().GetAll().FirstOrDefaultAsync(d => d.Key == key);
            var source = uow.GetRepo<Destination>().GetAll();
            if (data == null) return source;
            var dict = JsonConvert.DeserializeObject<Dictionary<int, int>>(data.Value);
            if (dict == null) return source;
            return source.Where(d => dict.Keys.Contains(d.Id));
        }
        #endregion
        #region Create destination
        public async Task<Destination> CreateDestinationAsync(DestinationCreate dto)
        {
            var destination = dto.Adapt<Destination>();
            await uow.GetRepo<Destination>().AddAsync(destination);
            if (await uow.SaveChangesAsync()) return destination;
            throw new DbUpdateException(AppMessage.ERR_DB_UPDATE);
        }
        #endregion
        #region Change status
        public async Task<Destination> ChangeStatusAsync(int id)
        {
            var dest = await uow.GetRepo<Destination>().FindAsync(id) 
                ?? throw new KeyNotFoundException(AppMessage.ERR_DESTINATION_NOT_FOUND);
            dest.IsVisible = !dest.IsVisible;
            if (await uow.SaveChangesAsync()) return dest;
            throw new DbUpdateException(AppMessage.ERR_DB_UPDATE);
        }
        #endregion
        #region Update destination
        public async Task<Destination> UpdateDestinationAsync(DestinationUpdate dto)
        {
            var destination = await uow.GetRepo<Destination>().FindAsync(dto.DestinationId)
                              ?? throw new KeyNotFoundException(AppMessage.ERR_DESTINATION_NOT_FOUND);
            dto.Adapt(destination);
            if (await uow.SaveChangesAsync()) return destination;
            throw new DbUpdateException(AppMessage.ERR_DB_UPDATE);
        }
        #endregion
        #region Create multi destination
        public async Task<List<Destination>> CreateMultiDestinationAsync(List<DestinationCreate> dto)
        {
            var destinations = dto.Adapt<List<Destination>>();
            await uow.GetRepo<Destination>().AddAsync(destinations);
            if (await uow.SaveChangesAsync()) return destinations;
            throw new DbUpdateException(AppMessage.ERR_DB_UPDATE);
        }
        #endregion
        #region Add destination comment
        public async Task<DestinationComment> AddDestinationCommentAsync(DestinationCommentCreate dto)
        {
            var comment = dto.Adapt<DestinationComment>();
            var accountId = claimService.GetClaim(ClaimConstants.ID, -1);
            comment.AccountId = accountId;
            comment.CreatedAt = timeService.Now;
            await uow.GetRepo<DestinationComment>().AddAsync(comment);
            if (await uow.SaveChangesAsync()) return comment;
            throw new DbUpdateException(AppMessage.ERR_DB_UPDATE);
        }
        #endregion
    }
}
