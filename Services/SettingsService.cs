using System.IO;
using System.Text.Json;
using JustTray.Models;

namespace JustTray.Services;

public class SettingsService
{
    private const string SettingsFileName = "settings.json";
    private readonly string _defaultFolder;
    private AppSettings _settings = null!;

    public AppSettings Settings => _settings;
    public string SettingsFolder => string.IsNullOrEmpty(_settings.SettingsFolder) 
        ? _defaultFolder 
        : _settings.SettingsFolder;

    public event Action? SettingsChanged;

    public SettingsService()
    {
        _defaultFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "JustTray");
        
        LoadSettings();
    }

    private string GetSettingsFilePath()
    {
        return Path.Combine(_defaultFolder, SettingsFileName);
    }

    public void LoadSettings()
    {
        var filePath = GetSettingsFilePath();
        
        if (!Directory.Exists(_defaultFolder))
        {
            Directory.CreateDirectory(_defaultFolder);
        }

        if (File.Exists(filePath))
        {
            var json = File.ReadAllText(filePath);
            _settings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
        else
        {
            _settings = new AppSettings();
        }

        if (string.IsNullOrEmpty(_settings.SettingsFolder))
        {
            _settings.SettingsFolder = _defaultFolder;
        }
    }

    public void SaveSettings()
    {
        var filePath = GetSettingsFilePath();
        
        if (!Directory.Exists(_defaultFolder))
        {
            Directory.CreateDirectory(_defaultFolder);
        }

        var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, json);
        
        SettingsChanged?.Invoke();
    }

    public void UpdateSettings(Action<AppSettings> update)
    {
        update(_settings);
        SaveSettings();
    }

    public static string GetWindowsAccentColor()
    {
        try
        {
            using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows\DWM");
            
            if (key?.GetValue("AccentColor") is int color)
            {
                var a = (byte)((color >> 24) & 0xFF);
                var b = (byte)((color >> 16) & 0xFF);
                var g = (byte)((color >> 8) & 0xFF);
                var r = (byte)(color & 0xFF);
                return $"#{r:X2}{g:X2}{b:X2}";
            }
        }
        catch
        {
            // Ignore registry access errors
        }
        
        return "#0078D4"; // Default Windows blue
    }
}
