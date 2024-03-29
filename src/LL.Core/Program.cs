using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LL.Core
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //NLog参考:https://www.cnblogs.com/muyeh/p/9788311.html   https://www.cnblogs.com/fuchongjundream/p/3936431.html
            //var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            Logger logger = LogManager.GetCurrentClassLogger();
            try
            {
                logger.Debug("init main");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception exception)
            {
                //NLog: catch setup errors
                logger.Error(exception, "Stopped program because of exception");
                throw;
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                LogManager.Shutdown();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>().ConfigureAppConfiguration((host, config) =>
                    {
                        config.AddJsonFile("./configs/ratelimitconfig.json", optional: true, reloadOnChange: true);
                        config.AddJsonFile("./configs/cacheconfig.json", optional: true, reloadOnChange: true);
                    });
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders(); //移除已经注册的其他日志处理程序
                    logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Debug);
                }).UseNLog();
    }
}
