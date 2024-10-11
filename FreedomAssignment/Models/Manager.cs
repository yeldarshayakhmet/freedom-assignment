using Microsoft.VisualBasic.FileIO;

namespace FreedomAssignment.Models;

public class Manager
{
    private int InitialClientCount { get; init; }
    public string Id { get; init; }
    public string Office { get; init; }
    public int ClientCount => InitialClientCount + Clients.Count;
    public List<Client> Clients { get; } = [];
    public bool IsVIP { get; init; }

    /// <summary>
    /// Parses managers from a csv file.
    /// </summary>
    /// <param name="filePath">The path to the csv file.</param>
    /// <returns>A list of parsed managers.</returns>
    public static List<Manager> Parse(string filePath)
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


        var result = new List<Manager>();
        while (!parser.EndOfData)
        {
            string[] fields = parser.ReadFields();
            if (fields is null || fields.Length == 0)
            {
                continue;
            }

            var manager = new Manager
            {
                Id = fields[0],
                Office = fields[1].Trim(),
                InitialClientCount = int.TryParse(fields[2], out var clientCount) ? clientCount : 0,
                // We care only about if the manager services VIP clients, so we ignore other attributes.
                IsVIP = fields[3].Contains("vip", StringComparison.OrdinalIgnoreCase),
            };
            result.Add(manager);
        }

        return result;
    }
}