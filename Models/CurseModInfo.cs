namespace MCModVersionChecker.Models;

internal class CurseModInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public Uri Link { get; set; } = new("https://www.curseforge.com");
    public string Status { get; set; } = string.Empty;
    public bool IsFilterMatched { get; set; }
}
