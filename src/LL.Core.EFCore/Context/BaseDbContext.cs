using LL.Core.Common.Logger;
using LL.Core.Model;
using LL.Core.Model.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace LL.Core.EFCore.Context
{
    public class BaseDbContext : DbContext
    {
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<BaseUserInfo> BaseUserInfos { get; set; }
        public DbSet<RequestLogEntity> RequestLogs { get; set; }

        /// <summary>
        /// 日志工厂
        /// </summary>
        private static readonly ILoggerFactory LoggerFactory = new LoggerFactory(new[] { new EFLoggerProvider() });

        public IServiceProvider _serviceProvider;

        public BaseDbContext(DbContextOptions<BaseDbContext> options, IServiceProvider serviceProvider) : base(options)
        {
            _serviceProvider = serviceProvider;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            //添加是否开启ef日志的开关
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
            //注意:因为开启了ef执行日志，所以要注意Nlog日志记录时各参数的空判断
            options.UseLoggerFactory(LoggerFactory);
        }
    }
}
