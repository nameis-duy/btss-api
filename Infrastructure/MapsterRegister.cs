using Application.DTOs.Destination;
using Application.DTOs.Order;
using Application.DTOs.Plan;
using Application.DTOs.Product;
using Application.DTOs.Provider;
using Application.DTOs.Staff_Admin;
using Application.DTOs.Traveler;
using Domain.Entities;
using Domain.Enums.Others;
using Domain.Enums.Provider;
using Domain.JsonEntities;
using Infrastructure.Constants;
using Infrastructure.Utilities;
using Mapster;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;

namespace Infrastructure
{
    public class MapsterRegister : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<Point, Point>().MapWith(p => p);
            config.NewConfig<Coordinate, Point>().MapWith(p => new Point(p) { SRID = 4326 });
            config.NewConfig<TravelerCreate, Account>()
                  .Map(dest => dest.Role, src => Role.TRAVELER)
                  .Map(dest => dest.AvatarPath,
                       src => src.AvatarUrl!.Replace(ValidationConstants.IMAGE_SOURCE, string.Empty),
                       src => src.AvatarUrl != null);
            config.NewConfig<StaffCreate, Account>().Map(dest => dest.PasswordHash, src => src.Password.Hash());
            config.NewConfig<DestinationCreate, Destination>()
                  .Map(dest => dest.UnaccentName, src => src.Name.RemoveDiacritics())
                  .Map(dest => dest.ImagePaths,
                       src => src.ImageUrls.Select(url => url.Replace(ValidationConstants.IMAGE_SOURCE, string.Empty)));
            config.NewConfig<DestinationUpdate, Destination>()
                  .Map(dest => dest.UnaccentName, src => src.Name.RemoveDiacritics())
                  .Map(dest => dest.ImagePaths,
                       src => src.ImageUrls.Select(url => url.Replace(ValidationConstants.IMAGE_SOURCE, string.Empty)));
            config.NewConfig<ProviderCreate, Provider>()
                  .Map(dest => dest.ImagePath, src => src.ImageUrl.Replace(ValidationConstants.IMAGE_SOURCE, string.Empty));
            config.NewConfig<ProductCreate, Product>()
                  .Map(dest => dest.ImagePath, src => src.ImageUrl.Replace(ValidationConstants.IMAGE_SOURCE, string.Empty))
                  .Map(dest => dest.Periods,
                       src => new List<Period> { Period.MORNING, Period.NOON, Period.AFTERNOON, Period.EVENING },
                       src => src.Type != ProductType.FOOD && src.Type != ProductType.BEVERAGE);
            config.NewConfig<PlanCreate, Plan>()
                  .Map(dest => dest.UtcDepartAt, src => src.DepartAt.UtcDateTime)
                  .Map(dest => dest.Offset, src => src.DepartAt.Offset)
                  .Map(dest => dest.Schedule, src => JsonConvert.SerializeObject(src.Schedule));
            config.NewConfig<PlanUpdate, Plan>()
                  .Ignore(dest => dest.Schedule)
                  .Map(dest => dest.UtcDepartAt, src => src.DepartAt.UtcDateTime)
                  .Map(dest => dest.Offset, src => src.DepartAt.Offset)
                  .Map(dest => dest.Schedule, src => JsonConvert.SerializeObject(src.Schedule));
            config.NewConfig<SurchargeUpdate, Surcharge>()
                  .Map(dest => dest.ImagePath, src => src.ImageUrl.Replace(ValidationConstants.IMAGE_SOURCE, string.Empty));
            config.NewConfig<TravelerUpdate, Account>()
                  .Map(dest => dest.AvatarPath,
                       src => src.AvatarUrl!.Replace(ValidationConstants.IMAGE_SOURCE, string.Empty),
                       src => src.AvatarUrl != null);
        }
    }
}
