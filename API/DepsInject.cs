using API.GraphQL.Mutations;
using API.GraphQL.Subscriptions;
using AppAny.HotChocolate.FluentValidation;
using Application.Interfaces.Services;
using Hangfire;
using HotChocolate.Execution.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Text;
using Hangfire.PostgreSql;
using API.Implements.Services;
using API.GraphQL.Queries;
using Infrastructure;
using API.GraphQL.Extends;

namespace API
{
    public static class DepsInject
    {
        public static void AddWebService(this IServiceCollection services, IConfiguration config)
        {
            services.AddHangfire(opt => opt.UsePostgreSqlStorage(cfg => cfg.UseNpgsqlConnection(config.GetConnectionString("BTSS_Render2nd"))));
            services.AddHangfireServer();
            //services.AddScoped<IScopedProcessingService, ScopedProcessingService>();
            services.AddCors(options => options.AddDefaultPolicy(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
            if (!config.HasJwtSectionReady()) throw new SystemException(AppMessage.ERR_CONFIG_MISSING);
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.RequireHttpsMetadata = false;
                        options.SaveToken = true;
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            ValidIssuer = config["Jwt:Issuer"],
                            ValidAudience = config["Jwt:Audience"],
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!))
                        };
                    });
            services.AddAuthorization();
            if (config.GetConnectionString("Redis") == null) throw new SystemException(AppMessage.ERR_CONFIG_MISSING);
            services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(config.GetConnectionString("Redis")!));
            services.AddHttpContextAccessor();
            services.AddSingleton<IClaimService, ClaimService>();
            services.AddControllers();
            services.AddGraphQLServer()
                    .AddType(new TimeSpanType(ScalarNames.TimeSpan, null, TimeSpanFormat.DotNet))
                    .AddMutationType<Mutation>()
                    .AddFluentValidation(opt => opt.UseDefaultErrorMapperWithDetails())
                    .BindRuntimeType<TimeOnly, TimeSpanType>()
                    .AddAPITypes()
                    .AddSpatialTypes()
                    .AddQueryMiddlewares()
                    .AddSubscriptionType<Subscription>()
                    .AddAuthorization()
                    //.AddDiagnosticEventListener<ServerEventListener>()
                    .AddRedisSubscriptions(redis => ConnectionMultiplexer.Connect(config.GetConnectionString("Redis")!));
            services.AddErrorFilter<ExceptionFilter>();



        }
        private static IRequestExecutorBuilder AddQueryMiddlewares(this IRequestExecutorBuilder builder)
        {
            return builder.AddProjections()
                          .AddFiltering()
                          .AddSorting()
                          .AddSpatialFiltering()
                          .AddSpatialProjections()
                          .AddQueryType<Query>();
        }
        private static bool HasJwtSectionReady(this IConfiguration config)
        {
            var jwtSection = config.GetSection("Jwt");
            if (jwtSection == null) return false;
            if (jwtSection.GetSection("Audience") == null
                || jwtSection.GetSection("Issuer") == null
                || jwtSection.GetSection("Key") == null
                || jwtSection.GetSection("RefreshKey") == null) return false;
            return true;
        }
    }
}
