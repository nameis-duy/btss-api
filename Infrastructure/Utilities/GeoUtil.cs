using Infrastructure.Constants;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using System.Reflection;

namespace Infrastructure.Utilities
{
    public static class GeoUtil
    {
        public static Geometry GetValidRegion()
        {
            string filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, GlobalConstants.REGION_GEOJSON_PATH);
            var featureCollection = new GeoJsonReader().Read<FeatureCollection>(File.ReadAllText(filePath));
            var countryFeature = featureCollection.First();
            return countryFeature.Geometry;
        }
        static double ToRadians(double angle)
        {
            return Math.PI * angle / 180;
        }

        public static double HaversineDistance(this Point coorA, Point coorB)
        {
            var lat1 = ToRadians(coorA.Y);
            var lat2 = ToRadians(coorB.Y);
            var dLat = ToRadians(coorB.Y - coorA.Y);
            var dLon = ToRadians(coorB.X - coorA.X);

            double a = Math.Pow(Math.Sin(dLat / 2), 2) +
               Math.Pow(Math.Sin(dLon / 2), 2) *
               Math.Cos(lat1) * Math.Cos(lat2);
            double rad = 6371e3; //earth radius
            double c = 2 * Math.Asin(Math.Sqrt(a));
            return rad * c;
        }
        public static bool IsWithinHaversineDistance(this Point coorA, Point coorB, double maxDistance)
        {
            return coorA.HaversineDistance(coorB) <= maxDistance;
        }
    }
}
