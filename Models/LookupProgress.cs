namespace MCModVersionChecker.Models;

internal sealed class LookupProgress
{
    public required int Current { get; init; }
    public required int Total { get; init; }
    public string? CurrentQuery { get; init; }
}
