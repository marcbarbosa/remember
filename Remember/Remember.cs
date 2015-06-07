using MsgPack.Serialization;
using Remember.Interfaces;
using StackExchange.Redis;
using System;
using System.Configuration;
using System.IO;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace Remember
{
    public class Remember : IRemember
    {
        private static ConnectionMultiplexer redis;

        private static volatile Remember instance;
        private static object syncRoot = new object();
        
        private readonly RememberConfig rememberConfig;
        private const string CONFIG_SECTION = "remember";
        
        private const string DELETE_CHANNEL = "remember_delete_channel";

        public Remember()
        {
            rememberConfig = ConfigurationManager.GetSection(CONFIG_SECTION) as RememberConfig;

            redis = CreateRedisConnectionMultiplexer();

            if (redis.IsConnected)
            {
                redis.GetSubscriber().Subscribe(DELETE_CHANNEL, (channel, cacheKey) => MemoryCache.Default.Remove(cacheKey));
            }
        }

        private ConnectionMultiplexer CreateRedisConnectionMultiplexer()
        { 
            var redisConfigurationOptions = new ConfigurationOptions 
            { 
                DefaultDatabase = rememberConfig.Database, 
                AbortOnConnectFail = false, 
                Password = rememberConfig.Password 
            };

            foreach (EndpointElement endpoint in rememberConfig.Endpoints)
	        {
                redisConfigurationOptions.EndPoints.Add(endpoint.Host, endpoint.Port);
	        }

            return ConnectionMultiplexer.Connect(redisConfigurationOptions);
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

        public async Task SaveAsync<T>(string cacheKey, T cacheItem)
        {
            if (redis.IsConnected)
            {
                MemoryCache.Default.Add(cacheKey, cacheItem, new CacheItemPolicy { AbsoluteExpiration = new DateTimeOffset(DateTime.Now).AddSeconds(rememberConfig.Expiry) });

                using (var ms = new MemoryStream())
                {
                    MessagePackSerializer.Get<T>().Pack(ms, cacheItem);

                    await redis.GetDatabase().StringSetAsync(cacheKey, ms.ToArray(), expiry: TimeSpan.FromSeconds(rememberConfig.Expiry));
                }
            }
        }

        public async Task<T> GetAsync<T>(string cacheKey)
        {
            if (MemoryCache.Default[cacheKey] != null)
            {
                return (T)MemoryCache.Default[cacheKey];
            }

            if (redis.IsConnected)
            {
                byte[] cacheItemBytes = await redis.GetDatabase().StringGetAsync(cacheKey);

                if (cacheItemBytes != null)
                {
                    var cacheItem = MessagePackSerializer.Get<T>().Unpack(new MemoryStream(cacheItemBytes));

                    MemoryCache.Default.Add(cacheKey, cacheItem, new CacheItemPolicy { AbsoluteExpiration = new DateTimeOffset(DateTime.Now).AddSeconds(rememberConfig.Expiry) });

                    return cacheItem;
                }
            }

            return default(T);
        }

        public async Task DeleteAsync(string cacheKey)
        {
            MemoryCache.Default.Remove(cacheKey);

            if (redis.IsConnected)
            {
                await redis.GetDatabase().KeyDeleteAsync(cacheKey);
                await redis.GetSubscriber().PublishAsync(DELETE_CHANNEL, cacheKey);
            }
        }

        public void Dispose()
        {
            redis.Close();
            redis.Dispose();
        }
    }
}
