namespace MCModVersionChecker.Services;

internal interface ICurseForgeService
{
    Task<List<string>> GetModsByIdsAsync(List<string> modIds, string mcVersion, int modLoader, bool filterPcOnly = true);
}
