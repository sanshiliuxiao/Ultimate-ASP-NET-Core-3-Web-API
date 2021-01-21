using CompanyEmployees.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompanyEmployees
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // 注入服务
            services.ConfigureCors();
            services.ConfigureIISIntegration();

            //web api 在 3.1 中 AddControllers 取代了 AddMvc
            // 不需要 View 视图
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // 中间件的启动顺序非常关键
            // UseStaticFiles UseCors 要在 UseRouting 之前
            // UseRouting 要在 UseAuthorization 之前

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            // 启用配置

            // 启动静态文件 默认访问 wwwroot 文件夹
            app.UseStaticFiles();
            // 启用跨域
            app.UseCors("CorsPolicy");
            
            // 启用代理转发， 可以获取到 proxy headers
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders =ForwardedHeaders.All
            });

            // 启用路由
            app.UseRouting();

            // 启用授权
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
