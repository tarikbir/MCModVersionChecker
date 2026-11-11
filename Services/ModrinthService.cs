using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using MCModVersionChecker.Enums;
using MCModVersionChecker.Helpers;
using MCModVersionChecker.Models;
using Newtonsoft.Json.Linq;

namespace MCModVersionChecker.Services;

internal class ModrinthService : IModLookupService
{
    private const string BaseUrl = "https://api.modrinth.com/v2/";
    private const string UserAgent = "MCModVersionChecker/1.0 (github.com/tarikbir/MCModVersionChecker)";

    private readonly HttpClient _httpClient;

    public ModrinthService()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(BaseUrl)
        };
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<List<ModInfo>> LookupAsync(
        IReadOnlyList<string> queries,
        string mcVersion,
        ModLoaders loader,
        LookupModes mode,
        IProgress<LookupProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var results = new List<ModInfo>();
        var loaderName = ToModrinthLoader(loader);
        var total = queries.Count(q => !string.IsNullOrWhiteSpace(q));
        var current = 0;

        foreach (var rawQuery in queries)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var query = rawQuery.Trim();
            if (string.IsNullOrWhiteSpace(query))
                continue;

            current++;
            progress?.Report(new LookupProgress
            {
                Current = current,
                Total = total,
                CurrentQuery = query
            });

            try
            {
                var project = mode switch
                {
                    LookupModes.ByName => await FindProjectByNameAsync(query, cancellationToken),
                    LookupModes.ById or LookupModes.BySlug => await GetProjectAsync(query, cancellationToken),
                    _ => null
                };

                if (project == null)
                {
                    results.Add(CreateMissing(query));
                    continue;
                }

                var (hasMatch, latestGameVersion) =
                    await GetVersionInfoAsync(project.Id, mcVersion, loaderName, cancellationToken);

                results.Add(new ModInfo
                {
                    Id = project.Id,
                    Name = project.Name,
                    Link = project.Link,
                    IsFilterMatched = hasMatch,
                    LatestGameVersion = latestGameVersion,
                    Query = query
                });
            }
            catch (HttpRequestException)
            {
                results.Add(CreateMissing(query));
            }
        }

        return results;
    }

    private async Task<ModInfo?> FindProjectByNameAsync(string name, CancellationToken cancellationToken)
    {
        var facets = JsonSerializer.Serialize(new[] { new[] { "project_type:mod" } });
        var url =
            $"search?query={Uri.EscapeDataString(name)}&limit=5&facets={Uri.EscapeDataString(facets)}";

        using var response = await _httpClient.GetAsync(url, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var root = JObject.Parse(json);
        var hits = root["hits"] as JArray;
        if (hits == null || hits.Count == 0)
            return null;

        var exact = hits.FirstOrDefault(h =>
            string.Equals(h["title"]?.ToString(), name, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(h["slug"]?.ToString(), name, StringComparison.OrdinalIgnoreCase));

        var hit = exact ?? hits[0];
        var slug = hit["slug"]?.ToString();
        var id = hit["project_id"]?.ToString() ?? string.Empty;
        var title = hit["title"]?.ToString() ?? name;

        return new ModInfo
        {
            Id = id,
            Name = title,
            Link = BuildProjectUri(slug ?? id),
            IsFilterMatched = false,
            Query = name
        };
    }

    private async Task<ModInfo?> GetProjectAsync(string idOrSlug, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync($"project/{Uri.EscapeDataString(idOrSlug)}", cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var root = JObject.Parse(json);
        var id = root["id"]?.ToString() ?? idOrSlug;
        var title = root["title"]?.ToString() ?? idOrSlug;
        var slug = root["slug"]?.ToString() ?? id;

        return new ModInfo
        {
            Id = id,
            Name = title,
            Link = BuildProjectUri(slug),
            IsFilterMatched = false,
            Query = idOrSlug
        };
    }

    private async Task<(bool HasMatch, string? LatestGameVersion)> GetVersionInfoAsync(
        string projectId,
        string mcVersion,
        string loader,
        CancellationToken cancellationToken)
    {
        var loaders = JsonSerializer.Serialize(new[] { loader });
        var url =
            $"project/{Uri.EscapeDataString(projectId)}/version?loaders={Uri.EscapeDataString(loaders)}&include_changelog=false";

        using var response = await _httpClient.GetAsync(url, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return (false, null);

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var array = JArray.Parse(json);
        if (array.Count == 0)
            return (false, null);

        var gameVersions = new List<string>();
        var hasMatch = false;

        foreach (var version in array)
        {
            if (version["game_versions"] is not JArray versionGames)
                continue;

            foreach (var gameVersionToken in versionGames)
            {
                var gameVersion = gameVersionToken?.ToString();
                if (string.IsNullOrWhiteSpace(gameVersion))
                    continue;

                gameVersions.Add(gameVersion);
                if (string.Equals(gameVersion, mcVersion, StringComparison.OrdinalIgnoreCase))
                    hasMatch = true;
            }
        }

        return (hasMatch, GameVersionComparer.Max(gameVersions));
    }

    private static ModInfo CreateMissing(string query) => new()
    {
        Id = string.Empty,
        Name = query,
        Link = new Uri("https://modrinth.com/mods"),
        IsFilterMatched = false,
        Query = query
    };

    private static Uri BuildProjectUri(string slugOrId) =>
        new($"https://modrinth.com/mod/{slugOrId}");

    private static string ToModrinthLoader(ModLoaders loader) => loader switch
    {
        ModLoaders.Forge => "forge",
        ModLoaders.Fabric => "fabric",
        ModLoaders.Quilt => "quilt",
        ModLoaders.NeoForge => "neoforge",
        ModLoaders.LiteLoader => "liteloader",
        ModLoaders.Cauldron => "cauldron",
        _ => "forge"
    };
}
