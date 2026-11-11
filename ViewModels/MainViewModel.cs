using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MCModVersionChecker.Enums;
using MCModVersionChecker.Models;
using MCModVersionChecker.Services;
using MCModVersionChecker.Views;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace MCModVersionChecker.ViewModels;

internal partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string statusText = "Ready";

    private string lastStatusText = string.Empty;
    private int queryCount = 1;
    private bool _suppressPreferenceSave;

    [ObservableProperty]
    private string modIdText = "";

    [ObservableProperty]
    private ModQueryResult? selectedResult;

    [ObservableProperty]
    private List<ModQueryResult> results = new();

    [ObservableProperty]
    private string versionText = "1.21.1";

    [ObservableProperty]
    private List<ModLoaders> modLoaderTypes = new()
    {
        ModLoaders.Forge,
        ModLoaders.Fabric,
        ModLoaders.NeoForge,
        ModLoaders.Quilt
    };

    [ObservableProperty]
    private ModLoaders currentModLoader = ModLoaders.Forge;

    [ObservableProperty]
    private ModSources currentSource = ModSources.Modrinth;

    [ObservableProperty]
    private LookupModes currentLookupMode = LookupModes.BySlug;

    [ObservableProperty]
    private bool isModrinthSelected = true;

    [ObservableProperty]
    private bool isCurseForgeSelected;

    [ObservableProperty]
    private bool isLookupById;

    [ObservableProperty]
    private bool isLookupByName;

    [ObservableProperty]
    private bool isLookupBySlug = true;

    [ObservableProperty]
    private string listHeaderText = "Mod slugs (line separated)";

    [ObservableProperty]
    private string sourceStatusText = "Source: Modrinth · Lookup: By Slug";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(FetchModsCommand))]
    private bool isBusy;

    [ObservableProperty]
    private double progressPercent;

    [ObservableProperty]
    private bool isProgressIndeterminate;

    public MainViewModel()
    {
        LoadPreferences();
    }

    private void LoadPreferences()
    {
        _suppressPreferenceSave = true;
        try
        {
            var settings = Properties.Settings.Default;

            VersionText = string.IsNullOrWhiteSpace(settings.DefaultMcVersion)
                ? "1.21.1"
                : settings.DefaultMcVersion;

            if (Enum.TryParse<ModLoaders>(settings.DefaultModLoader, out var loader) &&
                loader is ModLoaders.Forge or ModLoaders.Fabric or ModLoaders.NeoForge or ModLoaders.Quilt)
            {
                CurrentModLoader = loader;
            }

            if (Enum.TryParse<ModSources>(settings.PreferredSource, out var source))
                ApplySource(source, persist: false);
            else
                ApplySource(ModSources.Modrinth, persist: false);

            if (Enum.TryParse<LookupModes>(settings.PreferredLookupMode, out var mode))
                ApplyLookupMode(mode, persist: false);
            else
                ApplyLookupMode(LookupModes.BySlug, persist: false);

            RefreshListHeader();
            RefreshSourceStatus();
        }
        finally
        {
            _suppressPreferenceSave = false;
        }
    }

    partial void OnCurrentModLoaderChanged(ModLoaders value)
    {
        if (_suppressPreferenceSave)
            return;

        Properties.Settings.Default.DefaultModLoader = value.ToString();
        Properties.Settings.Default.Save();
    }

    partial void OnVersionTextChanged(string value)
    {
        if (_suppressPreferenceSave)
            return;

        Properties.Settings.Default.DefaultMcVersion = value?.Trim() ?? string.Empty;
        Properties.Settings.Default.Save();
    }

    [RelayCommand]
    private void SelectModrinth() => ApplySource(ModSources.Modrinth);

    [RelayCommand]
    private void SelectCurseForge() => ApplySource(ModSources.CurseForge);

    [RelayCommand]
    private void SelectLookupById() => ApplyLookupMode(LookupModes.ById);

    [RelayCommand]
    private void SelectLookupByName() => ApplyLookupMode(LookupModes.ByName);

    [RelayCommand]
    private void SelectLookupBySlug() => ApplyLookupMode(LookupModes.BySlug);

    private void ApplySource(ModSources source, bool persist = true)
    {
        CurrentSource = source;
        IsModrinthSelected = source == ModSources.Modrinth;
        IsCurseForgeSelected = source == ModSources.CurseForge;
        RefreshListHeader();
        RefreshSourceStatus();

        if (!persist || _suppressPreferenceSave)
            return;

        Properties.Settings.Default.PreferredSource = source.ToString();
        Properties.Settings.Default.Save();
    }

    private void ApplyLookupMode(LookupModes mode, bool persist = true)
    {
        CurrentLookupMode = mode;
        IsLookupById = mode == LookupModes.ById;
        IsLookupByName = mode == LookupModes.ByName;
        IsLookupBySlug = mode == LookupModes.BySlug;
        RefreshListHeader();
        RefreshSourceStatus();

        if (!persist || _suppressPreferenceSave)
            return;

        Properties.Settings.Default.PreferredLookupMode = mode.ToString();
        Properties.Settings.Default.Save();
    }

    private void RefreshListHeader()
    {
        ListHeaderText = CurrentLookupMode switch
        {
            LookupModes.ById => "Mod IDs (line separated)",
            LookupModes.ByName => "Mod names (line separated)",
            _ => "Mod slugs (line separated)"
        };
    }

    private void RefreshSourceStatus()
    {
        var lookupLabel = CurrentLookupMode switch
        {
            LookupModes.ById => "By ID",
            LookupModes.ByName => "By Name",
            _ => "By Slug"
        };

        var sourceLabel = CurrentSource == ModSources.Modrinth ? "Modrinth" : "CurseForge";
        SourceStatusText = $"Source: {sourceLabel} · Lookup: {lookupLabel}";
    }

    private bool CanFetchMods() => !IsBusy;

    [RelayCommand(CanExecute = nameof(CanFetchMods))]
    private async Task FetchMods()
    {
        try
        {
            var queries = ModIdText
                .Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();

            if (queries.Count == 0)
            {
                StatusText = "Enter at least one mod entry.";
                return;
            }

            if (CurrentSource == ModSources.CurseForge &&
                string.IsNullOrWhiteSpace(Properties.Settings.Default.ApiKey))
            {
                StatusText = "CurseForge API key is missing. Set it in Settings.";
                return;
            }

            IsBusy = true;
            ProgressPercent = 0;
            IsProgressIndeterminate = CurrentSource == ModSources.CurseForge;
            StatusText = $"Fetching mods from {(CurrentSource == ModSources.Modrinth ? "Modrinth" : "CurseForge")}...";

            IModLookupService service = CurrentSource == ModSources.Modrinth
                ? new ModrinthService()
                : new CurseForgeService();

            var progress = new Progress<LookupProgress>(p =>
            {
                if (p.Total > 0)
                {
                    IsProgressIndeterminate = false;
                    ProgressPercent = Math.Clamp(100.0 * p.Current / p.Total, 0, 100);
                }

                var label = string.IsNullOrWhiteSpace(p.CurrentQuery) ? "mods" : p.CurrentQuery;
                StatusText = p.Total > 0
                    ? $"Checking {p.Current}/{p.Total}: {label}"
                    : $"Checking: {label}";
            });

            var mods = await service.LookupAsync(
                queries,
                VersionText.Trim(),
                CurrentModLoader,
                CurrentLookupMode,
                progress);

            Results = new(mods.Select(mod => new ModQueryResult
            {
                ModName = mod.Name,
                ModLink = mod.Link,
                IsFilter = mod.IsFilterMatched,
                LatestGameVersion = mod.LatestGameVersion
            }));

            Results.Sort((x, y) =>
            {
                if (x.IsFilter && !y.IsFilter)
                    return -1;
                if (!x.IsFilter && y.IsFilter)
                    return 1;
                return string.Compare(x.ModName, y.ModName, StringComparison.OrdinalIgnoreCase);
            });

            var available = Results.Count(r => r.IsFilter);
            var message = $"Fetched {Results.Count} mods ({available} available).";
            if (message == lastStatusText)
            {
                queryCount++;
                StatusText = $"{message} ({queryCount})";
            }
            else
            {
                queryCount = 1;
                StatusText = message;
            }

            lastStatusText = message;
            ProgressPercent = 100;
        }
        catch (Exception ex)
        {
            StatusText = "Error: " + ex.Message;
        }
        finally
        {
            IsBusy = false;
            IsProgressIndeterminate = false;
            if (ProgressPercent < 100 && StatusText.StartsWith("Error", StringComparison.Ordinal))
                ProgressPercent = 0;
        }
    }

    [RelayCommand]
    private void OpenModLink()
    {
        if (SelectedResult?.ModLink == null)
            return;

        Process.Start(new ProcessStartInfo
        {
            FileName = SelectedResult.ModLink.ToString(),
            UseShellExecute = true
        });
    }

    [RelayCommand]
    private void ImportFromFolder()
    {
        var dialog = new Microsoft.Win32.OpenFolderDialog
        {
            Multiselect = false,
            Title = "Select mods folder"
        };

        if (dialog.ShowDialog() != true)
            return;

        var files = Directory.GetFiles(dialog.FolderName);
        ModIdText = string.Join("\r\n", files.Select(x => Path.GetFileNameWithoutExtension(x)));
        StatusText = $"Imported {files.Length} file name(s).";
    }

    [RelayCommand]
    private void OpenSettings()
    {
        var settings = new SettingsView
        {
            Owner = Application.Current.MainWindow
        };
        settings.ShowDialog();
    }
}
