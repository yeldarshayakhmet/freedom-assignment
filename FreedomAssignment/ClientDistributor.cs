using FreedomAssignment.Models;

namespace FreedomAssignment;

public class ClientDistributor
{
    /// <summary>
    /// Home country possible names.
    /// </summary>
    private static readonly string[] HomeOfficeCountry = ["Казахстан", "Kazahstan"];
    /// <summary>
    /// Offices managers of which service foreign clients.
    /// </summary>
    private static readonly string[] OfficeForForeignersIds = ["Отдел 1", "Отдел 2"];
    /// <summary>
    /// All clients.
    /// </summary>
    private readonly List<Client> _clients;
    /// <summary>
    /// All offices.
    /// </summary>
    private readonly List<Office> _offices;
    /// <summary>
    /// The geolocation service.
    /// </summary>
    private readonly Geolocator _geolocator;
    /// <summary>
    /// Map of offices by their IDs.
    /// </summary>
    private readonly Dictionary<string, Office> _officeMap = new(StringComparer.OrdinalIgnoreCase);
    /// <summary>
    /// Managers servicing VIP clients.
    /// </summary>
    private readonly List<Manager> _vipManagers = [];
    /// <summary>
    /// Managers servicing foreign clients.
    /// </summary>
    private readonly List<Manager> _managersForForeigners = [];

    public ClientDistributor(Geolocator geolocator, List<Client> clients, List<Manager> managers, List<Office> offices)
    {
        _clients = clients;
        _offices = offices;
        _geolocator = geolocator;
        MapOfficesById(_offices);
        AssignManagersToManagerPools(managers);
    }

    /// <summary>
    /// Calls the geolocation API and maps the clients' city names to their respective geolocation task.
    /// </summary>
    /// <param name="clients">All clients.</param>
    /// <returns>A dictionary mapping the clients' city names to their geolocation task.</returns>
    private Dictionary<string, Task<Coordinate>> FetchCityLocations(List<Client> clients)
    {
        var cityToLocation = new Dictionary<string, Task<Coordinate>>();
        foreach (var client in clients)
        {
            // Skip a client if we already mapped their city
            // Skip clients that do not have an address city
            // Skip foreign clients
            if (cityToLocation.ContainsKey(client.City)
                || string.IsNullOrWhiteSpace(client.City)
                || !HomeOfficeCountry.Contains(client.Country))
            {
                continue;
            }

            cityToLocation[client.City] = _geolocator.LocateCityAsync(client.City, "ru_RU");
        }

        return cityToLocation;
    }

    /// <summary>
    /// Makes a map of office IDs to its Office object to reference by ID later.
    /// </summary>
    /// <param name="offices">All offices.</param>
    private void MapOfficesById(List<Office> offices)
    {
        // We use ordinal comparison because strings in Cyrillic are compared incorrectly
        foreach (var office in offices)
        {
            _officeMap[office.Id] = office;
        }
    }

    /// <summary>
    /// Splits managers to their respective offices, separate VIP managers, and managers servicing foreign clients.
    /// </summary>
    /// <param name="managers">All managers.</param>
    private void AssignManagersToManagerPools(List<Manager> managers)
    {
        // Each office has its own managers
        // VIP managers and managers handling foreign clients are in separate pools
        foreach (var manager in managers)
        {
            if (manager.IsVIP)
            {
                _vipManagers.Add(manager);
            }

            if (OfficeForForeignersIds.Contains(manager.Office, StringComparer.OrdinalIgnoreCase))
            {
                _managersForForeigners.Add(manager);
            }

            _officeMap[manager.Office].Managers.Add(manager);
        }
    }

    /// <summary>
    /// Distributes clients to managers by office proximity and client status
    /// </summary>
    public async Task DistributeClients()
    {
        // Helper methods to reduce code repetition and improve readability
        Office GetClosestOfficeByDistance(Coordinate clientLocation, List<Office> officesToCheckDistance)
        {
            var officeDistances = officesToCheckDistance
                .Select(office => (Office: office, Distance: DistanceCalculator.CalculateDistance(office.Location, clientLocation)))
                .ToList();
            return officeDistances.MinBy(x => x.Distance).Office;
        }

        void AssignClientToManager(List<Manager> assignableManagers, Client client)
        {
            var targetManager = assignableManagers.MinBy(m => m.ClientCount);
            targetManager.Clients.Add(client);
        }

        var cityLocations = FetchCityLocations(_clients);

        // Distribute clients to managers
        foreach (var client in _clients)
        {
            // If the client is VIP, they are serviced by VIP managers
            if (client.IsVIP)
            {
                AssignClientToManager(_vipManagers, client);
                continue;
            }

            if (cityLocations.TryGetValue(client.City, out var location))
            {
                client.Location = await location;
            }

            // If the client is foreign, or has no determinable address, they are serviced by a separate pool of managers from Office 1 and 2
            if (!HomeOfficeCountry.Contains(client.City, StringComparer.OrdinalIgnoreCase)
                || string.IsNullOrWhiteSpace(client.City)
                || client.Location is null)
            {
                AssignClientToManager(_managersForForeigners, client);
                continue;
            }

            var targetOffice = GetClosestOfficeByDistance(client.Location, _offices);
            AssignClientToManager(targetOffice.Managers, client);
        }
    }
}