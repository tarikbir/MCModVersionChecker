using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MCModVersionChecker.Enums;
using MCModVersionChecker.Models;
using MCModVersionChecker.Services;
using MCModVersionChecker.Views;
using System.IO;

namespace MCModVersionChecker.ViewModels;

internal partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string statusText = "Ready";
    private const string fetchingModsText = "Fetching mods...";
    private int queryCount = 1;

    [ObservableProperty]
    private string modIdText = "";

    [ObservableProperty]
    private List<ModQueryResult> results = new();

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

            //Results = mods;
            queryCount++;
            if (StatusText == fetchingModsText)
            {
                StatusText = $"{fetchingModsText} ({queryCount})";
            }
            else
            {
                StatusText = fetchingModsText;
                queryCount = 1;
            }
        }
        catch (Exception ex)
        {
            StatusText = "Error: " + ex.Message;
        }
    }

    [RelayCommand]
    private void ImportFromFolder()
    {
        Microsoft.Win32.OpenFolderDialog dialog = new()
        {
            Multiselect = false,
            Title = "Select mods folder"
        };

        bool? result = dialog.ShowDialog();

        if (result == true)
        {
            string fullPathToFolder = dialog.FolderName;
            string[] files = Directory.GetFiles(fullPathToFolder);
            ModIdText = string.Join("\r\n", files.Select(x => Path.GetFileNameWithoutExtension(x)));
        }
    }

    [RelayCommand]
    private void OpenSettings()
    {
        var settings = new SettingsView();
        settings.ShowDialog();
    }
}
