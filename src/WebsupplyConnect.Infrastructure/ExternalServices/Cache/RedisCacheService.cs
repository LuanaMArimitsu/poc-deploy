using Microsoft.Extensions.Options;
using WebsupplyConnect.Application.Configuration;
using WebsupplyConnect.Application.Interfaces.ExternalServices;
using WebsupplyConnect.Infrastructure.Exceptions;
using StackExchange.Redis;
using System.Text.Json;

namespace WebsupplyConnect.Infrastructure.ExternalServices.Cache;
public class RedisCacheService(IConnectionMultiplexer redis, IOptions<RedisConfiguration> config) : IRedisCacheService
{
    private readonly IDatabase _cacheDb = redis.GetDatabase();
    private readonly TimeSpan _defaultExpiry = TimeSpan.FromDays(config.Value.CacheExpirationInDays);

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        var json = JsonSerializer.Serialize(value);
        var sucesso = await _cacheDb.StringSetAsync(key, json, expiry ?? _defaultExpiry);

        if (!sucesso)
            throw new InfraException($"Erro ao salvar o valor da chave '{key}' no Redis.");
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var json = await _cacheDb.StringGetAsync(key);


        if (json.IsNullOrEmpty)
            return default;

        var value = JsonSerializer.Deserialize<T>(json);

        return value is null ? throw new InfraException($"Não foi possível desserializar o valor da chave '{key}'.") : value;
    }

    public async Task<bool> SetStringAsync(string key, string value, TimeSpan? expiry = null)
    {
        return await _cacheDb.StringSetAsync(key, value, expiry ?? _defaultExpiry);
    }

    public async Task<string?> GetStringAsync(string key)
    {
        var value = await _cacheDb.StringGetAsync(key);
        return value.IsNullOrEmpty ? null : value.ToString();
    }

    public async Task RunAtomicAsync(Func<ITransaction, Task> action)
    {
        var tran = _cacheDb.CreateTransaction();

        await action(tran);

        var committed = await tran.ExecuteAsync();
        if (!committed)
        {
            throw new InfraException("Falha ao executar transação no Redis.");
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await _cacheDb.KeyDeleteAsync(key);
        }
        catch (Exception ex)
        {
            throw new InfraException($"Erro ao tentar remover a chave '{key}' do Redis.", ex);
        }
    }
}