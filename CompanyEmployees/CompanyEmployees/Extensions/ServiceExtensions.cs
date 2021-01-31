using AspNetCoreRateLimit;
using CompanyEmployees.CustomFormatters;
using Contracts;
using Entities;
using LoggerService;
using Marvin.Cache.Headers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
namespace CompanyEmployees.Extensions
{
    public static class ServiceExtensions
    {
        public static void ConfigureCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                {
                    builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();

                    //builder
                    //.WithOrigins()
                    //.WithMethods()
                    //.WithHeaders();
                });
            });
        }

        public static void ConfigureIISIntegration(this IServiceCollection services)
        {
            // Asp .Net Core 默认是 self hosted
            // 如果需要 IIS host， 则需要配置
            services.Configure<IISOptions>(options =>
            {

            });
        }
        

        public static void ConfigureLoggerService(this IServiceCollection services)
        {
            // 依赖注入,可以注入自己的一些服务
            // AddSingleton  AddScoped AddTransient
            services.AddScoped<ILoggerManager, LoggerManager>();
        }
    
        public static void ConfigureSqlContext(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<RepositoryContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("SqlConnection"), b => {
                    b.MigrationsAssembly(typeof(Startup).GetTypeInfo().Assembly.GetName().Name);
                });
            });
        }

        public static void ConfigureRepositoryManager(this IServiceCollection services)
        {
            services.AddScoped<IRepositoryManager, RepositoryManager>();
        }


        public static IMvcBuilder AddCustomCSVFormatter(this IMvcBuilder builder)
        {
            return builder.AddMvcOptions(config =>
            {
                config.OutputFormatters.Add(new CsvOutputFormatter());
            });
        }

        public static void AddCustomMediaTypes(this IServiceCollection services)
        {
            services.Configure<MvcOptions>(config =>
            {
                var newtonsoftJsonOutputFormatter = config.OutputFormatters.OfType<NewtonsoftJsonOutputFormatter>()?.FirstOrDefault();
            
                if (newtonsoftJsonOutputFormatter != null)
                {
                    newtonsoftJsonOutputFormatter.SupportedMediaTypes.Add("application/x.y.hateoas+json");
                    newtonsoftJsonOutputFormatter.SupportedMediaTypes.Add("application/x.y.apiroot+json");
                }

                var xmlOutputFormatter = config.OutputFormatters.OfType<XmlDataContractSerializerOutputFormatter>()?.FirstOrDefault();

                if (xmlOutputFormatter != null)
                {
                    xmlOutputFormatter.SupportedMediaTypes.Add("application/x.y.hateoas+xml");
                    xmlOutputFormatter.SupportedMediaTypes.Add("application/x.y.apiroot+xml");
                }
            });

        }
    
        public static void ConfigureVersioning(this IServiceCollection services)
        {
            // 注册 api version 服务， 设置默认的版本为 1
            services.AddApiVersioning(options =>
            {
                // 将 api version 加入 response header
                options.ReportApiVersions = true;
                // 假设所有 没加 ApiVersion 属性的 controllers 为 默认版本
                options.AssumeDefaultVersionWhenUnspecified = true;
                // 设置默认版本号 1
                options.DefaultApiVersion = new ApiVersion(1, 0);

                // 默认可以通过查询字符串进行版本更替访问，
                // 但可以通过设置 ApiVersionReader 属性，变成在  request header 里面进行控制
                options.ApiVersionReader = new HeaderApiVersionReader("api-version");
            
            });
        }
    
        public static void ConfigureResponseCaching(this IServiceCollection services)
        {
            // 添加 缓存
            services.AddResponseCaching();
        }

        public static void ConfigureHttpCacheHeaders(this IServiceCollection services)
        {
            services.AddHttpCacheHeaders(
                (expirationModelOptions) =>
                {
                    expirationModelOptions.MaxAge = 600;
                    expirationModelOptions.CacheLocation = CacheLocation.Private;
                },
                (validationModelOptions) =>
                {
                    validationModelOptions.MustRevalidate = true;
                });

        }
    
        public static void ConfigureRateLimitingOptions(this IServiceCollection services)
        {
            // 使用 AspNetCoreRateLimit 需要使用 AddMemoryCache

            // 在 Startup 里面写
            // services.AddMemoryCache();

            // 配置规则
            var rateLimitRules = new List<RateLimitRule>
            {
                new RateLimitRule
                {
                    Endpoint = "*",
                    Limit = 5,
                    Period = "1m"
                }
            };

            services.Configure<IpRateLimitOptions>(options =>
            {
                options.GeneralRules = rateLimitRules;
            });

            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

        }
    }
}
