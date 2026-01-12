using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;
using JustTray.Services;
using JustTray.Views;
using Microsoft.Extensions.Logging;
using Application = System.Windows.Application;

namespace JustTray;

public partial class App : Application
{
    private const string MutexName = "JustTray_SingleInstance_Mutex";
    private const string PipeName = "JustTray_Pipe";
    
    private static Mutex? _mutex;
    private TaskbarIcon? _trayIcon;
    private SettingsService _settingsService = null!;
    private ILoggerFactory _loggerFactory = null!;
    private ILogger<App> _logger = null!;
    private ShortcutService _shortcutService = null!;
    private CancellationTokenSource? _pipeServerCts;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Check for existing instance
        _mutex = new Mutex(true, MutexName, out bool createdNew);
        
        if (!createdNew)
        {
            // Another instance is running - signal it to show Runner window
            SignalExistingInstance();
            Shutdown();
            return;
        }

        _settingsService = new SettingsService();
        
        // Setup logging with file rotation (5 MB per file)
        var logPath = Path.Combine(_settingsService.SettingsFolder, "justtray.log");
        _loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddFile(logPath, minimumLevel: LogLevel.Information, fileSizeLimitBytes: 5_242_880, retainedFileCountLimit: 5);
        });
        
        _logger = _loggerFactory.CreateLogger<App>();
        var shortcutLogger = _loggerFactory.CreateLogger<ShortcutService>();
        _shortcutService = new ShortcutService(_settingsService, shortcutLogger);
        
        _logger.LogInformation("JustTray started");

        ApplyTheme(_settingsService.Settings.Theme);

        _trayIcon = new TaskbarIcon
        {
            Icon = LoadTrayIcon(),
            ToolTipText = "JustTray - Click to open shortcuts"
        };

        _trayIcon.TrayLeftMouseUp += TrayIcon_LeftClick;
        _trayIcon.TrayRightMouseUp += TrayIcon_RightClick;
        
        // Start listening for signals from other instances
        StartPipeServer();
    }

    private static void SignalExistingInstance()
    {
        try
        {
            using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
            client.Connect(1000);
            using var writer = new StreamWriter(client);
            writer.WriteLine("SHOW");
            writer.Flush();
        }
        catch
        {
            // Ignore connection errors
        }
    }

    private void StartPipeServer()
    {
        _pipeServerCts = new CancellationTokenSource();
        Task.Run(() => PipeServerLoop(_pipeServerCts.Token));
    }

    private async Task PipeServerLoop(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                using var server = new NamedPipeServerStream(PipeName, PipeDirection.In);
                await server.WaitForConnectionAsync(ct);
                
                using var reader = new StreamReader(server);
                var message = await reader.ReadLineAsync(ct);
                
                if (message == "SHOW")
                {
                    Dispatcher.Invoke(ShowRunnerWindow);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch
            {
                // Ignore pipe errors, continue listening
            }
        }
    }

    private void ShowRunnerWindow()
    {
        var runnerWindow = new ShortcutsRunnerWindow(_shortcutService, _settingsService);
        runnerWindow.Show();
        runnerWindow.Activate();
    }

    private static Icon LoadTrayIcon()
    {
        var resourceStream = GetResourceStream(new Uri("pack://application:,,,/app.ico"));
        if (resourceStream != null)
        {
            return new Icon(resourceStream.Stream);
        }
        
        // Fallback: create a simple circle icon
        var bitmap = new Bitmap(32, 32);
        using (var g = Graphics.FromImage(bitmap))
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.Clear(Color.Transparent);
            using var brush = new SolidBrush(Color.FromArgb(0, 120, 212));
            g.FillEllipse(brush, 2, 2, 28, 28);
        }
        return Icon.FromHandle(bitmap.GetHicon());
    }

    private void TrayIcon_LeftClick(object sender, RoutedEventArgs e)
    {
        ShowRunnerWindow();
    }

    private void TrayIcon_RightClick(object sender, RoutedEventArgs e)
    {
        var existingWindow = Current.Windows.OfType<SettingsWindow>().FirstOrDefault();
        if (existingWindow != null)
        {
            existingWindow.Activate();
            return;
        }

        var settingsWindow = new SettingsWindow(_shortcutService, _settingsService);
        settingsWindow.Show();
    }

    public void ApplyTheme(string theme)
    {
        var colors = Resources.MergedDictionaries
            .FirstOrDefault(d => d.Source?.OriginalString.Contains("Colors.xaml") == true);
        
        if (colors != null)
        {
            var isDark = theme == "Dark";
            colors["BackgroundColor"] = isDark 
                ? System.Windows.Media.Color.FromRgb(32, 32, 32) 
                : System.Windows.Media.Color.FromRgb(249, 249, 249);
            colors["SurfaceColor"] = isDark 
                ? System.Windows.Media.Color.FromRgb(44, 44, 44) 
                : System.Windows.Media.Color.FromRgb(255, 255, 255);
            colors["TextColor"] = isDark 
                ? System.Windows.Media.Color.FromRgb(255, 255, 255) 
                : System.Windows.Media.Color.FromRgb(0, 0, 0);
            colors["SecondaryTextColor"] = isDark 
                ? System.Windows.Media.Color.FromRgb(170, 170, 170) 
                : System.Windows.Media.Color.FromRgb(96, 96, 96);
            colors["BorderColor"] = isDark 
                ? System.Windows.Media.Color.FromRgb(60, 60, 60) 
                : System.Windows.Media.Color.FromRgb(229, 229, 229);
            colors["HoverColor"] = isDark 
                ? System.Windows.Media.Color.FromRgb(55, 55, 55) 
                : System.Windows.Media.Color.FromRgb(243, 243, 243);
        }
    }


    protected override void OnExit(ExitEventArgs e)
    {
        _pipeServerCts?.Cancel();
        _trayIcon?.Dispose();
        _loggerFactory?.Dispose();
        _mutex?.ReleaseMutex();
        _mutex?.Dispose();
        base.OnExit(e);
    }
}
