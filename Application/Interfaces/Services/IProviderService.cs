using Application.DTOs.Provider;
using Domain.Entities;

namespace Application.Interfaces.Services
{
    public interface IProviderService : IGenericService<Provider>
    {
        Task<Provider> CreateProviderAsync(ProviderCreate dto);
        Task<Provider> ChangeStatusAsync(int providerId);
        Task<Provider> UpdateProviderAsync(ProviderUpdate dto);
        IQueryable<Provider> GetProviders(string? searchTerm);
        Task<List<Provider>> CreateMultiProvidersAsync(List<ProviderCreate> dto);
    }
}
