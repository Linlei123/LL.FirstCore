﻿using AspNetCoreRateLimit;
using LL.Core.Common.Config;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Redis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LL.Core.Extensions
{
    /// <summary>
    /// Ip限流
    /// </summary>
    public static class RateLimitServiceCollectionExtensions
    {
        /// <summary>
        /// 添加Ip限流
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="cacheConfig"></param>
        public static void AddIpRateLimit(this IServiceCollection services, IConfiguration configuration, CacheConfig cacheConfig)
        {
            #region IP限流
            services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));
            services.Configure<IpRateLimitPolicies>(configuration.GetSection("IpRateLimitPolicies"));

            if (cacheConfig.TypeRateLimit == Common.Cache.CacheType.Redis)
            {
                //redis
                var redisRateLimit = new CSRedis.CSRedisClient(cacheConfig.Redis.ConnectionStringRateLimit);
                services.AddSingleton<IDistributedCache>(new CSRedisCache(redisRateLimit));
                services.AddSingleton<IIpPolicyStore, DistributedCacheIpPolicyStore>();
                services.AddSingleton<IRateLimitCounterStore, DistributedCacheRateLimitCounterStore>();
            }
            else
            {
                //内存
                services.AddMemoryCache();
                services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
                services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            }
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
            #endregion
        }
    }
}
