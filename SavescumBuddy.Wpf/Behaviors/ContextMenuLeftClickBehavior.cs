using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace SavescumBuddy.Wpf.Behaviors
{
    public class ContextMenuLeftClickBehavior : Behavior<Button>
    {
        private static FrameworkElement _lastClickedButton = null;

        protected override void OnAttached()
        {
            AssociatedObject.Click += OnMouseLeftButtonUp;
            AssociatedObject.MouseRightButtonUp += OnAssociatedObjectMouseRightButtonUp;
            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Click -= OnMouseLeftButtonUp;
            AssociatedObject.MouseRightButtonUp -= OnAssociatedObjectMouseRightButtonUp;
            base.OnDetaching();
        }

        private static void OnMouseLeftButtonUp(object sender, RoutedEventArgs e)
        {
            if (sender is Button fe && fe.ContextMenu != null)
            {
                if (fe.ContextMenu.DataContext == null)
                {
                    fe.ContextMenu.SetBinding(FrameworkElement.DataContextProperty, new Binding { Source = fe.DataContext });
                }

                fe.ContextMenu.Placement = PlacementMode.Bottom;
                fe.ContextMenu.PlacementTarget = fe;
                fe.ContextMenu.VerticalOffset = -16;
                fe.ContextMenu.HorizontalOffset = -16;
                fe.ContextMenu.IsOpen = true;
                fe.IsHitTestVisible = false;
                RegisterButton(fe);
            }
        }

        private void OnAssociatedObjectMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is Button fe && fe.ContextMenu != null)
            {
                if (fe.ContextMenu.DataContext == null)
                {
                    fe.ContextMenu.SetBinding(FrameworkElement.DataContextProperty, new Binding { Source = fe.DataContext });
                }

                fe.ContextMenu.Placement = PlacementMode.Bottom;
                fe.ContextMenu.PlacementTarget = fe;
                fe.ContextMenu.IsOpen = true;
                fe.IsHitTestVisible = false;
                RegisterButton(fe);
            }
        }

        private static void RegisterButton(FrameworkElement fe)
        {
            _lastClickedButton = fe;
            void handler(object s, RoutedEventArgs e)
            {
                fe.IsHitTestVisible = true;
                _lastClickedButton.ContextMenu.Closed -= handler;
            }
            _lastClickedButton.ContextMenu.Closed += handler;
        }
    }
}
