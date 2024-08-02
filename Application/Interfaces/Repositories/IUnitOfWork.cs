namespace Application.Interfaces.Repositories
{
    public interface IUnitOfWork
    {
        IGenericRepo<T> GetRepo<T>() where T : class;
        Task<bool> SaveChangesAsync();
    }
}
