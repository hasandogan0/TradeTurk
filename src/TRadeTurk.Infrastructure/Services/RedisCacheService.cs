using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using TRadeTurk.Application.Common.Interfaces;

namespace TRadeTurk.Infrastructure.Services;

public sealed class RedisCacheService : ICacheService
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly IDistributedCache _distributedCache;
    private readonly ICacheService _fallbackCache;
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(
        IDistributedCache distributedCache,
        InMemoryCacheService fallbackCache,
        ILogger<RedisCacheService> logger)
    {
        _distributedCache = distributedCache;
        _fallbackCache = fallbackCache;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var bytes = await _distributedCache.GetAsync(key, cancellationToken);
            return bytes is null ? default : JsonSerializer.Deserialize<T>(bytes, SerializerOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis cache unavailable for key {CacheKey}. Using in-memory fallback.", key);
            return await _fallbackCache.GetAsync<T>(key, cancellationToken);
        }
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan expiration,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(value, SerializerOptions);
            await _distributedCache.SetAsync(
                key,
                bytes,
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = expiration },
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis cache unavailable for key {CacheKey}. Using in-memory fallback.", key);
            await _fallbackCache.SetAsync(key, value, expiration, cancellationToken);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _distributedCache.RemoveAsync(key, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis cache unavailable for key {CacheKey}. Using in-memory fallback.", key);
        }

        await _fallbackCache.RemoveAsync(key, cancellationToken);
    }
}
