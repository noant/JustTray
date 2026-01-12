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

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
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
        // Get DPI scale for horizontal cursor conversion
        var source = PresentationSource.FromVisual(this);
        if (source?.CompositionTarget == null) return;
        
        double dpiX = source.CompositionTarget.TransformToDevice.M11;
        
        // Convert cursor X from physical pixels to DIPs
        double cursorX = _cursorAtOpen.X / dpiX;
        
        var workArea = SystemParameters.WorkArea;
        
        // Horizontal: centered on cursor
        double left = cursorX - ActualWidth / 2;
        
        // Vertical: position so the window content is fully above work area bottom
        // ActualHeight now has the correct rendered height
        double top = workArea.Bottom - ActualHeight;
        
        // Ensure within work area bounds (horizontal only)
        if (left < workArea.Left)
            left = workArea.Left;
        if (left + ActualWidth > workArea.Right)
            left = workArea.Right - ActualWidth;

        Left = left;
        Top = top;
    }

    private void Window_Deactivated(object sender, EventArgs e)
    {
        SafeClose();
    }
}
