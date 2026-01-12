using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace JustTray.Views.Controls;

public partial class ShortcutItemControl : System.Windows.Controls.UserControl
{
    public static readonly DependencyProperty ClickCommandProperty =
        DependencyProperty.Register(
            nameof(ClickCommand),
            typeof(ICommand),
            typeof(ShortcutItemControl),
            new PropertyMetadata(null));

    public ICommand ClickCommand
    {
        get => (ICommand)GetValue(ClickCommandProperty);
        set => SetValue(ClickCommandProperty, value);
    }

    public ShortcutItemControl()
    {
        InitializeComponent();
    }
}
