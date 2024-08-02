using Application.DTOs.Generic;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Infrastructure.Implements.Services
{
    public class GenericService<T> : IGenericService<T> where T : class
    {
        protected readonly IUnitOfWork uow;
        protected readonly AppConfig appConfig;
        protected readonly ITimeService timeService;
        protected readonly IClaimService claimService;
        protected readonly ICacheService cacheService;
        public GenericService(IOptionsSnapshot<AppConfig> configSnapshot,
                              IUnitOfWork uow,
                              ITimeService timeService,
                              IClaimService claimService,
                              ICacheService cacheService)
        {
            appConfig = configSnapshot.Value;
            this.uow = uow;
            this.timeService = timeService;
            this.claimService = claimService;
            this.cacheService = cacheService;
        }

        public Task<T?> FindAsync(params object[] keys)
        {
            return uow.GetRepo<T>().FindAsync(keys);
        }

        public IQueryable<T> GetAll(bool enableTracking = false)
        {
            return uow.GetRepo<T>().GetAll(enableTracking);
        }
    }
}
