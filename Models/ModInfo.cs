namespace MCModVersionChecker.Models;

internal class ModInfo
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required Uri Link { get; set; }
    public bool IsFilterMatched { get; set; }
    public string? Query { get; set; }
    /// <summary>Latest Minecraft game version available for the selected loader.</summary>
    public string? LatestGameVersion { get; set; }
}
