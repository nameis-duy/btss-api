using Application.Interfaces.Services;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Infrastructure.Implements.Services
{
    public class CacheService : ICacheService
    {
        private IDatabase db;
        public CacheService(IConnectionMultiplexer muxer)
        {
            db = muxer.GetDatabase();
        }

        public async Task<T?> GetDataAsync<T>(string key)
        {
            var value = await db.StringGetAsync(key);
            if (!string.IsNullOrEmpty(value))
            {
                return JsonConvert.DeserializeObject<T>(value!);
            }
            return default;
        }

        public async Task<bool> IsKeyExistedAsync(string key)
        {
            return await db.KeyExistsAsync(key);
        }

        public async Task<bool> RemoveDataAsync(string key)
        {
            var isExistKey = await IsKeyExistedAsync(key);
            if (isExistKey is true)
            {
                return await db.KeyDeleteAsync(key);
            }
            return false;
        }

        public async Task<bool> SetDataAsync<T>(string key, T value, int minuteValid)
        {
            TimeSpan expiryTime = TimeSpan.FromMinutes(minuteValid);
            return await db.StringSetAsync(key, JsonConvert.SerializeObject(value), expiryTime);
        }
    }
}
