using MCModVersionChecker.Models;

namespace MCModVersionChecker.Services;

internal interface ICurseForgeService
{
    Task<List<CurseModInfo>> GetModsByIdsAsync(List<string> modIds, string mcVersion, int modLoader, bool filterPcOnly = true);
}
