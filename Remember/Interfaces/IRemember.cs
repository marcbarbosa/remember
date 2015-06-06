using System;
using System.Threading.Tasks;

namespace Remember.Interfaces
{
    public interface IRemember
    {
        Task SaveAsync<T>(string cacheKey, T cacheItem);

        Task<T> GetAsync<T>(string cacheKey);

        Task DeleteAsync(string cacheKey);
    }
}
