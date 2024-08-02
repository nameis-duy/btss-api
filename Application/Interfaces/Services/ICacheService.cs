namespace Application.Interfaces.Services
{
    public interface ICacheService
    {
        Task<bool> IsKeyExistedAsync(string key);
        Task<T?> GetDataAsync<T>(string key);
        Task<bool> SetDataAsync<T>(string key, T value, int minuteValid);
        Task<bool> RemoveDataAsync(string key);
    }
}
