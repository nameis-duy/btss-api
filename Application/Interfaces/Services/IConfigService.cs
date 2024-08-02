
using Application.DTOs.Generic;

namespace Application.Interfaces.Services
{
    public interface IConfigService
    {
        DateTime ResetSystemDateTime();
        DateTime SetSystemDateTime(DateTime dateTime);
        bool UpdateConfig(AppConfig dto);
    }
}
