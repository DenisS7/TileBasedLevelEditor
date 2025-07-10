using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TileBasedLevelEditor.ViewModels;

namespace TileBasedLevelEditor.Misc
{
    //from https://stackoverflow.com/questions/27865441/move-items-in-a-canvas-using-mvvm
    public static class DragBehavior
    {
        private static Point _mouseStartPosition;
        private static bool _isDragging = false;

        public static readonly DependencyProperty IsEnabledProperty =
          DependencyProperty.RegisterAttached("IsEnabled",
          typeof(bool), typeof(DragBehavior),
          new PropertyMetadata(false, OnIsEnabledChanged));

        public static bool GetIsEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(IsEnabledProperty, value);
        }

        private static void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(sender is not ScrollViewer scrollViewer)
            {
                throw new ArgumentException("Attaching element must be of type Scroll Viewer");
            }

            bool isEnabled = (bool)e.NewValue;
            if (isEnabled)
            {
                scrollViewer.MouseRightButtonDown += ElementOnMouseRightButtonDown;
                scrollViewer.MouseRightButtonUp += ElementOnMouseRightButtonUp;
                scrollViewer.MouseMove += ElementOnMouseMove;
            }
            else
            {
                scrollViewer.MouseRightButtonDown -= ElementOnMouseRightButtonDown;
                scrollViewer.MouseRightButtonUp -= ElementOnMouseRightButtonUp;
                scrollViewer.MouseMove -= ElementOnMouseMove;
            }
        }

        private static void ElementOnMouseRightButtonDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (sender is not ScrollViewer scrollViewer)
                return;
            
            scrollViewer.CaptureMouse();
            var parent = Application.Current.MainWindow;
            _mouseStartPosition = mouseButtonEventArgs.GetPosition(parent);
            _isDragging = true;
        }

        private static void ElementOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (sender is not ScrollViewer scrollViewer)
                return;
            
            scrollViewer.ReleaseMouseCapture();
            _isDragging = false;
        }

        private static void ElementOnMouseMove(object sender, MouseEventArgs mouseEventArgs)
        {
            if (sender is not ScrollViewer scrollViewer || !scrollViewer.IsMouseCaptured || !_isDragging)
                return;

            var parent = Application.Current.MainWindow;
            var mousePos = mouseEventArgs.GetPosition(parent);
            var diff = (_mouseStartPosition - mousePos);
            _mouseStartPosition = mousePos;
            
            scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + diff.X); 
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + diff.Y); 
        }
    }
}
