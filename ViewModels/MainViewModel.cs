using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MCModVersionChecker.Enums;
using MCModVersionChecker.Services;
using MCModVersionChecker.Views;

namespace MCModVersionChecker.ViewModels;

internal partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string modIdText = "";

    [ObservableProperty]
    private List<string> results = new();

    [ObservableProperty]
    private string versionText = "1.21.1";

    [ObservableProperty]
    private List<ModLoaders> modLoaderTypes = new List<ModLoaders>()
    {
        ModLoaders.Forge,
        ModLoaders.Fabric,
        ModLoaders.NeoForge
    };

    [ObservableProperty]
    private ModLoaders currentModLoader = ModLoaders.Forge;

    [RelayCommand]
    private async Task FetchMods()
    {
        try
        {
            var ids = ModIdText.Split("\r\n", StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();

            ICurseForgeService curseForgeService = new CurseForgeService(); //I'm too lazy to implement DI for a simple task like this. Just allocate CurseForgeService directly.

            var mods = await curseForgeService.GetModsByIdsAsync(ids, VersionText, (int)CurrentModLoader);

            Results = mods;
        }
        catch (Exception ex)
        {
            Results.Clear();
            Results.Add($"Error:\n{ex.Message}");
        }
    }

    [RelayCommand]
    private void OpenSettings()
    {
        var settings = new SettingsView();
        settings.ShowDialog();
    }
}
