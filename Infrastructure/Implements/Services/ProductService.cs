using Application.DTOs.Generic;
using Application.DTOs.Product;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums.Others;
using Domain.Enums.Provider;
using Infrastructure.Constants;
using Infrastructure.Utilities;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Infrastructure.Implements.Services
{
    public class ProductService : GenericService<Product>, IProductService
    {
        private readonly IBackgroundService backgroundService;
        public ProductService(IOptionsSnapshot<AppConfig> configSnapshot,
                              IUnitOfWork uow,
                              ITimeService timeService,
                              IClaimService claimService,
                              ICacheService cacheService,
                              IBackgroundService backgroundService) : base(configSnapshot,
                                                                 uow,
                                                                 timeService,
                                                                 claimService,
                                                                 cacheService)
        {
            this.backgroundService = backgroundService;
        }
        #region Get products
        public IQueryable<Product> GetProducts(string? searchTerm)
        {
            var source = uow.GetRepo<Product>().GetAll();
            if (searchTerm != null) 
            {
                var unaccent = searchTerm.RemoveDiacritics();
                source = unaccent.Length < 5
                    ? source.Where(p => EF.Functions.Unaccent(p.Name).Contains(unaccent))
                    : source.Where(p => EF.Functions.TrigramsAreSimilar(EF.Functions.Unaccent(p.Name), unaccent));
            }
            var role = claimService.GetClaim(ClaimTypes.Role, Role.TRAVELER);
            switch (role)
            {
                case Role.PROVIDER:
                    var providerId = claimService.GetClaim(ClaimConstants.PROVIDER_ID, -1);
                    return source.Where(p => p.ProviderId == providerId);
                default:
                    return source;
            }
        }
        #endregion
        #region Create product
        public async Task<Product> CreateProductAsync(ProductCreate dto)
        {
            var product = dto.Adapt<Product>();
            await uow.GetRepo<Product>().AddAsync(product);
            if (await uow.SaveChangesAsync()) return product;
            throw new DbUpdateException(AppMessage.ERR_DB_UPDATE);
        }
        #endregion
        #region Change status
        public async Task<Product> ChangeStatusAsync(int productId)
        {
            var product = await uow.GetRepo<Product>()
                                   .GetAll()
                                   .Include(p => p.Provider.Account)
                                   .FirstOrDefaultAsync(o => o.Id == productId)
                          ?? throw new KeyNotFoundException(AppMessage.ERR_PRODUCT_NOT_FOUND);
            var role = claimService.GetClaim(ClaimTypes.Role, Role.PROVIDER);
            switch (role)
            {
                case Role.PROVIDER:
                    var providerId = claimService.GetClaim(ClaimConstants.PROVIDER_ID, -1);
                    if (product.ProviderId != providerId) throw new UnauthorizedAccessException(AppMessage.ERR_AUTHORIZE);
                    break;
                case Role.STAFF:
                    if (product.Provider.Account != null) throw new UnauthorizedAccessException(AppMessage.ERR_AUTHORIZE);
                    break;
            }
            product.IsAvailable = !product.IsAvailable;
            if (await uow.SaveChangesAsync()) return product;
            throw new DbUpdateException(AppMessage.ERR_DB_UPDATE);
        }
        #endregion
        #region Update product
        public async Task<DateTime> UpdateProductAsync(ProductUpdate dto)
        {
            var product = await uow.GetRepo<Product>().FindAsync(dto.ProductId)
                          ?? throw new KeyNotFoundException(AppMessage.ERR_PRODUCT_NOT_FOUND);
            dto.Adapt(product);
            if (product.Type != ProductType.FOOD && product.Type != ProductType.BEVERAGE)
                product.Periods = [Period.MORNING, Period.NOON, Period.AFTERNOON, Period.EVENING];
            var now = timeService.Now;
            var updateAt = new DateTime(now.Year, now.Month + BackgroundConstants.MONTHS_UPDATE_PRODUCT, 1);
            backgroundService.ScheduleProductUpdate(product, updateAt);
            return updateAt;
        }
        #endregion
        #region Create multi products
        public async Task<List<Product>> CreateMultiProductsAsync(List<ProductCreate> dto)
        {
            var products = dto.Adapt<List<Product>>();
            await uow.GetRepo<Product>().AddAsync(products);
            if (await uow.SaveChangesAsync()) return products;
            throw new DbUpdateException(AppMessage.ERR_DB_UPDATE);
        }
        #endregion
    }
}
