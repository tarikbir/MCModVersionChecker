namespace MCModVersionChecker.Models;

internal class CurseModInfo
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Summary { get; set; }
    public Uri Link { get; set; }
    public string Status { get; set; }
    public bool IsFilterMatched { get; set; }
}
