using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using System.Runtime.Serialization.Formatters.Binary;
using Webhook.Interface;

namespace Webhook.Service
{
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly ISocialManagementRepository _socialManagementRepository;
        private DistributedCacheEntryOptions option;
        public CacheService(IDistributedCache cache, ISocialManagementRepository socialManagementRepository)
        {
            _cache = cache;
            _socialManagementRepository = socialManagementRepository;
            option = new DistributedCacheEntryOptions()
               .SetAbsoluteExpiration(DateTime.Now.AddMinutes(10))
               .SetSlidingExpiration(TimeSpan.FromMinutes(2));
        }
        public async Task<Dictionary<string, string>> GetFacebookPagesSetting()
        {
            var redisFacebookPages = await _cache.GetAsync("FacebookPages");

            Dictionary<string, string> facebookPages = new Dictionary<string, string>();
            if (redisFacebookPages == null)
            {
                facebookPages = await _socialManagementRepository.GetFbPages();

                var binFormatter = new BinaryFormatter();
                var mStream = new MemoryStream();
                binFormatter.Serialize(mStream, facebookPages);

                //This gives you the byte array.
                byte[] valueCache = mStream.ToArray();

                await _cache.SetAsync("FacebookPages", valueCache);
            }
            else
            {
                facebookPages = await _socialManagementRepository.GetFbPages();
            }

            return facebookPages;
        }

        public async Task<Dictionary<string, string>> GetZaloPagesSetting()
        {
            var redisZaloPages = await _cache.GetAsync("ZaloPages");

            Dictionary<string, string> zaloPages = new Dictionary<string, string>();
            if (redisZaloPages == null)
            {
                zaloPages = await _socialManagementRepository.GetZaloPages();

                var binFormatter = new BinaryFormatter();
                var mStream = new MemoryStream();
                binFormatter.Serialize(mStream, zaloPages);

                //This gives you the byte array.
                byte[] valueCache = mStream.ToArray();

                await _cache.SetAsync("FacebookPages", valueCache);
            }
            else
            {
                zaloPages = await _socialManagementRepository.GetZaloPages();
            }

            return zaloPages;
        }

        public async Task<Dictionary<string, string>> GetAllPagesSetting()
        {
            var redisAllPages = await _cache.GetAsync("AllPages");

            Dictionary<string, string> allPages = new Dictionary<string, string>();
            if (redisAllPages == null)
            {
                allPages = await _socialManagementRepository.GetPages();

                var binFormatter = new BinaryFormatter();
                var mStream = new MemoryStream();
                binFormatter.Serialize(mStream, allPages);

                //This gives you the byte array.
                byte[] valueCache = mStream.ToArray();

                await _cache.SetAsync("FacebookPages", valueCache);
            }
            else
            {
                allPages = await _socialManagementRepository.GetPages();
            }

            return allPages;
        }
    }
}
