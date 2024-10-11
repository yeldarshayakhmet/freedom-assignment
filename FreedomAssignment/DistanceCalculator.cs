using FreedomAssignment.Models;

namespace FreedomAssignment;

public class DistanceCalculator
{
    /// <summary>
    /// Calculates the distance between two latitude/longitude points using the Haversine formula.
    /// </summary>
    /// <param name="p1">Coordinate of point 1</param>
    /// <param name="p2">Coordinate of point 2</param>
    /// <returns>Distance between the two points in kilometers</returns>
    public static double CalculateDistance(Coordinate p1, Coordinate p2)
    {
        // Earth's radius in kilometers
        const double r = 6378.137;

        // Convert degrees to radians
        double lat1Rad = DegreesToRadians(p1.Latitude);
        double lat2Rad = DegreesToRadians(p2.Latitude);
        double deltaLatRad = DegreesToRadians(p2.Latitude - p1.Latitude);
        double deltaLonRad = DegreesToRadians(p2.Longitude - p1.Longitude);

        // Haversine formula of distance on a sphere
        double a = Math.Sin(deltaLatRad / 2) * Math.Sin(deltaLatRad / 2) +
                   Math.Cos(lat1Rad) * Math.Cos(lat2Rad) * Math.Sin(deltaLonRad / 2) * Math.Sin(deltaLonRad / 2);

        double distance = 2 * r * Math.Asin(Math.Sqrt(a));

        // Distance in kilometers
        return distance;
    }

    /// <summary>
    /// Converts degrees to radians.
    /// </summary>
    private static double DegreesToRadians(double degrees)
    {
        return degrees * (Math.PI / 180);
    }
}