using Application.DTOs.Product;
using Domain.Entities;

namespace Application.Interfaces.Services
{
    public interface IProductService : IGenericService<Product>
    {
        Task<Product> CreateProductAsync(ProductCreate dto);
        IQueryable<Product> GetProducts(string? searchTerm);
        Task<Product> ChangeStatusAsync(int productId);
        Task<DateTime> UpdateProductAsync(ProductUpdate dto);
        Task<List<Product>> CreateMultiProductsAsync(List<ProductCreate> dto);
    }
}
