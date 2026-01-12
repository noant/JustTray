using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using JustTray.Models;
using Microsoft.Extensions.Logging;
using Shortcut = JustTray.Models.Shortcut;

namespace JustTray.Services;

public class ShortcutService
{
    private const string ShortcutsFileName = "shortcuts.json";
    private readonly SettingsService _settingsService;
    private readonly ILogger<ShortcutService> _logger;
    private ObservableCollection<Shortcut> _shortcuts = new();

    public ObservableCollection<Shortcut> Shortcuts => _shortcuts;

    public event Action? ShortcutsChanged;

    public ShortcutService(SettingsService settingsService, ILogger<ShortcutService> logger)
    {
        _settingsService = settingsService;
        _logger = logger;
        LoadShortcuts();
    }

    private string GetShortcutsFilePath()
    {
        return Path.Combine(_settingsService.SettingsFolder, ShortcutsFileName);
    }

    public void LoadShortcuts()
    {
        var filePath = GetShortcutsFilePath();
        
        if (File.Exists(filePath))
        {
            var json = File.ReadAllText(filePath);
            var shortcuts = JsonSerializer.Deserialize<List<Shortcut>>(json) ?? new List<Shortcut>();
            _shortcuts = new ObservableCollection<Shortcut>(shortcuts.OrderBy(s => s.Order));
        }
        else
        {
            _shortcuts = new ObservableCollection<Shortcut>();
        }
        
        ShortcutsChanged?.Invoke();
    }

    public void SaveShortcuts()
    {
        var folder = _settingsService.SettingsFolder;
        
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        var filePath = GetShortcutsFilePath();
        var json = JsonSerializer.Serialize(_shortcuts.ToList(), new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, json);
        
        ShortcutsChanged?.Invoke();
    }

    public void AddShortcut(Shortcut shortcut)
    {
        shortcut.Order = _shortcuts.Count;
        _shortcuts.Add(shortcut);
        SaveShortcuts();
    }

    public void UpdateShortcut(Shortcut shortcut)
    {
        var existing = _shortcuts.FirstOrDefault(s => s.Id == shortcut.Id);
        if (existing != null)
        {
            var index = _shortcuts.IndexOf(existing);
            _shortcuts[index] = shortcut;
            SaveShortcuts();
        }
    }

    public void RemoveShortcut(Models.Shortcut shortcut)
    {
        _shortcuts.Remove(shortcut);
        ReorderShortcuts();
        SaveShortcuts();
    }

    public void MoveShortcut(int oldIndex, int newIndex)
    {
        if (oldIndex < 0 || oldIndex >= _shortcuts.Count || 
            newIndex < 0 || newIndex >= _shortcuts.Count)
            return;

        _shortcuts.Move(oldIndex, newIndex);
        ReorderShortcuts();
        SaveShortcuts();
    }

    private void ReorderShortcuts()
    {
        for (int i = 0; i < _shortcuts.Count; i++)
        {
            _shortcuts[i].Order = i;
        }
    }

    public void ExecuteShortcut(Shortcut shortcut)
    {
        if (!shortcut.IsValid) return;

        // Expand environment variables
        var executablePath = Environment.ExpandEnvironmentVariables(shortcut.ExecutablePath);
        var arguments = Environment.ExpandEnvironmentVariables(shortcut.Arguments);
        var workingDirectory = string.IsNullOrWhiteSpace(shortcut.WorkingDirectory) 
            ? string.Empty 
            : Environment.ExpandEnvironmentVariables(shortcut.WorkingDirectory);

        _logger.LogInformation("Executing shortcut: {ShortcutName} ({ExecutablePath} {Arguments})", 
            shortcut.Name, executablePath, arguments);

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = executablePath,
                Arguments = arguments,
                UseShellExecute = true, // Use shell execute to avoid console handle issues
                CreateNoWindow = false
            };

            if (!string.IsNullOrWhiteSpace(workingDirectory) && Directory.Exists(workingDirectory))
            {
                startInfo.WorkingDirectory = workingDirectory;
            }

            if (shortcut.RunAsAdmin)
            {
                startInfo.Verb = "runas";
            }

            // Set window style
            startInfo.WindowStyle = shortcut.WindowStyle switch
            {
                "Hidden" => ProcessWindowStyle.Hidden,
                "Minimized" => ProcessWindowStyle.Minimized,
                "Maximized" => ProcessWindowStyle.Maximized,
                _ => ProcessWindowStyle.Normal
            };

            var process = Process.Start(startInfo);
            
            if (process != null)
            {
                _logger.LogInformation("Shortcut '{ShortcutName}' started (PID: {ProcessId})", 
                    shortcut.Name, process.Id);
                
                // Monitor process completion in background
                Task.Run(() =>
                {
                    try
                    {
                        process.WaitForExit();
                        _logger.LogInformation("Shortcut '{ShortcutName}' exited with code: {ExitCode}", 
                            shortcut.Name, process.ExitCode);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error monitoring process for '{ShortcutName}'", shortcut.Name);
                    }
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute shortcut '{ShortcutName}'", shortcut.Name);
            System.Windows.MessageBox.Show(
                $"Failed to execute shortcut: {ex.Message}",
                "Error",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
        }
    }
}
