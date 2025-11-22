using System.Text.Json;
using System.Text.Json.Serialization;
using RestSharp;

string merakiApiKey = "YOUR_MERAKI_API_KEY";
string merakiOrgId = "YOUR_MERAKI_ORG_ID";
string zabbixUrl = "http://your-zabbix-server/api_jsonrpc.php";
string zabbixUser = "Admin";
string zabbixPassword = "zabbix";


// === Meraki ===
var merakiClient = new RestClient("https://api.meraki.com/api/v1");
merakiClient.AddDefaultHeader("X-Cisco-Meraki-API-Key", merakiApiKey);
merakiClient.AddDefaultHeader("Accept", "application/json");

var networks = await GetNetworksAsync(merakiOrgId);
var merakiIps = new Dictionary<string, (string? wan1, string? wan2)>();

foreach (var network in networks)
{
    var devices = await GetDevicesAsync(network.Id);
    var mx = devices.FirstOrDefault(d => d.Model.StartsWith("MX"));
    if (mx != null)
    {
        var uplinks = await GetUplinkStatusAsync(mx.Serial);
        var wan1 = uplinks.FirstOrDefault(u => u.Interface == "wan1")?.Ip;
        var wan2 = uplinks.FirstOrDefault(u => u.Interface == "wan2")?.Ip;
        merakiIps[network.Name.ToLower()] = (wan1, wan2);
    }
}

// === Zabbix ===
var zabbixClient = new RestClient(zabbixUrl);
var zabbixToken = await ZabbixLoginAsync();
var zabbixIps = await GetZabbixWanIpsAsync(zabbixToken);

// === Porównanie ===
Console.WriteLine("\n=== Porównanie IP WAN1/WAN2 ===");
foreach (var sklep in merakiIps.Keys)
{
    var (merakiWan1, merakiWan2) = merakiIps[sklep];
    var zabbixWan1 = zabbixIps.GetValueOrDefault($"{sklep}_wan1");
    var zabbixWan2 = zabbixIps.GetValueOrDefault($"{sklep}_wan2");

    if (merakiWan1 != zabbixWan1 || merakiWan2 != zabbixWan2)
    {
        Console.WriteLine($"\nSklep: {sklep}");
        Console.WriteLine($"  Meraki WAN1: {merakiWan1}, Zabbix WAN1: {zabbixWan1}");
        Console.WriteLine($"  Meraki WAN2: {merakiWan2}, Zabbix WAN2: {zabbixWan2}");
    }
}

// === Meraki helpery ===
async Task<List<Network>> GetNetworksAsync(string orgId)
{
    var req = new RestRequest($"/organizations/{orgId}/networks", Method.Get);
    var res = await merakiClient.ExecuteAsync(req);
    return JsonSerializer.Deserialize<List<Network>>(res.Content) ?? new();
}

async Task<List<Device>> GetDevicesAsync(string networkId)
{
    var req = new RestRequest($"/networks/{networkId}/devices", Method.Get);
    var res = await merakiClient.ExecuteAsync(req);
    return JsonSerializer.Deserialize<List<Device>>(res.Content) ?? new();
}

async Task<List<Uplink>> GetUplinkStatusAsync(string serial)
{
    var req = new RestRequest($"/devices/{serial}/uplink", Method.Get);
    var res = await merakiClient.ExecuteAsync(req);
    return JsonSerializer.Deserialize<List<Uplink>>(res.Content) ?? new();
}

// === Zabbix helpery ===


async Task<string> ZabbixLoginAsync()
    {

        var req = new RestRequest();
        req.Method = Method.Post;
        req.AddJsonBody(new
        {
            jsonrpc = "2.0",
            method = "user.login",
            @params = new { user = zabbixUser, password = zabbixPassword },
            id = 1
        });

        // Wyślij żądanie do API
        var res = await zabbixClient.ExecuteAsync(req);

        // Parsowanie odpowiedzi JSON
        var json = JsonDocument.Parse(res.Content);

        // Pobranie tokena sesyjnego
        return json.RootElement.GetProperty("result").GetString();
    }

async Task<Dictionary<string, string>> GetZabbixWanIpsAsync(string token)
{
    var req = new RestRequest();
    req.Method = Method.Post;
    req.AddJsonBody(new
    {
        jsonrpc = "2.0",
        method = "item.get",
        @params = new
        {
            output = new[] { "name", "lastvalue" },
            search = new { name = "_wan" },
            sortfield = "name"
        },
        auth = token,
        id = 2
    });

    var res = await zabbixClient.ExecuteAsync(req);
    var json = JsonDocument.Parse(res.Content);
    var items = json.RootElement.GetProperty("result");

    var dict = new Dictionary<string, string>();
    foreach (var item in items.EnumerateArray())
    {
        var name = item.GetProperty("name").GetString();
        var value = item.GetProperty("lastvalue").GetString();
        dict[name.ToLower()] = value;
    }
    return dict;
}

// === Modele ===
record Network(string Id, string Name);
record Device(string Serial, string Model);
record Uplink(string Interface, string Ip);
