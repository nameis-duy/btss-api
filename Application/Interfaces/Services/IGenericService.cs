namespace Application.Interfaces.Services
{
    public interface IGenericService<T> where T : class
    {
        IQueryable<T> GetAll(bool enableTracking = false);
        Task<T?> FindAsync(params object[] keys);
    }
}
