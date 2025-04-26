using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MCModVersionChecker.Enums;
using MCModVersionChecker.Models;
using MCModVersionChecker.Services;
using MCModVersionChecker.Views;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;

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
    private ModQueryResult selectedResult;

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
            GetCurseResults(ModIdText.Split("\r\n", StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList());

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

    private async void GetCurseResults(List<string>? ids)
    {
        ICurseForgeService curseForgeService = new CurseForgeService(); //I'm too lazy to implement DI for a simple task like this. Just allocate CurseForgeService directly.

        var mods = await curseForgeService.GetModsByIdsAsync(ids, VersionText, (int)CurrentModLoader);

        Results = new(mods.Select(mod => new ModQueryResult()
        {
            ModName = mod.Name,
            ModLink = mod.Link,
            IsFilter = mod.IsFilterMatched
        }));

        Results.Sort((x, y) =>
        {
            if (x.IsFilter && !y.IsFilter)
                return -1;
            else if (!x.IsFilter && y.IsFilter)
                return 1;
            else
                return string.Compare(x.ModName, y.ModName);
        });
    }

    [RelayCommand]
    private void OpenModLink()
    {
        if (SelectedResult?.ModLink != null)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = SelectedResult.ModLink.ToString(),
                UseShellExecute = true
            });
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
