using FreedomAssignment;
using FreedomAssignment.Models;

namespace UnitTests;

public class DistanceCalculatorTests
{
    /// <summary>
    /// Tests the CalculateDistance method for various known city pairs.
    /// </summary>
    [Theory]
    [InlineData(51.5074, -0.1278, 48.8566, 2.3522, 343)]   // London to Paris
    [InlineData(35.6895, 139.6917, 37.7749, -122.4194, 8270)] // Tokyo to San Francisco
    [InlineData(-33.8688, 151.2093, -37.8136, 144.9631, 713)] // Sydney to Melbourne
    [InlineData(55.7558, 37.6176, 59.9343, 30.3351, 634)]   // Moscow to Saint Petersburg
    public void CalculateDistance_ShouldReturnCorrectRangeOfDistance(double lat1, double lon1, double lat2, double lon2, double expectedDistance)
    {
        // Arrange
        var point1 = new Coordinate(lon1, lat1);
        var point2 = new Coordinate(lon2, lat2);

        // Act
        double result = DistanceCalculator.CalculateDistance(point1, point2);

        // Assert
        Assert.InRange(result, expectedDistance - 10, expectedDistance + 10);
    }
}