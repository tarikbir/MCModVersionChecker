using MCModVersionChecker.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Text;

namespace MCModVersionChecker.Services;

internal class CurseForgeService : ICurseForgeService
{
    private const string BASE_URL = "https://api.curseforge.com/v1";
    
    private readonly HttpClient _httpClient;

    public CurseForgeService()
    {
        _httpClient = new HttpClient()
        {
            BaseAddress = new Uri(BASE_URL)
        };
    }

    public async Task<List<CurseModInfo>> GetModsByIdsAsync(List<string> modIds, string mcVersion, int modLoader, bool filterPcOnly = true)
    {
        var apiKey = Properties.Settings.Default.ApiKey;

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("API key could not be found!");

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);

        CurseModQueryModel modQueryModel = new()
        {
            ModIds = modIds,
            FilterPcOnly = filterPcOnly
        };

        var json = System.Text.Json.JsonSerializer.Serialize(modQueryModel);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("/v1/mods", content);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadAsStringAsync();

        var jResult = JsonConvert.DeserializeObject<JObject>(result) ?? throw new InvalidOperationException("Failed to parse response from CurseForge API.");
        var jMods = jResult["data"] ?? throw new InvalidOperationException("Failed to parse response data from CurseForge API.");

        List<CurseModInfo> allMods = new List<CurseModInfo>();

        //Build mod list as CurseModInfo
        foreach (var jModInfo in jMods)
        {
            if (jModInfo == null)
                continue;

            bool hasVersion = false;
            if (jModInfo["latestFilesIndexes"] != null && jModInfo["latestFilesIndexes"] is JArray latestFileArray)
            {
                foreach (var fileIndex in latestFileArray)
                {
                    if (mcVersion == (fileIndex["gameVersion"]?.ToString() ?? string.Empty) && modLoader == int.Parse(fileIndex["modLoader"]?.ToString() ?? "99"))
                    {
                        hasVersion = true;
                        break;
                    }
                }
            }
            
            CurseModInfo info = new CurseModInfo
            {
                Name = jModInfo["name"]?.ToString() ?? "Unknown",
                Id = jModInfo["id"]?.ToString() ?? "-1",
                Link = new Uri(jModInfo["links"]?["websiteUrl"]?.ToString() ?? "localhost"),
                Summary = jModInfo["summary"]?.ToString() ?? "",
                Status = StatusPairs[int.Parse(jModInfo["status"]?.ToString() ?? "0")],
                IsFilterMatched = hasVersion
            };

            allMods.Add(info);
        }

        return allMods;
    }

    private static Dictionary<int, string> StatusPairs = new()
    {
        { 0, "Error" },
        { 1, "New" },
        { 2, "Changes Required" },
        { 3, "Under Soft Review" },
        { 4, "Approved" },
        { 5, "Rejected" },
        { 6, "Changes Made" },
        { 7, "Inactive" },
        { 8, "Abandoned" },
        { 9, "Deleted" },
        { 10, "Under Review" }
    };
}
