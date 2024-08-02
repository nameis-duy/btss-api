using Application.DTOs.Generic;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using FirebaseAdmin;
using FluentValidation;
using Google.Apis.Auth.OAuth2;
using Infrastructure.Constants;
using Infrastructure.Implements.Repositories;
using Infrastructure.Implements.Services;
using Infrastructure.Validators.Traveler;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetTopologySuite.IO.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Npgsql;
using System.Reflection;
using Vonage;
using Vonage.Request;

namespace Infrastructure
{
    public static class DepsInject
    {
        public static void AddInfra(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<AppConfig>(GetAppConfig());
            services.AddLocalization(opt => opt.ResourcesPath = GetFilePath(typeof(AppMessage)));
            services.AddOptions<AppConfig>();
            services.AddValidatorsFromAssemblyContaining<TravelerCreateValidator>();
            services.AddMapsterConfig();
            ConfigJsonSerializer();
            services.AddSingleton<ITimeService, TimeService>();
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(config.GetConnectionString("BTSS_Render2nd"));
            dataSourceBuilder.UseNetTopologySuite().UseJsonNet();
            var dataSource = dataSourceBuilder.Build();
            services.AddDbContext<AppDbContext>(options => options.UseNpgsql(dataSource, o => 
            {
                o.UseNetTopologySuite();
                o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            }));
            var firebaseApp = FirebaseApp.DefaultInstance;
            firebaseApp ??= FirebaseApp.Create(new AppOptions 
            { 
                Credential = GoogleCredential.FromJson(config["Firebase"]),
            });
            services.AddSingleton(firebaseApp);
            var vonageCredentials = Credentials.FromApiKeyAndSecret(config["Vonage:Key"], config["Vonage:Secret"]);
            var vonage = new VonageClient(vonageCredentials);
            services.AddSingleton(vonage);
            services.AddScoped(typeof(IGenericRepo<>), typeof(GenericRepo<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IGenericService<>), typeof(GenericService<>));
            services.AddScoped<IBackgroundService, BackgroundService>();
            services.AddScoped<ICacheService, CacheService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IConfigService, ConfigService>();
            services.AddScoped<IDestinationService, DestinationService>();
            services.AddScoped<IAnnouncementService, AnnouncementService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IPlanService, PlanService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IProviderService, ProviderService>();
            services.AddScoped<ITransactionService, TransactionService>();
        }
        private static string GetFilePath(Type targetType)
        {
            return Path.GetDirectoryName(Assembly.GetAssembly(targetType)!.Location)!;
        }
        private static IConfiguration GetAppConfig()
        {
            var builder = new ConfigurationBuilder().SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!)
                                                    .AddJsonFile(GlobalConstants.CONFIG_PATH, false, true);
            return builder.Build();
        }
        private static void AddMapsterConfig(this IServiceCollection services)
        {
            var config = TypeAdapterConfig.GlobalSettings;
            config.Scan(Assembly.GetExecutingAssembly());
            services.AddSingleton(config);
        }
        private static void ConfigJsonSerializer()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Converters = [new GeometryConverter(), new StringEnumConverter()],
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }
    }
}
