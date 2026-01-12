using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JustTray.Models;
using JustTray.Services;
using Shortcut = JustTray.Models.Shortcut;

namespace JustTray.ViewModels;

public partial class ShortcutsRunnerViewModel : ObservableObject
{
    private readonly ShortcutService _shortcutService;
    private readonly bool _isSettingsMode;

    [ObservableProperty]
    private ObservableCollection<Shortcut> _shortcuts;

    public event Action? CloseRequested;
    public event Action<Shortcut>? EditShortcutRequested;

    public ShortcutsRunnerViewModel(ShortcutService shortcutService, bool isSettingsMode = false)
    {
        _shortcutService = shortcutService;
        _isSettingsMode = isSettingsMode;
        _shortcuts = shortcutService.Shortcuts;
        
        _shortcutService.ShortcutsChanged += () => Shortcuts = _shortcutService.Shortcuts;
    }

    [RelayCommand]
    private void ShortcutClicked(Shortcut shortcut)
    {
        if (_isSettingsMode)
        {
            EditShortcutRequested?.Invoke(shortcut);
        }
        else
        {
            _shortcutService.ExecuteShortcut(shortcut);
            CloseRequested?.Invoke();
        }
    }

    public void MoveShortcut(int oldIndex, int newIndex)
    {
        _shortcutService.MoveShortcut(oldIndex, newIndex);
    }
}
