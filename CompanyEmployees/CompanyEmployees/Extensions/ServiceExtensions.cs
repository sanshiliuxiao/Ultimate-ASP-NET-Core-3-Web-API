using AspNetCoreRateLimit;
using CompanyEmployees.CustomFormatters;
using Contracts;
using Entities;
using Entities.Models;
using LoggerService;
using Marvin.Cache.Headers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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
                    expirationModelOptions.MaxAge = 120;
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
                    Limit = 100,
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
    
        public static void ConfigureIdentity(this IServiceCollection services)
        {
            var builder = services.AddIdentityCore<User>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 10;
                options.User.RequireUniqueEmail = true;
            }).AddRoles<IdentityRole>();

            builder = new IdentityBuilder(builder.UserType, typeof(IdentityRole), builder.Services);
            builder.AddEntityFrameworkStores<RepositoryContext>().AddDefaultTokenProviders();
        }
    
        public static void ConfigureJWT(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtSettings = configuration.GetSection("JwtSettings");
            var issuer = jwtSettings.GetSection("validIssuer").Value;
            var audience = jwtSettings.GetSection("validAudience").Value;
            var secretKey = jwtSettings.GetSection("validSecretKey").Value;


            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options => 
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                };
            });
        
        }
    
        public static void ConfigureSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(s =>
            {
                s.SwaggerDoc("v1", new OpenApiInfo 
                { 
                    Title = "Code Maze API", 
                    Version = "v1",
                    Description = "CompanyEmployees API by CodeMaze",
                    TermsOfService = new Uri("https://example.com/terms"),
                    Contact = new OpenApiContact
                    {
                        Name = "Joho Doe",
                        Email = "John.Doe@gmail.com",
                        Url = new Uri("https://baidu.com")
                    },
                    License = new OpenApiLicense
                    {
                        Name = "CompanyEmployees API LICX",
                        Url = new Uri("https://baidu.com")
                    }
                });
                s.SwaggerDoc("v2", new OpenApiInfo { Title = "Code Maze API", Version = "v1" });

                // 支持 XML 注释
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile); ;
                s.IncludeXmlComments(xmlPath);


                // 添加 JWT 认证
                s.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme 
                {
                    Description ="JWT Authorization header using the Bearer scheme   Example: 'Bearer 1234sasd' ",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });


                s.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header
                        },
                        new List<string>()
                    }
                });
            });


        }
    }
}
