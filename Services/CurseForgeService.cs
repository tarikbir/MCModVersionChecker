using System.Net.Http;
using System.Text;
using MCModVersionChecker.Enums;
using MCModVersionChecker.Helpers;
using MCModVersionChecker.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MCModVersionChecker.Services;

internal class CurseForgeService : IModLookupService
{
    private const string BaseUrl = "https://api.curseforge.com/v1";

    private readonly HttpClient _httpClient;

    public CurseForgeService()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(BaseUrl)
        };
    }

    public async Task<List<ModInfo>> LookupAsync(
        IReadOnlyList<string> queries,
        string mcVersion,
        ModLoaders loader,
        LookupModes mode,
        IProgress<LookupProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (mode is LookupModes.ByName or LookupModes.BySlug)
            throw new NotSupportedException("CurseForge currently supports lookup by Mod ID only.");

        var apiKey = Properties.Settings.Default.ApiKey;
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("CurseForge API key is missing. Set it in Settings.");

        var modIds = queries
            .Select(q => q.Trim())
            .Where(q => !string.IsNullOrWhiteSpace(q))
            .ToList();

        if (modIds.Count == 0)
            return new List<ModInfo>();

        progress?.Report(new LookupProgress
        {
            Current = 0,
            Total = modIds.Count,
            CurrentQuery = "CurseForge batch request"
        });

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);

        var modQueryModel = new CurseModQueryModel
        {
            ModIds = modIds,
            FilterPcOnly = true
        };

        var json = System.Text.Json.JsonSerializer.Serialize(modQueryModel);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var response = await _httpClient.PostAsync("/v1/mods", content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadAsStringAsync(cancellationToken);
        var jResult = JsonConvert.DeserializeObject<JObject>(result)
            ?? throw new InvalidOperationException("Failed to parse response from CurseForge API.");
        var jMods = jResult["data"]
            ?? throw new InvalidOperationException("Failed to parse response data from CurseForge API.");

        var loaderId = (int)loader;
        var found = new Dictionary<string, ModInfo>(StringComparer.OrdinalIgnoreCase);

        foreach (var jModInfo in jMods)
        {
            if (jModInfo == null)
                continue;

            var hasVersion = false;
            var loaderGameVersions = new List<string>();

            if (jModInfo["latestFilesIndexes"] is JArray latestFileArray)
            {
                foreach (var fileIndex in latestFileArray)
                {
                    var gameVersion = fileIndex["gameVersion"]?.ToString() ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(gameVersion))
                        continue;

                    var fileLoader = int.Parse(fileIndex["modLoader"]?.ToString() ?? "99");
                    if (loaderId != fileLoader)
                        continue;

                    loaderGameVersions.Add(gameVersion);
                    if (string.Equals(mcVersion, gameVersion, StringComparison.OrdinalIgnoreCase))
                        hasVersion = true;
                }
            }

            var id = jModInfo["id"]?.ToString() ?? "-1";
            found[id] = new ModInfo
            {
                Id = id,
                Name = jModInfo["name"]?.ToString() ?? "Unknown",
                Link = new Uri(jModInfo["links"]?["websiteUrl"]?.ToString() ?? "https://www.curseforge.com"),
                IsFilterMatched = hasVersion,
                LatestGameVersion = GameVersionComparer.Max(loaderGameVersions),
                Query = id
            };
        }

        var allMods = new List<ModInfo>();
        var current = 0;
        foreach (var modId in modIds)
        {
            current++;
            progress?.Report(new LookupProgress
            {
                Current = current,
                Total = modIds.Count,
                CurrentQuery = modId
            });

            if (found.TryGetValue(modId, out var info))
                allMods.Add(info);
            else
            {
                allMods.Add(new ModInfo
                {
                    Id = modId,
                    Name = modId,
                    Link = new Uri("https://www.curseforge.com/minecraft/search"),
                    IsFilterMatched = false,
                    Query = modId
                });
            }
        }

        return allMods;
    }
}
