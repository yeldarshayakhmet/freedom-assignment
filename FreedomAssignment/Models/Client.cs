using Microsoft.VisualBasic.FileIO;

namespace FreedomAssignment.Models;

public class Client
{
    public string Id { get; init; }
    public string Country { get; init; }
    public string City { get; init; }
    public bool IsVIP { get; init; }
    public Coordinate Location { get; set; }

    /// <summary>
    /// Parses clients from a csv file.
    /// </summary>
    /// <param name="filePath">The path to the csv file.</param>
    /// <returns>A list of parsed clients.</returns>
    public static List<Client> Parse(string filePath)
    {
        // TextFieldParser methods are synchronous, so it is a limitation
        using var parser = new TextFieldParser(filePath);
        parser.TextFieldType = FieldType.Delimited;
        parser.SetDelimiters(",");
        parser.HasFieldsEnclosedInQuotes = true;
        // Skip the header line
        if (!parser.EndOfData)
        {
            parser.ReadFields();
        }

        var result = new List<Client>();
        while (!parser.EndOfData)
        {
            string[] fields = parser.ReadFields();
            if (fields is null || fields.Length == 0)
            {
                continue;
            }

            var client = new Client
            {
                Id = fields[0],
                Country = fields[1],
                City = fields[2],
                // We care only about if the client is VIP, so we ignore parsing other attributes.
                IsVIP = fields[3].Contains("vip", StringComparison.OrdinalIgnoreCase)
            };
            result.Add(client);
        }

        return result;
    }
}