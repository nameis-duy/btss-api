using Application.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Implements.Repositories
{
    public class GenericRepo<T> : IGenericRepo<T> where T : class
    {
        protected readonly AppDbContext context;

        public GenericRepo(AppDbContext context)
        {
            this.context = context;
        }

        public async Task AddAsync(T item)
        {
            await context.AddAsync(item);
        }

        public async Task AddAsync(IEnumerable<T> items)
        {
            await context.AddRangeAsync(items);
        }

        public void Attach(T item)
        {
            context.Attach(item);
        }

        public async Task<T?> FindAsync(params object[] keys)
        {
            return await context.FindAsync<T>(keys);
        }

        public IQueryable<T> GetAll(bool enableTracking = false)
        {
            if (enableTracking) return context.Set<T>().AsQueryable();
            return context.Set<T>().AsNoTrackingWithIdentityResolution();
        }

        public void Remove(T item)
        {
            context.Remove(item);
        }

        public void Remove(IEnumerable<T> items)
        {
            context.RemoveRange(items);
        }

        public void Update(T item)
        {
            context.Update(item);
        }
    }
}
