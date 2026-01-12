using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JustTray.Models;
using JustTray.Services;
using Microsoft.Win32;
using Application = System.Windows.Application;

namespace JustTray.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly SettingsService _settingsService;
    private readonly ShortcutService _shortcutService;

    [ObservableProperty]
    private string _theme;

    [ObservableProperty]
    private string _settingsFolder;

    [ObservableProperty]
    private bool _runAtStartup;

    public string[] AvailableThemes => new[] { "Dark", "Light" };

    public SettingsViewModel(SettingsService settingsService, ShortcutService shortcutService)
    {
        _settingsService = settingsService;
        _shortcutService = shortcutService;

        _theme = settingsService.Settings.Theme;
        _settingsFolder = settingsService.SettingsFolder;
        _runAtStartup = AutostartService.IsEnabled();
    }

    partial void OnThemeChanged(string value)
    {
        _settingsService.UpdateSettings(s => s.Theme = value);
        ((App)Application.Current).ApplyTheme(value);
    }

    partial void OnRunAtStartupChanged(bool value)
    {
        AutostartService.SetEnabled(value);
        _settingsService.UpdateSettings(s => s.RunAtStartup = value);
    }

    [RelayCommand]
    private void BrowseFolder()
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Select Settings Folder",
            InitialDirectory = SettingsFolder
        };

        if (dialog.ShowDialog() == true)
        {
            var oldFolder = SettingsFolder;
            SettingsFolder = dialog.FolderName;
            _settingsService.UpdateSettings(s => s.SettingsFolder = dialog.FolderName);
            
            // Reload shortcuts from new location
            _shortcutService.LoadShortcuts();
        }
    }

    [RelayCommand]
    private void ExitApplication()
    {
        Application.Current.Shutdown();
    }
}
