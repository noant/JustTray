using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JustTray.Models;
using JustTray.Services;
using Microsoft.Win32;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using Shortcut = JustTray.Models.Shortcut;

namespace JustTray.ViewModels;

public partial class ShortcutEditorViewModel : ObservableObject
{
    private readonly ShortcutService _shortcutService;
    private readonly Shortcut? _originalShortcut;
    private readonly bool _isNew;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _executablePath = string.Empty;

    [ObservableProperty]
    private string _arguments = string.Empty;

    [ObservableProperty]
    private string _workingDirectory = string.Empty;

    [ObservableProperty]
    private bool _runAsAdmin = false;

    [ObservableProperty]
    private string _windowStyle = "Normal";

    [ObservableProperty]
    private string _iconColor = string.Empty;

    [ObservableProperty]
    private bool _useWindowsAccentColor = true;

    public string[] PredefinedColors => new[]
    {
        "#FF0000", "#FF5722", "#FF9800", "#FFC107",
        "#4CAF50", "#009688", "#00BCD4", "#03A9F4",
        "#2196F3", "#3F51B5", "#673AB7", "#9C27B0",
        "#E91E63", "#795548", "#607D8B", "#9E9E9E"
    };

    public string[] WindowStyles => new[] { "Normal", "Hidden", "Minimized", "Maximized" };

    public event Action<bool>? CloseRequested;

    public ShortcutEditorViewModel(ShortcutService shortcutService, Shortcut? shortcut = null)
    {
        _shortcutService = shortcutService;
        _originalShortcut = shortcut;
        _isNew = shortcut == null;

        if (shortcut != null)
        {
            Name = shortcut.Name;
            ExecutablePath = shortcut.ExecutablePath;
            Arguments = shortcut.Arguments;
            WorkingDirectory = shortcut.WorkingDirectory;
            RunAsAdmin = shortcut.RunAsAdmin;
            WindowStyle = shortcut.WindowStyle;
            IconColor = shortcut.IconColor;
            UseWindowsAccentColor = string.IsNullOrEmpty(shortcut.IconColor);
        }
    }

    partial void OnUseWindowsAccentColorChanged(bool value)
    {
        if (value)
        {
            IconColor = string.Empty;
        }
    }

    [RelayCommand]
    private void BrowseExecutable()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Select Executable",
            Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*",
            CheckFileExists = true
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            ExecutablePath = dialog.FileName;
            
            if (string.IsNullOrEmpty(Name))
            {
                Name = System.IO.Path.GetFileNameWithoutExtension(dialog.FileName);
            }
        }
    }

    [RelayCommand]
    private void BrowseWorkingDirectory()
    {
        var dialog = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = "Select Working Directory",
            UseDescriptionForTitle = true
        };

        if (!string.IsNullOrEmpty(WorkingDirectory) && System.IO.Directory.Exists(WorkingDirectory))
        {
            dialog.InitialDirectory = WorkingDirectory;
        }

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            WorkingDirectory = dialog.SelectedPath;
        }
    }

    [RelayCommand]
    private void SelectColor(string color)
    {
        IconColor = color;
        UseWindowsAccentColor = false;
    }

    [RelayCommand]
    private void Save()
    {
        if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(ExecutablePath))
        {
            System.Windows.MessageBox.Show(
                "Please fill in the name and executable path.",
                "Validation Error",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Warning);
            return;
        }

        var shortcut = _originalShortcut?.Clone() ?? new Shortcut();
        shortcut.Name = Name;
        shortcut.ExecutablePath = ExecutablePath;
        shortcut.Arguments = Arguments;
        shortcut.WorkingDirectory = WorkingDirectory;
        shortcut.RunAsAdmin = RunAsAdmin;
        shortcut.WindowStyle = WindowStyle;
        shortcut.IconColor = UseWindowsAccentColor ? string.Empty : IconColor;

        if (_isNew)
        {
            _shortcutService.AddShortcut(shortcut);
        }
        else
        {
            _shortcutService.UpdateShortcut(shortcut);
        }

        CloseRequested?.Invoke(true);
    }

    [RelayCommand]
    private void Cancel()
    {
        CloseRequested?.Invoke(false);
    }

    [RelayCommand]
    private void Delete()
    {
        if (_originalShortcut == null) return;

        var result = System.Windows.MessageBox.Show(
            $"Are you sure you want to delete '{Name}'?",
            "Confirm Delete",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Question);

        if (result == System.Windows.MessageBoxResult.Yes)
        {
            _shortcutService.RemoveShortcut(_originalShortcut);
            CloseRequested?.Invoke(true);
        }
    }

    public bool CanDelete => !_isNew;
}
