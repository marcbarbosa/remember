using MsgPack.Serialization;
using Remember.Interfaces;
using StackExchange.Redis;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Remember
{
    public class Remember : IRemember
    {
        private static volatile Remember instance;
        private static object syncRoot = new object();

        private static ConnectionMultiplexer redis;

        public Remember()
        {
            redis = ConnectionMultiplexer.Connect("10.0.1.7");
        }

        public static Remember Instance
        {
            get 
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new Remember();
                        }
                    }
                }

                return instance;
            }
        }

        public async Task SaveAsync<T>(string cacheKey, T obj)
        {
            HttpRuntime.Cache.Insert(cacheKey, obj);

            var ms = new MemoryStream();
            MessagePackSerializer.Get<T>().Pack(ms, obj);

            await redis.GetDatabase().StringSetAsync(cacheKey, ms.ToArray());
        }

        public async Task<T> GetAsync<T>(string cacheKey)
        {
            if (HttpRuntime.Cache[cacheKey] != null)
            {
                return (T)HttpRuntime.Cache[cacheKey];
            }

            byte[] cacheItem = await redis.GetDatabase().StringGetAsync(cacheKey);

            if (cacheItem != null)
            {
                return MessagePackSerializer.Get<T>().Unpack(new MemoryStream(cacheItem));
            }

            return default(T);
        }

        public async Task RemoveAsync(string cacheKey)
        {
            HttpRuntime.Cache.Remove(cacheKey);
        }
    }
}
