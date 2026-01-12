# JustTray

A lightweight Windows tray application for quick program shortcuts.

## Features

- Tray icon with customizable shortcuts
- Left-click on tray icon to open shortcuts menu
- Right-click for settings
- Dark/Light theme support
- Windows startup integration
- Single instance (re-launching opens shortcuts menu)

## Usage

- **Left-click** tray icon - open shortcuts menu
- **Right-click** tray icon - open settings
- **Click outside** shortcuts menu - close it

### Adding Shortcuts

1. Right-click tray icon -> Settings
2. Click "+ Add Shortcut"
3. Fill in name, executable path, arguments (optional)
4. Set working directory (optional)
5. Choose icon color or use Windows accent color
6. Enable "Run as Administrator" if needed
7. Select window style (Normal, Minimized, Maximized, Hidden)
8. Save

### Managing Shortcuts

- **Reorder**: Drag and drop shortcuts in the settings window
- **Edit**: Click on a shortcut in settings to modify it
- **Delete**: Open shortcut editor and click "Delete"

## Build

Requirements: .NET 8 SDK

```bash
# Development
dotnet build

# Release (single file)
python build.py <output_path>
```

## Configuration

Settings stored in `%APPDATA%\JustTray\`:
- `settings.json` - app settings
- `shortcuts.json` - shortcuts list
