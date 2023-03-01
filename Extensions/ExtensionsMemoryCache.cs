using Microsoft.Extensions.Caching.Memory;

namespace MinimalScrapyApi.Extensions
{
    public static class ExtensionsMemoryCache
    {
        public static void AddKey<T>(this IMemoryCache memory,string key,T data, DateTimeOffset lifeTime){
            
            var cacheEntryOptions = new MemoryCacheEntryOptions().AbsoluteExpiration=lifeTime;
            memory.Set<T>(key,data);   

        }
    }
}