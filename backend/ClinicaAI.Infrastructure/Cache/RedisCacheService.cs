using System;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace ClinicaAI.Infrastructure.Cache
{
    public class RedisCacheService
    {
        private readonly IConfiguration _configuration;
        private readonly IDatabase? _redisDatabase;
        private readonly ConcurrentDictionary<string, (string Value, DateTime Expiry)> _localCache = new();

        public RedisCacheService(IConfiguration configuration)
        {
            _configuration = configuration;
            try
            {
                var connectionString = _configuration["Redis:ConnectionString"] ?? "localhost:6379";
                if (!string.IsNullOrEmpty(connectionString))
                {
                    var options = ConfigurationOptions.Parse(connectionString);
                    options.ConnectTimeout = 3000;
                    options.AbortOnConnectFail = false; // Prevents crash if Redis is down
                    
                    var connection = ConnectionMultiplexer.Connect(options);
                    _redisDatabase = connection.GetDatabase();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Redis Cache Warning] Redis is unavailable. Falling back to internal Memory Cache. Detail: {ex.Message}");
                _redisDatabase = null;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            var json = JsonSerializer.Serialize(value);
            var absoluteExpiry = DateTime.UtcNow.Add(expiry ?? TimeSpan.FromHours(1));

            if (_redisDatabase != null)
            {
                try
                {
                    await _redisDatabase.StringSetAsync(key, json, expiry);
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Redis Error] Failed to write cache key '{key}': {ex.Message}. Using Local Fallback.");
                }
            }

            _localCache[key] = (json, absoluteExpiry);
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            if (_redisDatabase != null)
            {
                try
                {
                    string? value = await _redisDatabase.StringGetAsync(key);
                    if (!string.IsNullOrEmpty(value))
                    {
                        return JsonSerializer.Deserialize<T>(value);
                    }
                    return default;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Redis Error] Failed to retrieve cache key '{key}': {ex.Message}. Reading from Local Fallback.");
                }
            }

            // Local Memory Check
            if (_localCache.TryGetValue(key, out var cached))
            {
                if (cached.Expiry > DateTime.UtcNow)
                {
                    return JsonSerializer.Deserialize<T>(cached.Value);
                }
                _localCache.TryRemove(key, out _); // Clean up expired
            }

            return default;
        }

        public async Task RemoveAsync(string key)
        {
            if (_redisDatabase != null)
            {
                try
                {
                    await _redisDatabase.KeyDeleteAsync(key);
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Redis Error] Failed to remove cache key '{key}': {ex.Message}. Removing from Local.");
                }
            }

            _localCache.TryRemove(key, out _);
        }
    }
}
