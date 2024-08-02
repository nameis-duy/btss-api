using AppAny.HotChocolate.FluentValidation;
using Application.DTOs.Product;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums.Others;
using HotChocolate.Authorization;
using Infrastructure.Validators.Product;

namespace API.GraphQL.Mutations
{
    public partial class Mutation
    {
        [Authorize(Roles = [nameof(Role.STAFF), nameof(Role.PROVIDER)])]
        public async Task<Product> CreateProductAsync([Service] IProductService productService,
                                                      [UseFluentValidation, UseValidator<ProductCreateValidator>] ProductCreate dto)
        {
            return await productService.CreateProductAsync(dto);
        }

        [Authorize(Roles = [nameof(Role.PROVIDER), nameof(Role.STAFF)])]
        public async Task<Product> ChangeProductStatusAsync([Service] IProductService productService,
                                                            int productId)
        {
            return await productService.ChangeStatusAsync(productId);
        }
        [Authorize(Roles = [nameof(Role.PROVIDER), nameof(Role.STAFF)])]
        public async Task<DateTime> UpdateProductAsync([Service] IProductService productService, [UseFluentValidation, UseValidator<ProductUpdateValidator>] ProductUpdate dto)
        {
            return await productService.UpdateProductAsync(dto);
        }
        [Authorize(Roles = [nameof(Role.STAFF), nameof(Role.PROVIDER)])]
        public async Task<List<Product>> CreateMultiProductsAsync([Service] IProductService productService, [UseFluentValidation, UseValidator<ListProductCreateValidator>] List<ProductCreate> dto)
        {
            return await productService.CreateMultiProductsAsync(dto);
        }
    }
}
