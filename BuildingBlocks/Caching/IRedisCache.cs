using System;
using System.Threading.Tasks;
namespace Caching;

public interface IRedisCache
{
    Task<string> GetStringAsync(string key);
    Task SetStringAsync(string key, string value, TimeSpan? expiry = null);
    Task RemoveAsync(string key);
}