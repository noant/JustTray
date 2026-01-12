using System.Windows;
using CommunityToolkit.Mvvm.Input;
using JustTray.Models;
using JustTray.Services;
using JustTray.ViewModels;
using Application = System.Windows.Application;
using Shortcut = JustTray.Models.Shortcut;

namespace JustTray.Views;

public partial class SettingsWindow : Window
{
    private readonly ShortcutService _shortcutService;
    private readonly SettingsService _settingsService;
    private readonly SettingsViewModel _settingsViewModel;
    private readonly ShortcutsRunnerViewModel _shortcutsViewModel;

    public IRelayCommand AddShortcutCommand { get; }

    public SettingsWindow(ShortcutService shortcutService, SettingsService settingsService)
    {
        InitializeComponent();
        
        _shortcutService = shortcutService;
        _settingsService = settingsService;
        
        _settingsViewModel = new SettingsViewModel(settingsService, shortcutService);
        _shortcutsViewModel = new ShortcutsRunnerViewModel(shortcutService, isSettingsMode: true);
        _shortcutsViewModel.EditShortcutRequested += OpenShortcutEditor;
        
        AddShortcutCommand = new RelayCommand(AddNewShortcut);
        
        DataContext = _settingsViewModel;
        ShortcutsControl.DataContext = _shortcutsViewModel;
        
        // Apply dark title bar based on theme
        SourceInitialized += (_, _) => 
            App.ApplyWindowDarkMode(this, ((App)Application.Current).IsDarkTheme);
    }

    private void AddNewShortcut()
    {
        OpenShortcutEditor(null);
    }

    private void OpenShortcutEditor(Shortcut? shortcut)
    {
        var editorWindow = new ShortcutEditorWindow(_shortcutService, shortcut)
        {
            Owner = this
        };
        editorWindow.ShowDialog();
    }
}
