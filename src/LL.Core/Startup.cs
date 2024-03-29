using LL.Core.EFCore.Context;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LL.Core.Common.Logger;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Http;
using LL.Core.Common.Config;
using LL.Core.Common.Jwt;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using LL.Core.Filter;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using LL.Core.Filter.GlobalConvention;
using Microsoft.Extensions.Http;
using static LL.Core.HttpHelper.CustomHttpClientLogging;
using LL.Core.Extensions;
using Autofac;
using System.Reflection;
using Swashbuckle.AspNetCore.SwaggerUI;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using StackExchange.Profiling.Storage;
using AutoMapper;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Newtonsoft.Json.Linq;
using LL.Core.Middleware;
using Autofac.Extensions.DependencyInjection;
using LL.Core.Common.Extensions;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;
using LL.Core.Common.Cache;

namespace LL.Core
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        /// <summary>
        /// Api版本信息
        /// </summary>
        private IApiVersionDescriptionProvider provider;
        private IServiceCollection _services;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers(option =>
            {
                option.Filters.Add(typeof(GlobalExceptionFilter));
                //全局路由前缀公约
                option.UseCentralRoutePrefix(new RouteAttribute("api"));
            }).AddControllersAsServices();  //将所有的controller作为service注入
            //使用System.Text.Json会有中文编码问题
            //.AddJsonOptions(option =>
            //{
            //    //默认设置下中文会被编码
            //    option.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
            //    //设置此参数避免报循坏引用的异常
            //    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            //});
            #region using Api version(eg:参考链接:https://www.cnblogs.com/jjg0519/p/7253594.html,https://www.quarkbook.com/?p=793)
            //动态整合版:https://blog.csdn.net/ma524654165/article/details/77880106

            services.AddApiVersioning(option =>
            {
                // 可选，为true时API返回支持的版本信息
                option.ReportApiVersions = true;
                // 不提供版本时，默认为1.0
                option.AssumeDefaultVersionWhenUnspecified = true;
                // 请求中未指定版本时默认为1.0
                option.DefaultApiVersion = new ApiVersion(1, 0);
            }).AddVersionedApiExplorer(option =>
            {
                // 版本名的格式：v+版本号
                option.GroupNameFormat = "'v'V";
                option.AssumeDefaultVersionWhenUnspecified = true;
            });
            #endregion

            services.Configure<JwtSetting>(Configuration.GetSection("JwtSetting"));

            #region 官方JWT认证方式
            //开启bearer认证，注入jwtbearer认证服务
            /*AddAuthentication("Bearer")==AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })*/
            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    //token认证对象
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        //3+2模式 (发行人+订阅人+秘钥)+(生命周期+过期时间)

                        ValidateIssuer = true,      //是否验证Issuer
                        ValidIssuer = Configuration["JwtSetting:Issure"],   //验证与配置文件中设置的是否一致
                        ValidateAudience = true,    //是否验证Audience
                        ValidAudience = Configuration["JwtSetting:Audience"],   //验证与配置文件中设置的是否一致
                        ValidateIssuerSigningKey = true,    //是否验证SecurityKey
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JwtSetting:Secret"])),

                        ValidateLifetime = true,    //是否验证失效时间,使用当前时间与token中的Claim中声明的Notbefore和expires对比
                        RequireExpirationTime = true,    //是否要求token的claim中必须包含Expires
                        //允许的服务器时间偏移量(过期滑动时间,通过这个属性可以设置滑动时间的长短,设置为TimeSpan.Zero表示到期就失效,没有缓冲期)
                        ClockSkew = TimeSpan.FromSeconds(30)
                    };
                    //Jwt事件(JwtBearer认证中，默认是通过Http的Authorization头来获取的，这也是最推荐的做法，但是在某些场景下，我们可能会使用Url或者是Cookie来传递Token)
                    options.Events = new JwtBearerEvents()
                    {
                        OnTokenValidated = context =>
                        {
                            return Task.CompletedTask;
                        },
                        OnMessageReceived = context =>
                        {
                            //SignalR采用query传递token信息
                            context.Token = context.Request.Query["access_token"];
                            return Task.CompletedTask;
                        },
                        //认证失败(请求处理过程中的异常错误)
                        OnAuthenticationFailed = c =>
                        {
                            c.NoResult();
                            c.Response.StatusCode = 500;
                            c.Response.ContentType = "text/plain";
                            c.Response.WriteAsync(c.Exception.ToString()).Wait();
                            return Task.CompletedTask;
                        },
                        //401(身份验证未通过)
                        OnChallenge = context =>
                        {
                            // Skip the default logic.
                            context.HandleResponse();

                            var payload = new JObject
                            {
                                ["error"] = context.Error,
                                ["error_description"] = context.ErrorDescription,
                                ["error_uri"] = context.ErrorUri
                            };

                            context.Response.ContentType = "application/json";
                            context.Response.StatusCode = 401;

                            return context.Response.WriteAsync(payload.ToString());
                        }
                    };
                });

            //因为获取声明的方式默认是走微软定义的一套映射方式，如果我们想要走JWT映射声明，那么我们需要将默认映射方式给移除掉
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            #endregion

            #region 配置跨域请求
            services.AddCors(options =>
            {
                var origins = Configuration.GetSection("AllowOrigins").Get<string[]>();
                options.AddDefaultPolicy(builder => builder.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod().AllowCredentials());
            });
            #endregion

            #region 添加miniProfile(https://miniprofiler.com/dotnet/AspDotNetCore)
            services.AddMemoryCache();
            services.AddMiniProfiler(options =>
            {
                // 设定访问分析结果URL的路由基地址
                options.RouteBasePath = "/profiler";
                //（可选）控制存储
                //（在MemoryCacheStorage中默认为30分钟）
                (options.Storage as MemoryCacheStorage).CacheDuration = TimeSpan.FromMinutes(30);
                // (Optional) Control which SQL formatter to use, InlineFormatter is the default
                options.SqlFormatter = new StackExchange.Profiling.SqlFormatters.InlineFormatter();
                // 设定弹出窗口的位置是左下角
                options.PopupRenderPosition = StackExchange.Profiling.RenderPosition.Left;
                // 设定在弹出的明细窗口里会显示Time With Children这列
                options.PopupShowTimeWithChildren = true;
                options.ColorScheme = StackExchange.Profiling.ColorScheme.Auto;
            }).AddEntityFramework();
            #endregion

            #region using Swagger
            provider = BuildServiceProvider(services).GetRequiredService<IApiVersionDescriptionProvider>();
            services.AddSwaggerService(provider);
            #endregion

            #region 添加EF Core服务
            services.AddDbContext<BaseDbContext>(options =>
            {
                //使用RetryOnFailure可能会出现以下问题:https://devblogs.microsoft.com/cesardelatorre/using-resilient-entity-framework-core-sql-connections-and-transactions-retries-with-exponential-backoff/
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"), builder => builder.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(20), errorNumbersToAdd: null));
            });
            //services.AddScoped<BaseDbContext>();
            #endregion

            #region 添加HttpClient请求
            //services.AddHttpClient("test").ConfigurePrimaryHttpMessageHandler(provider =>
            //{
            //    return new PrimaryHttpMessageHandler(provider);
            //})
            //.AddHttpMessageHandler(provider =>
            //{
            //    return new LogHttpMessageHandler(provider);
            //})
            //.AddHttpMessageHandler(provider =>
            //{
            //    return new Log2HttpMessageHandler(provider);
            //});
            services.AddHttpClient();
            //P3 在DI框架中替换原有的 IHttpMessageHandlerFilter 实现
            services.Replace(ServiceDescriptor.Singleton<IHttpMessageHandlerBuilderFilter, TraceIdLoggingMessageHandlerFilter>());
            #endregion

            #region 添加统一模型验证,无需实现IActionFilter
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true; // 使用自定义模型验证
                options.InvalidModelStateResponseFactory = (context) =>
                {
                    StringBuilder errorMessage = new StringBuilder();
                    var validationErrors = context.ModelState.Keys.SelectMany(k => context.ModelState[k].Errors).Select(e => e.ErrorMessage).ToArray();
                    foreach (var item in context.ModelState.Values)
                    {
                        foreach (var error in item.Errors)
                        {
                            errorMessage.Append(error.ErrorMessage + "|");
                        }
                    }

                    return new JsonResult(errorMessage);
                };
            });
            #endregion

            //注入类似于HttpContext的上下文
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IJwtProvider, JwtProvider>();

            //测试读取配置信息帮助类
            var str = ConfigHelper.GetDefaultJsonValue("AllowedHosts"); //默认配置文件
            var otherStr = ConfigHelper.GetAppSetting("test.json", "AllowedHosts");   //其他json文件信息
            //下面写法将配置信息以对象形式来表达，并以单例方式注册到服务容器中
            services.AddOptions().Configure<string>(Configuration.GetSection("AllowedHosts"));
            services.AddSingleton<ILogFormat, ContentFormat>();

            #region 设置编码格式
            //services.Configure<WebEncoderOptions>(options =>
            //{
            //    options.TextEncoderSettings = new System.Text.Encodings.Web.TextEncoderSettings(UnicodeRanges.All);
            //});

            //支持编码大全 例如:支持 System.Text.Encoding.GetEncoding("GB2312")  System.Text.Encoding.GetEncoding("GB18030") 
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            #endregion

            #region 添加健康检查
            //添加对数据库的检测
            services.AddHealthChecks().AddSqlServer(
                 Configuration.GetConnectionString("DefaultConnection"),
                 healthQuery: "SELECT 1;",
                 name: "sql server",
                 failureStatus: HealthStatus.Degraded,
                 tags: new[] { "db", "sql", "sqlserver" }
                );
            //添加AspNetCore.HealthChecks.UI以及HealthChecks.UI.InMemory.Storage包
            services.AddHealthChecksUI(setupSettings: setup =>
            {
                setup.AddHealthCheckEndpoint("sqlserver", "/health");
            }).AddInMemoryStorage();
            #endregion

            #region AutoMapper 自动映射
            //寻找本程序集中继承Profile类的所有实现
            services.AddAutoMapper(typeof(Startup));
            #endregion

            #region 开启响应压缩(参考链接:https://masuit.com/1771)
            services.Configure<BrotliCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Optimal;
            }).Configure<GzipCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Optimal;
            }).AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
                {
                    "text/html; charset=utf-8",
                    "application/xhtml+xml",
                    "application/atom+xml",
                    "image/svg+xml"
                });
            });
            #endregion

            #region 添加缓存以及IP限流服务
            var cacheConfig = Configuration.Get<CacheConfig>();
            if (cacheConfig.Type == CacheType.Redis)
            {
                var csredis = new CSRedis.CSRedisClient(cacheConfig.Redis.ConnectionString);
                RedisHelper.Initialization(csredis);
                services.AddSingleton<ICaching, RedisCaching>();
            }
            else
            {
                services.AddMemoryCache();
                services.AddSingleton<ICaching, MemoryCaching>();
            }
            services.AddIpRateLimit(Configuration, cacheConfig);
            #endregion

            _services = services;
        }

        // 注意在Program.CreateHostBuilder，添加Autofac服务工厂(3.0语法)
        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterModule(new AutofacModuleRegister());
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                var autofacContaniers = (app.ApplicationServices.GetAutofacRoot())?.ComponentRegistry?.Registrations;
                app.Map("/allservices", builder => builder.Run(async context =>
                {
                    context.Response.ContentType = "text/html; charset=utf-8";
                    await context.Response.WriteAsync($"<h1>所有服务{_services.Count}个</h1><table><thead><tr><th>类型</th><th>生命周期</th><th>Instance</th></tr></thead><tbody>");
                    foreach (var svc in _services)
                    {
                        await context.Response.WriteAsync("<tr>");
                        await context.Response.WriteAsync($"<td>{svc.ServiceType.FullName}</td>");
                        await context.Response.WriteAsync($"<td>{svc.Lifetime}</td>");
                        await context.Response.WriteAsync($"<td>{svc.ImplementationType?.FullName}</td>");
                        await context.Response.WriteAsync("</tr>");
                    }
                    foreach (var item in autofacContaniers.ToList())
                    {
                        var interfaceType = item.Services;
                        foreach (var typeArray in interfaceType)
                        {
                            await context.Response.WriteAsync("<tr>");
                            await context.Response.WriteAsync($"<td>{typeArray?.Description}</td>");
                            await context.Response.WriteAsync($"<td>{item.Lifetime}</td>");
                            await context.Response.WriteAsync($"<td>{item?.Target.Activator.SafeString().Replace("(ReflectionActivator)", "")}</td>");
                            await context.Response.WriteAsync("</tr>");
                        }
                    }
                    await context.Response.WriteAsync("</tbody></table>");
                }));
                app.UseDeveloperExceptionPage();
            }
            //启用压缩
            app.UseResponseCompression();

            #region swagger
            app.UseSwagger();
            app.UseSwaggerUI(option =>
            {
                foreach (var item in provider.ApiVersionDescriptions)
                {
                    option.SwaggerEndpoint($"/swagger/{item.GroupName}/swagger.json", "LL.Core V" + item.ApiVersion);
                }
                option.RoutePrefix = string.Empty;
                // 将swagger首页，设置成我们自定义的页面，记得这个字符串的写法,记得这个字符串的写法,记得这个字符串的写法：程序集名.index.html
                option.IndexStream = () => GetType().GetTypeInfo().Assembly.GetManifestResourceStream("LL.Core.index.html");
                option.DocExpansion(DocExpansion.List);//折叠Api
                //关闭页面Scheme展示(不知道两者的区别)
                option.DefaultModelsExpandDepth(-1);
                //设置下面这个好像没有效果
                option.DefaultModelExpandDepth(-1);
            });
            #endregion
            //获取当前运行的进程名称
            var process = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
            //请求日志中间件
            app.UseRequestLog();
            // ↓↓↓↓↓↓ 注意下边这些中间件的顺序，很重要 ↓↓↓↓↓↓
            // CORS跨域
            app.UseCors();
            // 跳转https
            //app.UseHttpsRedirection();
            // 使用静态文件
            app.UseStaticFiles();
            //
            app.UseRouting();
            // 先开启认证
            app.UseAuthentication();
            // 然后是授权中间件
            app.UseAuthorization();
            // 性能分析
            app.UseMiniProfiler();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health", new HealthCheckOptions()
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
                //访问"/healthchecks-ui"即可看到可视化页面
                endpoints.MapHealthChecksUI();
                endpoints.MapControllers();
            });
        }

        private ServiceProvider BuildServiceProvider(IServiceCollection services)
        {
            return services.BuildServiceProvider();
        }
    }
}
