using FluentValidation;
using Infrastructure.Constants;
using Mapster;
using NetTopologySuite.Geometries;

namespace Infrastructure.Validators
{
    public static class CustomRules
    {
        public static IRuleBuilderOptions<T, string> FromValidSource<T>(this IRuleBuilder<T, string> builder,
                                                                        string source = ValidationConstants.IMAGE_SOURCE)
        {
            return builder.Must((parent, url) => url.StartsWith(source));
        }
        public static IRuleBuilderOptions<T, ICollection<string>> AllFromValidSource<T>(this IRuleBuilder<T, ICollection<string>> builder,
                                                                                        string source = ValidationConstants.IMAGE_SOURCE)
        {
            return builder.Must((parent, urls) => urls.Any(url => !url.StartsWith(source)) == false);
        }
        public static IRuleBuilderOptions<T, ICollection<TChild>> Count<T, TChild> (this IRuleBuilder<T, ICollection<TChild>> builder,
                                                                                    int min,
                                                                                    int max) 
        {
            return builder.Must((parent, collection) => collection.Count >= min && collection.Count <= max);
        }
        public static IRuleBuilderOptions<T, Coordinate> IsInsideGeometry<T>(this IRuleBuilder<T, Coordinate> builder,
                                                                             Geometry geometry)
        {
            return builder.Must((parent, coor) => geometry.Contains(coor.Adapt<Point>()));
        }
        public static IRuleBuilderOptions<T, IEnumerable<TEnum>> AllValuesValid<T, TEnum>(this IRuleBuilder<T, IEnumerable<TEnum>> builder) where TEnum : Enum
        {
            return builder.ForEach(r => r.IsInEnum());
        }
        public static IRuleBuilderOptions<T, ICollection<DateOnly>> DatesInclusiveBetween<T>(this IRuleBuilder<T, ICollection<DateOnly>> builder, DateOnly min, DateOnly max)
        {
            return builder.Must((parent, dates) => dates.Any(date => date <= min || date >= max) == false);
        }
        public static IRuleBuilderOptions<T, ICollection<TChild>> MaxCount<T, TChild>(this IRuleBuilder<T, ICollection<TChild>> builder, int maxCount)
        {
            return builder.Must((parent, children) => children.Count <= maxCount);
        }
        //public static IRuleBuilderOptions<T, TChild> WithMessage<T, TChild>(this IRuleBuilder<T, TChild> builder)
    }
}
