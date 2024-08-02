using Application.DTOs.Generic;
using Infrastructure.Constants;
using System.Reflection;

namespace API.GraphQL.Types
{
    public class AppConfigType : ObjectType<AppConfig>
    {
        protected override void Configure(IObjectTypeDescriptor<AppConfig> descriptor)
        {
            foreach (var property in typeof(AppConfig).GetProperties())
            {
                descriptor.Field(property).Name(property.Name);
            }
            descriptor.Field("LAST_MODIFIED").Type<DateTimeType>().Resolve(context =>
            {
                var basePath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
                var configFilePath = System.IO.Path.Combine(basePath, GlobalConstants.CONFIG_PATH);
                var configInfo = new FileInfo(configFilePath);
                if (configInfo.Exists) return configInfo.LastWriteTimeUtc;
                return null;
            });

            descriptor.Field(c => c.USE_FIXED_OTP).Description("Using fixed otp for testing");
        }
    }
}
