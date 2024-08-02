using Application.DTOs.Destination;
using Domain.Entities;

namespace Application.Interfaces.Services
{
    public interface IDestinationService : IGenericService<Destination>
    {
        IQueryable<Destination> GetDestinations(string? searchTerm);
        Task<Destination> CreateDestinationAsync(DestinationCreate dto);
        Task<Destination> ChangeStatusAsync(int destinationId);
        Task<Destination> UpdateDestinationAsync(DestinationUpdate dto);
        Task<List<Destination>> CreateMultiDestinationAsync(List<DestinationCreate> dtos);
        Task<DestinationComment> AddDestinationCommentAsync(DestinationCommentCreate dto);
        Task<IQueryable<Destination>> GetTrendingDestinationsAsync();
    }
}
