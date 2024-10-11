using System.Text.Json;
using FreedomAssignment;
using FreedomAssignment.Models;
using Microsoft.Extensions.Configuration;

string clientsTablePath = "clients.csv";
string managersTablePath = "managers.csv";
string officesTablePath = "offices.csv";

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("config.json")
    .Build();
var geolocationConfig = new GeolocationOptions();
configuration.GetRequiredSection(nameof(GeolocationOptions)).Bind(geolocationConfig);
var geolocator = new Geolocator(geolocationConfig);

// Parse csv tables of the entities
var clients = Client.Parse(clientsTablePath);
var managers = Manager.Parse(managersTablePath);
var offices = Office.Parse(officesTablePath);

var distributor = new ClientDistributor(geolocator, clients, managers, offices);
await distributor.DistributeClients();

var clientAllocations = managers
    .SelectMany(m => m.Clients.Select(c => new ClientAllocation(m.Id, c.Id, m.ClientCount)))
    .ToList();
string resultJson = JsonSerializer.Serialize(clientAllocations);
File.WriteAllText("result.json", resultJson);