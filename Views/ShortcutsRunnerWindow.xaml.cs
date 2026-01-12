using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using JustTray.Services;
using JustTray.ViewModels;
using Point = System.Windows.Point;

namespace JustTray.Views;

public partial class ShortcutsRunnerWindow : Window
{
    private readonly ShortcutsRunnerViewModel _viewModel;
    private bool _isClosing;

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetCursorPos(out POINT lpPoint);

    [DllImport("user32.dll")]
    private static extern bool SystemParametersInfo(int uiAction, int uiParam, ref RECT pvParam, int fWinIni);

    private const int SPI_GETWORKAREA = 0x0030;

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    public ShortcutsRunnerWindow(ShortcutService shortcutService, SettingsService settingsService)
    {
        InitializeComponent();
        
        _viewModel = new ShortcutsRunnerViewModel(shortcutService, isSettingsMode: false);
        _viewModel.CloseRequested += () => SafeClose();
        
        DataContext = _viewModel;
        
        // Get cursor position immediately in constructor (before window moves)
        GetCursorPos(out _cursorAtOpen);
        
        // Position off-screen initially, then move after render
        Left = -10000;
        Top = -10000;
        
        ContentRendered += OnContentRendered;
    }
    
    private POINT _cursorAtOpen;

    private void SafeClose()
    {
        if (_isClosing) return;
        _isClosing = true;
        Close();
    }

    private void OnContentRendered(object? sender, EventArgs e)
    {
        // Get DPI scale
        var source = PresentationSource.FromVisual(this);
        if (source?.CompositionTarget == null) return;
        
        double dpiX = source.CompositionTarget.TransformToDevice.M11;
        double dpiY = source.CompositionTarget.TransformToDevice.M22;
        
        // Get work area in physical pixels using Win32 API
        RECT workAreaPhysical = default;
        SystemParametersInfo(SPI_GETWORKAREA, 0, ref workAreaPhysical, 0);
        
        // Convert physical pixels to DIPs
        double workAreaBottom = workAreaPhysical.Bottom / dpiY;
        double workAreaLeft = workAreaPhysical.Left / dpiX;
        double workAreaRight = workAreaPhysical.Right / dpiX;
        
        // Convert cursor X from physical pixels to DIPs
        double cursorX = _cursorAtOpen.X / dpiX;
        
        // Horizontal: centered on cursor
        double left = cursorX - ActualWidth / 2;
        
        // Vertical: position so the window is fully above work area bottom
        double top = workAreaBottom - ActualHeight - 40;
        
        // Ensure within work area bounds (horizontal only)
        if (left < workAreaLeft)
            left = workAreaLeft;
        if (left + ActualWidth > workAreaRight)
            left = workAreaRight - ActualWidth;

        Left = left;
        Top = top;
    }

    private void Window_Deactivated(object sender, EventArgs e)
    {
        SafeClose();
    }
}
