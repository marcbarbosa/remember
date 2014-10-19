using System.Threading.Tasks;

namespace Remember.Interfaces
{
    public interface IRemember
    {
        Task SaveAsync<T>(string cacheKey, T obj);

        Task<T> GetAsync<T>(string cacheKey);

        Task RemoveAsync(string cacheKey);
    }
}
