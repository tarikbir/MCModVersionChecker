using MCModVersionChecker.Enums;
using MCModVersionChecker.Models;

namespace MCModVersionChecker.Services;

internal interface IModLookupService
{
    Task<List<ModInfo>> LookupAsync(
        IReadOnlyList<string> queries,
        string mcVersion,
        ModLoaders loader,
        LookupModes mode,
        IProgress<LookupProgress>? progress = null,
        CancellationToken cancellationToken = default);
}
