namespace Jex.Assessment.Api.Companies;

internal static class Haversine
{
    private const double EarthRadiusKilometres = 6_371.0088;

    public static double DistanceInKilometres(
        double latitude1,
        double longitude1,
        double latitude2,
        double longitude2)
    {
        var latitudeDelta = ToRadians(latitude2 - latitude1);
        var longitudeDelta = ToRadians(longitude2 - longitude1);
        var latitude1Radians = ToRadians(latitude1);
        var latitude2Radians = ToRadians(latitude2);

        var haversine = Math.Pow(Math.Sin(latitudeDelta / 2), 2) +
                        Math.Cos(latitude1Radians) * Math.Cos(latitude2Radians) *
                        Math.Pow(Math.Sin(longitudeDelta / 2), 2);

        return EarthRadiusKilometres * 2 * Math.Atan2(Math.Sqrt(haversine), Math.Sqrt(1 - haversine));
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;
}
