# JustTray

A lightweight Windows tray application for quick program shortcuts.
<img width="347" height="242" alt="image" src="https://github.com/user-attachments/assets/1616e06a-c4e3-46e7-aa2d-a63299195a66" />

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


<img width="429" height="454" alt="image" src="https://github.com/user-attachments/assets/dc1488c2-6c8a-4f49-b77a-34a7eae05980" />
<img width="498" height="520" alt="image" src="https://github.com/user-attachments/assets/7373fe10-5595-4377-a4cf-322487ad46d3" />
<img width="509" height="674" alt="image" src="https://github.com/user-attachments/assets/70e0805f-b942-4777-93c3-46b4acac915d" />


