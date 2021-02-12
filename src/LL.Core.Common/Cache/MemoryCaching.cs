using LL.Core.Common.Extensions;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LL.Core.Common.Cache
{
    /// <summary>
    /// 内存缓存模式
    /// </summary>
    public class MemoryCaching : ICaching
    {
        //引用Microsoft.Extensions.Caching.Memory;这个和.net 还是不一样，没有了Httpruntime了
        private readonly IMemoryCache _memoryCache;
        public MemoryCaching(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public long Delete(params string[] keys)
        {
            foreach (var key in keys)
            {
                _memoryCache.Remove(key);
            }
            return keys.Length;
        }

        public Task<long> DeleteAsync(params string[] keys)
        {
            foreach (var key in keys)
            {
                _memoryCache.Remove(key);
            }

            return Task.FromResult(keys.Length.ToLong());
        }

        public async Task<long> DeleteByPatternAsync(string pattern)
        {
            if (pattern.IsEmpty())
                return default;

            pattern = Regex.Replace(pattern, @"\{.*\}", "(.*)");

            var keys = GetAllKeys().Where(k => Regex.IsMatch(k, pattern));

            if (keys != null && keys.Count() > 0)
            {
                return await DeleteAsync(keys.ToArray());
            }

            return default;
        }

        public bool Exists(string key)
        {
            return _memoryCache.TryGetValue(key, out _);
        }

        public Task<bool> ExistsAsync(string key)
        {
            return Task.FromResult(_memoryCache.TryGetValue(key, out _));
        }

        public string Get(string key)
        {
            return _memoryCache.Get(key)?.ToString();
        }

        public T Get<T>(string key)
        {
            return _memoryCache.Get<T>(key);
        }

        public Task<string> GetAsync(string key)
        {
            return Task.FromResult(Get(key));
        }

        public Task<T> GetAsync<T>(string key)
        {
            return Task.FromResult(Get<T>(key));
        }

        public bool Set(string key, object value)
        {
            _memoryCache.Set(key, value);
            return true;
        }

        public bool Set(string key, object value, TimeSpan expire)
        {
            _memoryCache.Set(key, value, expire);
            return true;
        }

        public Task<bool> SetAsync(string key, object value)
        {
            Set(key, value);
            return Task.FromResult(true);
        }

        public Task<bool> SetAsync(string key, object value, TimeSpan expire)
        {
            Set(key, value, expire);
            return Task.FromResult(true);
        }

        /// <summary>
        /// 获取所有key信息
        /// </summary>
        /// <returns></returns>
        private List<string> GetAllKeys()
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var entries = _memoryCache.GetType().GetField("_entries", flags).GetValue(_memoryCache);
            var keys = new List<string>();
            if (!(entries is IDictionary cacheItems))
                return keys;
            foreach (DictionaryEntry cacheItem in cacheItems)
            {
                keys.Add(cacheItem.Key.ToString());
            }
            return keys;
        }
    }
}
