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
        /// Api�汾��Ϣ
        /// </summary>
        private IApiVersionDescriptionProvider provider;
        private IServiceCollection _services;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers(option =>
            {
                option.Filters.Add(typeof(GlobalExceptionFilter));
                //ȫ��·��ǰ׺��Լ
                option.UseCentralRoutePrefix(new RouteAttribute("api"));
            }).AddControllersAsServices();  //�����е�controller��Ϊserviceע��
            //ʹ��System.Text.Json�������ı�������
            //.AddJsonOptions(option =>
            //{
            //    //Ĭ�����������Ļᱻ����
            //    option.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
            //    //���ô˲������ⱨѭ�����õ��쳣
            //    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            //});
            #region using Api version(eg:�ο�����:https://www.cnblogs.com/jjg0519/p/7253594.html,https://www.quarkbook.com/?p=793)
            //��̬���ϰ�:https://blog.csdn.net/ma524654165/article/details/77880106

            services.AddApiVersioning(option =>
            {
                // ��ѡ��ΪtrueʱAPI����֧�ֵİ汾��Ϣ
                option.ReportApiVersions = true;
                // ���ṩ�汾ʱ��Ĭ��Ϊ1.0
                option.AssumeDefaultVersionWhenUnspecified = true;
                // ������δָ���汾ʱĬ��Ϊ1.0
                option.DefaultApiVersion = new ApiVersion(1, 0);
            }).AddVersionedApiExplorer(option =>
            {
                // �汾���ĸ�ʽ��v+�汾��
                option.GroupNameFormat = "'v'V";
                option.AssumeDefaultVersionWhenUnspecified = true;
            });
            #endregion

            services.Configure<JwtSetting>(Configuration.GetSection("JwtSetting"));

            #region �ٷ�JWT��֤��ʽ
            //����bearer��֤��ע��jwtbearer��֤����
            /*AddAuthentication("Bearer")==AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })*/
            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    //token��֤����
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        //3+2ģʽ (������+������+��Կ)+(��������+����ʱ��)

                        ValidateIssuer = true,      //�Ƿ���֤Issuer
                        ValidIssuer = Configuration["JwtSetting:Issure"],   //��֤�������ļ������õ��Ƿ�һ��
                        ValidateAudience = true,    //�Ƿ���֤Audience
                        ValidAudience = Configuration["JwtSetting:Audience"],   //��֤�������ļ������õ��Ƿ�һ��
                        ValidateIssuerSigningKey = true,    //�Ƿ���֤SecurityKey
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JwtSetting:Secret"])),

                        ValidateLifetime = true,    //�Ƿ���֤ʧЧʱ��,ʹ�õ�ǰʱ����token�е�Claim��������Notbefore��expires�Ա�
                        RequireExpirationTime = true,    //�Ƿ�Ҫ��token��claim�б������Expires
                        //����ķ�����ʱ��ƫ����(���ڻ���ʱ��,ͨ��������Կ������û���ʱ��ĳ���,����ΪTimeSpan.Zero��ʾ���ھ�ʧЧ,û�л�����)
                        ClockSkew = TimeSpan.FromSeconds(30)
                    };
                    //Jwt�¼�(JwtBearer��֤�У�Ĭ����ͨ��Http��Authorizationͷ����ȡ�ģ���Ҳ�����Ƽ���������������ĳЩ�����£����ǿ��ܻ�ʹ��Url������Cookie������Token)
                    options.Events = new JwtBearerEvents()
                    {
                        OnTokenValidated = context =>
                        {
                            return Task.CompletedTask;
                        },
                        OnMessageReceived = context =>
                        {
                            //SignalR����query����token��Ϣ
                            context.Token = context.Request.Query["access_token"];
                            return Task.CompletedTask;
                        },
                        //��֤ʧ��(����������е��쳣����)
                        OnAuthenticationFailed = c =>
                        {
                            c.NoResult();
                            c.Response.StatusCode = 500;
                            c.Response.ContentType = "text/plain";
                            c.Response.WriteAsync(c.Exception.ToString()).Wait();
                            return Task.CompletedTask;
                        },
                        //401(�����֤δͨ��)
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

            //��Ϊ��ȡ�����ķ�ʽĬ������΢�����һ��ӳ�䷽ʽ�����������Ҫ��JWTӳ����������ô������Ҫ��Ĭ��ӳ�䷽ʽ���Ƴ���
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            #endregion

            #region ���ÿ�������
            services.AddCors(options =>
            {
                var origins = Configuration.GetSection("AllowOrigins").Get<string[]>();
                options.AddDefaultPolicy(builder => builder.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod().AllowCredentials());
            });
            #endregion

            #region ���miniProfile(https://miniprofiler.com/dotnet/AspDotNetCore)
            services.AddMemoryCache();
            services.AddMiniProfiler(options =>
            {
                // �趨���ʷ������URL��·�ɻ���ַ
                options.RouteBasePath = "/profiler";
                //����ѡ�����ƴ洢
                //����MemoryCacheStorage��Ĭ��Ϊ30���ӣ�
                (options.Storage as MemoryCacheStorage).CacheDuration = TimeSpan.FromMinutes(30);
                // (Optional) Control which SQL formatter to use, InlineFormatter is the default
                options.SqlFormatter = new StackExchange.Profiling.SqlFormatters.InlineFormatter();
                // �趨�������ڵ�λ�������½�
                options.PopupRenderPosition = StackExchange.Profiling.RenderPosition.Left;
                // �趨�ڵ�������ϸ���������ʾTime With Children����
                options.PopupShowTimeWithChildren = true;
                options.ColorScheme = StackExchange.Profiling.ColorScheme.Auto;
            }).AddEntityFramework();
            #endregion

            #region using Swagger
            provider = BuildServiceProvider(services).GetRequiredService<IApiVersionDescriptionProvider>();
            services.AddSwaggerService(provider);
            #endregion

            #region ���EF Core����
            services.AddDbContext<BaseDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"), builder => builder.EnableRetryOnFailure());
            });
            //services.AddScoped<BaseDbContext>();
            #endregion

            #region ���HttpClient����
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
            //P3 ��DI������滻ԭ�е� IHttpMessageHandlerFilter ʵ��
            services.Replace(ServiceDescriptor.Singleton<IHttpMessageHandlerBuilderFilter, TraceIdLoggingMessageHandlerFilter>());
            #endregion

            #region ���ͳһģ����֤,����ʵ��IActionFilter
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true; // ʹ���Զ���ģ����֤
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

            //ע��������HttpContext��������
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IJwtProvider, JwtProvider>();

            //���Զ�ȡ������Ϣ������
            var str = ConfigHelper.GetDefaultJsonValue("AllowedHosts"); //Ĭ�������ļ�
            var otherStr = ConfigHelper.GetAppSetting("test.json", "AllowedHosts");   //����json�ļ���Ϣ
            //����д����������Ϣ�Զ�����ʽ�������Ե�����ʽע�ᵽ����������
            services.AddOptions().Configure<string>(Configuration.GetSection("AllowedHosts"));
            services.AddSingleton<ILogFormat, ContentFormat>();

            #region ���ñ����ʽ
            //services.Configure<WebEncoderOptions>(options =>
            //{
            //    options.TextEncoderSettings = new System.Text.Encodings.Web.TextEncoderSettings(UnicodeRanges.All);
            //});

            //֧�ֱ����ȫ ����:֧�� System.Text.Encoding.GetEncoding("GB2312")  System.Text.Encoding.GetEncoding("GB18030") 
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            #endregion

            #region ��ӽ������
            //��Ӷ����ݿ�ļ��
            services.AddHealthChecks().AddSqlServer(
                 Configuration.GetConnectionString("DefaultConnection"),
                 healthQuery: "SELECT 1;",
                 name: "sql server",
                 failureStatus: HealthStatus.Degraded,
                 tags: new[] { "db", "sql", "sqlserver" }
                );
            //���AspNetCore.HealthChecks.UI�Լ�HealthChecks.UI.InMemory.Storage��
            services.AddHealthChecksUI(setupSettings: setup =>
            {
                setup.AddHealthCheckEndpoint("sqlserver", "/health");
            }).AddInMemoryStorage();
            #endregion

            #region AutoMapper �Զ�ӳ��
            //Ѱ�ұ������м̳�Profile�������ʵ��
            services.AddAutoMapper(typeof(Startup));
            #endregion

            _services = services;
        }

        // ע����Program.CreateHostBuilder�����Autofac���񹤳�(3.0�﷨)
        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterModule(new AutofacModuleRegister());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                var autofacContaniers = (app.ApplicationServices.GetAutofacRoot())?.ComponentRegistry?.Registrations;
                app.Map("/allservices", builder => builder.Run(async context =>
                {
                    context.Response.ContentType = "text/html; charset=utf-8";
                    await context.Response.WriteAsync($"<h1>���з���{_services.Count}��</h1><table><thead><tr><th>����</th><th>��������</th><th>Instance</th></tr></thead><tbody>");
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

            #region swagger
            app.UseSwagger();
            app.UseSwaggerUI(option =>
            {
                foreach (var item in provider.ApiVersionDescriptions)
                {
                    option.SwaggerEndpoint($"/swagger/{item.GroupName}/swagger.json", "LL.Core V" + item.ApiVersion);
                }
                option.RoutePrefix = string.Empty;
                // ��swagger��ҳ�����ó������Զ����ҳ�棬�ǵ�����ַ�����д��,�ǵ�����ַ�����д��,�ǵ�����ַ�����д����������.index.html
                option.IndexStream = () => GetType().GetTypeInfo().Assembly.GetManifestResourceStream("LL.Core.index.html");
                option.DocExpansion(DocExpansion.List);//�۵�Api
                //�ر�ҳ��Schemeչʾ(��֪�����ߵ�����)
                option.DefaultModelsExpandDepth(-1);
                //���������������û��Ч��
                option.DefaultModelExpandDepth(-1);
            });
            #endregion
            //��ȡ��ǰ���еĽ�������
            var process = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
            //������־�м��
            app.UseRequestLog();

            // ������������ ע���±���Щ�м����˳�򣬺���Ҫ ������������
            // CORS����
            app.UseCors();
            // ��תhttps
            //app.UseHttpsRedirection();
            // ʹ�þ�̬�ļ�
            app.UseStaticFiles();
            //
            app.UseRouting();
            // �ȿ�����֤
            app.UseAuthentication();
            // Ȼ������Ȩ�м��
            app.UseAuthorization();
            // ���ܷ���
            app.UseMiniProfiler();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health", new HealthCheckOptions()
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
                //����"/healthchecks-ui"���ɿ������ӻ�ҳ��
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
