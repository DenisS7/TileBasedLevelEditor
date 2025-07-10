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
    public class DragBehavior
    {
        public readonly TranslateTransform Transform = new TranslateTransform();
        private Point _elementStartPosition2;
        private Point _mouseStartPosition2;
        private bool _isDragging = false;
        private static DragBehavior _instance = new DragBehavior();
        public static DragBehavior Instance
        {
            get { return _instance; }
            set { _instance = value; }
        }

        public static bool GetDrag(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsDragProperty);
        }

        public static void SetDrag(DependencyObject obj, bool value)
        {
            obj.SetValue(IsDragProperty, value);
        }

        public static readonly DependencyProperty IsDragProperty =
          DependencyProperty.RegisterAttached("Drag",
          typeof(bool), typeof(DragBehavior),
          new PropertyMetadata(false, OnDragChanged));

        // Optional
        public static readonly DependencyProperty DragScrollViewerProperty = DependencyProperty.RegisterAttached(
          "DragScrollViewer", typeof(ScrollViewer), typeof(DragBehavior), new PropertyMetadata(default(ScrollViewer)));

        public static void SetDragScrollViewer(DependencyObject attachingElement, ScrollViewer value) => attachingElement.SetValue(DragScrollViewerProperty, value);

        public static ScrollViewer GetDragScrollViewer(DependencyObject attachingElement) => (ScrollViewer)attachingElement.GetValue(DragScrollViewerProperty);

        private static void OnDragChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var element = (UIElement)sender;
            var isDrag = (bool)(e.NewValue);


            Instance = new DragBehavior();
            element.RenderTransform = Instance.Transform;

            if (isDrag)
            {
                element.MouseRightButtonDown += Instance.ElementOnMouseRightButtonDown;
                element.MouseRightButtonUp += Instance.ElementOnMouseRightButtonUp;
                element.MouseMove += Instance.ElementOnMouseMove;
            }
            else
            {
                element.MouseRightButtonDown -= Instance.ElementOnMouseRightButtonDown;
                element.MouseRightButtonUp -= Instance.ElementOnMouseRightButtonUp;
                element.MouseMove -= Instance.ElementOnMouseMove;
            }
        }

        private void ElementOnMouseRightButtonDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            var parent = Application.Current.MainWindow;
            _mouseStartPosition2 = mouseButtonEventArgs.GetPosition(parent);
            ((UIElement)sender).CaptureMouse();
            _isDragging = true;
        }

        private void ElementOnMouseRightButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ((UIElement)sender).ReleaseMouseCapture();
            _elementStartPosition2.X = Transform.X;
            _elementStartPosition2.Y = Transform.Y;
            _isDragging = false;
        }

        private void ElementOnMouseMove(object sender, MouseEventArgs mouseEventArgs)
        {
            if (!((UIElement)sender).IsMouseCaptured || !_isDragging) return;

            var parent = Application.Current.MainWindow;
            var mousePos = mouseEventArgs.GetPosition(parent);
            var diff = (mousePos - _mouseStartPosition2);
            Transform.X = _elementStartPosition2.X + diff.X;
            Transform.Y = _elementStartPosition2.Y + diff.Y;

            if (sender is FrameworkElement f && GetDragScrollViewer(f) is ScrollViewer scrollViewer && f.DataContext is TileGridViewModel vm)
            {
                ;
                //Transform.X = Math.Clamp(Transform.X, -vm.DragLimits.X, vm.DragLimits.X);
                //Transform.Y = Math.Clamp(Transform.Y, -vm.DragLimits.Y, vm.DragLimits.Y);
                //Debug.WriteLine("Transform: {0}, {1}", Transform.X, Transform.Y);
                //Debug.WriteLine("Scroll Viewer: {0}, {1}, {2}", vm.ScrollViewerWidth, vm.ScrollViewerHeight, vm.ScrollViewerZoom);
                //Debug.WriteLine("Scaled Size: {0}, {1}", vm.ScaledViewportWidth, vm.ScaledViewportHeight);
               // Debug.WriteLine("Limits: {0}, {1}", scrollViewer.ViewportWidth * (1.0 - vm.ScrollViewerZoom), scrollViewer.ViewportHeight * (1.0 - vm.ScrollViewerZoom));
            }
        }
    }
}
