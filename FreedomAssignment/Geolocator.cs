using System.Globalization;
using System.Text.Json;
using FreedomAssignment.Models;

namespace FreedomAssignment;

public class Geolocator(GeolocationOptions options)
{
    private const string API = "https://geocode-maps.yandex.ru/1.x/";
    private readonly GeolocationOptions _options = options;

    /// <summary>
    /// Performs geolocation by the name of the city
    /// </summary>
    /// <param name="cityName">The name of the city</param>
    /// <param name="language">Response language and regional settings, where the two-letter language code is in ISO 639-1, and the two-letter region code in ISO 3166-1 format</param>
    /// <returns></returns>
    public async Task<Coordinate> LocateCityAsync(string cityName, string language)
    {
        // Construct the API URI with query parameters
        var url = await GetApiUri(cityName, language);

        using var client = new HttpClient();

        try
        {
            var response = await client.GetAsync(url);
            Console.WriteLine(url.ToString());
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Fetching geolocation data failed: {response.StatusCode} {await response.Content.ReadAsStringAsync()}");
                return null;
            }

            // Get the coordinate position in the JSON response, which is a single string in the form of [longitude latitude]: "12.345 56.789"
            using JsonDocument document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
            var position = document.RootElement
                .GetProperty("response")
                .GetProperty("GeoObjectCollection")
                .GetProperty("featureMember")[0]
                .GetProperty("GeoObject")
                .GetProperty("Point")
                .GetProperty("pos");

            var stringPositions = position.GetString()?.Split(' ');
            if (stringPositions is null)
            {
                return null;
            }

            var longitude = double.Parse(stringPositions[0], NumberStyles.Float, CultureInfo.InvariantCulture);
            var latitude = double.Parse(stringPositions[1], NumberStyles.Float, CultureInfo.InvariantCulture);
            return new Coordinate(longitude, latitude);

        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"An error occurred while fetching geolocation data: {e.Message}");
        }

        return null;
    }

    private async Task<Uri> GetApiUri(string cityName, string language)
    {
        var builder = new UriBuilder(API);
        var parameters = new Dictionary<string, string>
        {
            { "apikey", _options.APIKey },
            { "geocode", cityName },
            { "lang", language },
            { "format", "json" }
        };
        var encodedParameters = new FormUrlEncodedContent(parameters);
        var query = await encodedParameters.ReadAsStringAsync();
        builder.Query = query;
        var url = builder.Uri;
        return url;
    }
}