using System;
using System.Threading.Tasks;

namespace Remember
{
    public interface IRemember : IDisposable
    {
        Task SaveAsync<T>(string cacheKey, T cacheValue);

        Task<T> GetAsync<T>(string cacheKey);

        Task DeleteAsync(string cacheKey);
    }
}
