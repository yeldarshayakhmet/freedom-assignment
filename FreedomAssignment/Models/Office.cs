using System.Globalization;
using Microsoft.VisualBasic.FileIO;

namespace FreedomAssignment.Models;

public class Office
{
    public string Id { get; set; }
    public Coordinate Location { get; set; }

    public List<Manager> Managers { get; set; } = [];

    /// <summary>
    /// Parses offices from a csv file.
    /// </summary>
    /// <param name="filePath">The path to the csv file.</param>
    /// <returns>A list of parsed offices.</returns>
    public static List<Office> Parse(string filePath)
    {
        using var parser = new TextFieldParser(filePath);
        parser.TextFieldType = FieldType.Delimited;
        parser.SetDelimiters(",");
        parser.HasFieldsEnclosedInQuotes = true;
        // Skip the header line
        if (!parser.EndOfData)
        {
            parser.ReadFields();
        }


        var result = new List<Office>();
        while (!parser.EndOfData)
        {
            string[] fields = parser.ReadFields();
            if (fields is null || fields.Length == 0)
            {
                continue;
            }

            // If one of the coordinate fields fails to be parsed, discard the other and assign nulls
            bool isCoordinateParsed = double.TryParse(fields[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var latitude);
            double longitude = 0;
            if (isCoordinateParsed)
            {
                isCoordinateParsed = double.TryParse(fields[2], NumberStyles.Float, CultureInfo.InvariantCulture, out longitude);
            }

            var office = new Office
            {
                Id = fields[0],
                Location = isCoordinateParsed ? new Coordinate(longitude, latitude) : null,
            };
            result.Add(office);
        }

        return result;
    }
}