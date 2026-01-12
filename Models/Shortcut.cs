using System.Text.Json.Serialization;

namespace JustTray.Models;

public class Shortcut
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string ExecutablePath { get; set; } = string.Empty;
    public string Arguments { get; set; } = string.Empty;
    public string WorkingDirectory { get; set; } = string.Empty;
    public bool RunAsAdmin { get; set; } = false;
    public string WindowStyle { get; set; } = "Normal"; // Normal, Hidden, Minimized, Maximized
    public string IconColor { get; set; } = string.Empty; // Empty means use Windows accent color
    public int Order { get; set; }

    [JsonIgnore]
    public bool IsValid => !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(ExecutablePath);

    public Shortcut Clone()
    {
        return new Shortcut
        {
            Id = Id,
            Name = Name,
            ExecutablePath = ExecutablePath,
            Arguments = Arguments,
            WorkingDirectory = WorkingDirectory,
            RunAsAdmin = RunAsAdmin,
            WindowStyle = WindowStyle,
            IconColor = IconColor,
            Order = Order
        };
    }
}
