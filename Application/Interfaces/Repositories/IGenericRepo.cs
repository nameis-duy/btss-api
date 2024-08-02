namespace Application.Interfaces.Repositories
{
    public interface IGenericRepo<T> where T : class
    {
        void Attach(T item);
        Task AddAsync(T item);
        Task AddAsync(IEnumerable<T> items);
        void Update(T item);
        void Remove(T item);
        void Remove(IEnumerable<T> items);
        IQueryable<T> GetAll(bool enableTracking = false);
        Task<T?> FindAsync(params object[] keys);
    }
}
