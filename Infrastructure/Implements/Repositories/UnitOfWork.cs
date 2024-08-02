using Application.Interfaces.Repositories;

namespace Infrastructure.Implements.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext context;
        private readonly IServiceProvider serviceProvider;
        public UnitOfWork(IServiceProvider serviceProvider, AppDbContext context)
        {
            this.serviceProvider = serviceProvider;
            this.context = context;
        }

        public IGenericRepo<T> GetRepo<T>() where T : class
        {
            return (IGenericRepo<T>) serviceProvider.GetService(typeof(IGenericRepo<T>))!;
        }

        public async Task<bool> SaveChangesAsync()
        {
            var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                var affected = await context.SaveChangesAsync();
                transaction.Commit();
                return affected > 0;
            }
            catch
            {
                transaction.Rollback();
                return false;
            }
        }
    }
}
