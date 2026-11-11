namespace MCModVersionChecker.Models;

internal class ModQueryResult
{
    public required bool IsFilter { get; set; }
    public required string ModName { get; set; }
    public required Uri ModLink { get; set; }
    public string? LatestGameVersion { get; set; }

    public string LatestVersionDisplay =>
        string.IsNullOrWhiteSpace(LatestGameVersion) ? "—" : LatestGameVersion;
}
