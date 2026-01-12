using System.Windows;
using System.Windows.Input;
using JustTray.Models;
using JustTray.Services;
using JustTray.ViewModels;

namespace JustTray.Views;

public partial class ShortcutEditorWindow : Window
{
    private readonly ShortcutEditorViewModel _viewModel;

    public ShortcutEditorWindow(ShortcutService shortcutService, Models.Shortcut? shortcut = null)
    {
        InitializeComponent();
        
        _viewModel = new ShortcutEditorViewModel(shortcutService, shortcut);
        _viewModel.CloseRequested += (saved) => 
        {
            DialogResult = saved;
            Close();
        };
        
        DataContext = _viewModel;
        
        Title = shortcut == null ? "Add Shortcut" : "Edit Shortcut";
    }

    private void ColorItem_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element && element.Tag is string color)
        {
            _viewModel.IconColor = color;
            _viewModel.UseWindowsAccentColor = false;
        }
    }
}
