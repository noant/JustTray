namespace JustTray.Models;

public class AppSettings
{
    public string Theme { get; set; } = "Dark";
    public string SettingsFolder { get; set; } = string.Empty;
    public bool RunAtStartup { get; set; } = false;
}
