using Application.DTOs.Generic;
using Application.Interfaces.Services;
using Infrastructure.Constants;
using Mapster;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Reflection;

namespace Infrastructure.Implements.Services
{
    public class ConfigService : IConfigService
    {
        private readonly ITimeService timeService;
        private readonly IConfiguration configuration;
        private readonly IBackgroundService backgroundService;
        private readonly AppConfig appConfig;

        public ConfigService(IOptionsSnapshot<AppConfig> appConfig, ITimeService timeService, IBackgroundService backgroundService)
        {
            configuration = new ConfigurationBuilder()
               .SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!)
               .AddJsonFile(GlobalConstants.CONFIG_PATH, false, true).Build();
            this.appConfig = appConfig.Value;
            this.timeService = timeService;
            this.backgroundService = backgroundService;
        }
        #region Set system datetime
        public DateTime SetSystemDateTime(DateTime dateTime)
        {
            var now = timeService.Now;
            var utcNow = timeService.UtcNow;
            var span = dateTime - now;
            var actualSpan = dateTime - utcNow;
            backgroundService.RecalculateExecutionTime(span);
            timeService.SetAdditionalTimeSpan(actualSpan);
            var newNow = timeService.Now;
            return newNow;
        }
        #endregion
        #region Reset system datetime
        public DateTime ResetSystemDateTime()
        {
            backgroundService.RecalculateExecutionTime(-timeService.AdditionalSpan);
            timeService.SetAdditionalTimeSpan(TimeSpan.Zero);
            return timeService.Now;
        }
        #endregion
        #region App config update
        public bool UpdateConfig(AppConfig dto)
        {
            try
            {
                //var propInfor = typeof(AppConfig).GetProperty(dto.SettingName)!;
                //if (propInfor.PropertyType == typeof(bool)) propInfor.SetValue(appConfig, Convert.ToBoolean(dto.Value));
                //typeof(AppConfig).GetProperty(dto.SettingName)!.SetValue(appConfig, dto.Value);

                dto.Adapt(appConfig);
                configuration.GetSection(nameof(AppConfig)).Bind(appConfig);
                var filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, GlobalConstants.CONFIG_PATH);
                using var streamWriter = new StreamWriter(filePath);
                streamWriter.Write(JsonConvert.SerializeObject(appConfig));

                return true;
            }
            catch
            {
                throw new SystemException(AppMessage.ERR_CONFIG_MISSING);
            }
        }
        #endregion
    }
}
