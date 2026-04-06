using StackExchange.Redis;

namespace WebsupplyConnect.Application.Interfaces.ExternalServices
{
    public interface IRedisCacheService
    {
        Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);
        Task<bool> SetStringAsync(string key, string value, TimeSpan? expiry = null);
        Task<string?> GetStringAsync(string key);
        Task RunAtomicAsync(Func<ITransaction, Task> action);
        Task<T?> GetAsync<T>(string key);
        Task RemoveAsync(string key);
    }
}
