using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using JustTray.Models;
using JustTray.ViewModels;
using DataObject = System.Windows.DataObject;
using DragDropEffects = System.Windows.DragDropEffects;
using DragEventArgs = System.Windows.DragEventArgs;
using ListBox = System.Windows.Controls.ListBox;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Point = System.Windows.Point;
using Shortcut = JustTray.Models.Shortcut;

namespace JustTray.Views.Controls;

public partial class ShortcutsRunnerControl : System.Windows.Controls.UserControl
{
    public static readonly DependencyProperty ShowAddButtonProperty =
        DependencyProperty.Register(
            nameof(ShowAddButton),
            typeof(bool),
            typeof(ShortcutsRunnerControl),
            new PropertyMetadata(false));

    public static readonly DependencyProperty AddCommandProperty =
        DependencyProperty.Register(
            nameof(AddCommand),
            typeof(ICommand),
            typeof(ShortcutsRunnerControl),
            new PropertyMetadata(null));

    public bool ShowAddButton
    {
        get => (bool)GetValue(ShowAddButtonProperty);
        set => SetValue(ShowAddButtonProperty, value);
    }

    public ICommand AddCommand
    {
        get => (ICommand)GetValue(AddCommandProperty);
        set => SetValue(AddCommandProperty, value);
    }

    private Point _dragStartPoint;
    private bool _isDragging;

    public ShortcutsRunnerControl()
    {
        InitializeComponent();
    }

    private void ShortcutsList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (!ShowAddButton) return;
        _dragStartPoint = e.GetPosition(null);
        _isDragging = false;
    }

    private void ShortcutsList_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (!ShowAddButton || e.LeftButton != MouseButtonState.Pressed) return;

        var diff = _dragStartPoint - e.GetPosition(null);

        if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
            Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
        {
            var listBox = sender as ListBox;
            var listBoxItem = FindAncestor<ListBoxItem>((DependencyObject)e.OriginalSource);

            if (listBoxItem != null && listBox != null)
            {
                var shortcut = listBoxItem.DataContext as Shortcut;
                if (shortcut != null && !_isDragging)
                {
                    _isDragging = true;
                    var data = new DataObject(typeof(Shortcut), shortcut);
                    DragDrop.DoDragDrop(listBoxItem, data, DragDropEffects.Move);
                    _isDragging = false;
                }
            }
        }
    }

    private void ShortcutsList_DragOver(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(typeof(Shortcut)))
        {
            e.Effects = DragDropEffects.None;
            e.Handled = true;
        }
    }

    private void ShortcutsList_Drop(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(typeof(Shortcut))) return;

        var droppedShortcut = e.Data.GetData(typeof(Shortcut)) as Shortcut;
        var listBox = sender as ListBox;
        
        if (droppedShortcut == null || listBox == null) return;

        var targetItem = FindAncestor<ListBoxItem>((DependencyObject)e.OriginalSource);
        if (targetItem == null) return;

        var targetShortcut = targetItem.DataContext as Shortcut;
        if (targetShortcut == null || droppedShortcut == targetShortcut) return;

        var viewModel = DataContext as ShortcutsRunnerViewModel;
        if (viewModel == null) return;

        var oldIndex = viewModel.Shortcuts.IndexOf(droppedShortcut);
        var newIndex = viewModel.Shortcuts.IndexOf(targetShortcut);

        if (oldIndex >= 0 && newIndex >= 0 && oldIndex != newIndex)
        {
            viewModel.MoveShortcut(oldIndex, newIndex);
        }
    }

    private static T? FindAncestor<T>(DependencyObject current) where T : DependencyObject
    {
        while (current != null)
        {
            if (current is T ancestor)
                return ancestor;
            current = VisualTreeHelper.GetParent(current);
        }
        return null;
    }
}
