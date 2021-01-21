using Contracts;
using Entities;
using LoggerService;
using Microsoft.AspNetCore.Builder;
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
    }
}
