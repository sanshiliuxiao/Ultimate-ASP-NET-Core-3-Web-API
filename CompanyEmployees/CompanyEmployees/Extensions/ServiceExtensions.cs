using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
