using AutoMapper;
using CompanyEmployees.ActionFilters;
using CompanyEmployees.Dtos;
using CompanyEmployees.Extensions;
using CompanyEmployees.Utility;
using Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using Repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CompanyEmployees
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            LogManager.LoadConfiguration(string.Concat(Directory.GetCurrentDirectory(),"/nlog.config"));
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // 注入服务
            services.ConfigureCors();
            services.ConfigureIISIntegration();
            services.ConfigureLoggerService();
            services.ConfigureSqlContext(Configuration);
            services.ConfigureRepositoryManager();
            services.ConfigureVersioning();
            services.ConfigureResponseCaching();
            services.ConfigureHttpCacheHeaders();
            services.AddAutoMapper(typeof(Startup));

            //web api 在 3.1 中 AddControllers 取代了 AddMvc
            // 不需要 View 视图
            // 添加内容协商， 支持返回 XML 使用 AddXmlDataContractSerializerFormatters
            // 默认是返回 json 格式数据

            // ASP .NET Core 支持自定义数据格式

            // 如果需要解析  ExpandoObject 类 不要使用 AddXmlSerializerFormatters 而是使用 AddXmlDataContractSerializerFormatters
            services.AddControllers(config =>
            {
                // 开启 Accept 字段
                config.RespectBrowserAcceptHeader = true;
                // 限制 Media Types， 当 服务器不支持请求的 数据格式时， 会返回 406
                config.ReturnHttpNotAcceptable = true;

                // 全局设置 public cache 时间

                config.CacheProfiles.Add("120SecondsDuration", new CacheProfile { Duration = 120 });
            }).AddNewtonsoftJson()
            .AddXmlDataContractSerializerFormatters()
            .AddCustomCSVFormatter();


            // 自定义 WebApi 验证模型
            services.Configure<ApiBehaviorOptions>(options => {

                // 该选项开启后，会阻止 默认的 modelState valid 行为
                // 但也能够灵活的在 action 使用 ModelState.IsValid 进行 判断操作。
                // 或者传递 InvalidModelStateResponseFactory  委托，统一返回
                options.SuppressModelStateInvalidFilter = true;
            });

            // 注入 action filter
            services.AddScoped<ValidationFilterAttribute>();
            services.AddScoped<ValidateCompanyExistsAttribute>();
            services.AddScoped<ValidateEmployeeForCompanyExistsAttribute>();
            services.AddScoped<ValidateMediaTypeAttribute>();

            // 注入 HATEOAS
            services.AddScoped<EmployeeLinks>();

            // 注入 data shaping service
            services.AddScoped<IDataShaper<EmployeeDto>, DataShaper<EmployeeDto>>();

            // 注入 自定义的媒体类型
            services.AddCustomMediaTypes();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerManager logger)
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

            // 全局错误响应
            app.ConfigureExceptionHandler(logger);


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

            // 启用缓存
            app.UseResponseCaching();
            app.UseHttpCacheHeaders();

            // 启用授权
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
