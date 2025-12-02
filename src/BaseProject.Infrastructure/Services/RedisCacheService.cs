using BaseProject.Application.Abstractions;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System.Text.Json;

namespace BaseProject.Infrastructure.Services;

public sealed class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _distributedCache;
    private readonly IConnectionMultiplexer? _connectionMultiplexer;
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerOptions.Default)
    {
        PropertyNamingPolicy = null,
        WriteIndented = false
    };

    public RedisCacheService(IDistributedCache distributedCache, IConnectionMultiplexer? connectionMultiplexer = null)
    {
        _distributedCache = distributedCache;
        _connectionMultiplexer = connectionMultiplexer;
    }

    public async Task Add(string key, object data, DateTimeOffset? absExpr, TimeSpan? sldExpr)
    {
        if (data is null)
        {
            return;
        }

        var cacheEntryOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpiration = absExpr,
            SlidingExpiration = sldExpr
        };

        string json = JsonSerializer.Serialize(data, SerializerOptions);
        await _distributedCache.SetStringAsync(key, json, cacheEntryOptions);
    }

    public async Task<bool> AnyAsync(string key)
    {
        var data = await _distributedCache.GetStringAsync(key);
        return !string.IsNullOrEmpty(data);
    }

    public async Task<T?> Get<T>(string key)
    {
        var data = await _distributedCache.GetStringAsync(key);
        if (string.IsNullOrEmpty(data))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(data, SerializerOptions);
    }

    public async Task Remove(string key)
    {
        await _distributedCache.RemoveAsync(key);
    }

    /// <summary>
    /// Redis'e key ekler, ancak sadece key yoksa (SETNX - SET if Not eXists).
    /// Atomic işlem - race condition'ı önler.
    /// </summary>
    public async Task<bool> AddIfNotExists(string key, object data, DateTimeOffset? absExpr, TimeSpan? sldExpr)
    {
        if (data is null)
        {
            return false;
        }

        // Eğer IConnectionMultiplexer yoksa (ör: MemoryCache kullanılıyorsa), fallback olarak normal Add kullan
        if (_connectionMultiplexer == null)
        {
            // MemoryCache veya başka bir cache provider kullanılıyorsa, atomic olmayan kontrol yap
            var exists = await AnyAsync(key);
            if (exists)
            {
                return false;
            }

            await Add(key, data, absExpr, sldExpr);
            return true;
        }

        // Redis SETNX kullanarak atomic işlem
        var database = _connectionMultiplexer.GetDatabase();
        string json = JsonSerializer.Serialize(data, SerializerOptions);

        // Instance name'i key'e ekle (AddStackExchangeRedisCache'deki InstanceName ile uyumlu olmalı)
        var prefixedKey = $"BaseProject_{key}";

        // SETNX ile atomic olarak key ekle
        var wasSet = await database.StringSetAsync(
            prefixedKey,
            json,
            absExpr?.Subtract(DateTimeOffset.UtcNow) ?? sldExpr,
            When.NotExists);

        return wasSet;
    }
}
