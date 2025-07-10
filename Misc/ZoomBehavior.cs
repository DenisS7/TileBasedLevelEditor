using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using TileBasedLevelEditor.ViewModels;
using System.Diagnostics;

namespace TileBasedLevelEditor.Misc
{
    //modified from https://stackoverflow.com/questions/62714559/wpf-canvas-zoom-and-children-position/62715838#62715838
    public class ZoomBehavior
    {
        // Required
        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
          "IsEnabled", typeof(bool), typeof(ZoomBehavior), new PropertyMetadata(default(bool), OnIsEnabledChanged));

        public static void SetIsEnabled(DependencyObject attachingElement, bool value) => attachingElement.SetValue(IsEnabledProperty, value);

        public static bool GetIsEnabled(DependencyObject attachingElement) => (bool)attachingElement.GetValue(IsEnabledProperty);

        // Optional
        public static readonly DependencyProperty ZoomFactorProperty = DependencyProperty.RegisterAttached(
          "ZoomFactor", typeof(double), typeof(ZoomBehavior), new PropertyMetadata(0.1));

        public static void SetZoomFactor(DependencyObject attachingElement, double value) => attachingElement.SetValue(ZoomFactorProperty, value);

        public static double GetZoomFactor(DependencyObject attachingElement) => (double)attachingElement.GetValue(ZoomFactorProperty);

        // Optional
        public static readonly DependencyProperty ChildElementProperty = DependencyProperty.RegisterAttached(
          "ChildElement", typeof(FrameworkElement), typeof(ZoomBehavior), new PropertyMetadata(null, OnChildElementChanged));

        public static void SetChildElement(DependencyObject attachingElement, FrameworkElement value)
        {
            attachingElement.SetValue(ChildElementProperty, value);
        }

        public static FrameworkElement GetChildElement(DependencyObject attachingElement) => (FrameworkElement)attachingElement.GetValue(ChildElementProperty);
        private static void OnIsEnabledChanged(DependencyObject attachingElement, DependencyPropertyChangedEventArgs e)
        {
            if (attachingElement is not ScrollViewer scrollViewer)
            {
                throw new ArgumentException("Attaching element must be of type Scroll Viewer");
            }

            bool isEnabled = (bool)e.NewValue;
            FrameworkElement childElement = GetChildElement(scrollViewer);
            if (childElement == null || TryGetScaleTransform(childElement, out _))
                return;
            
            if (isEnabled)
            {
                scrollViewer.PreviewMouseWheel += Zoom_OnMouseWheel;

                if (childElement.LayoutTransform is TransformGroup transformGroup)
                {
                    transformGroup.Children.Add(new ScaleTransform());
                }
                else
                {
                    childElement.LayoutTransform = new ScaleTransform();
                }
            }
            else
            {
                scrollViewer.PreviewMouseWheel -= Zoom_OnMouseWheel;
            }
        }

        private static void OnChildElementChanged(DependencyObject attachingObject, DependencyPropertyChangedEventArgs e)
        {
            if (attachingObject is not ScrollViewer scrollViewer)
            {
                throw new ArgumentException("Attaching element must be of type Scroll Viewer");
            }

            if (GetIsEnabled(scrollViewer) && e.NewValue is FrameworkElement f)
            {
                scrollViewer.PreviewMouseWheel += Zoom_OnMouseWheel;

                if (f.LayoutTransform is TransformGroup transformGroup)
                {
                    transformGroup.Children.Add(new ScaleTransform());
                }
                else
                {
                    f.LayoutTransform = new ScaleTransform();
                }
            }
            else
            {
                scrollViewer.PreviewMouseWheel -= Zoom_OnMouseWheel;
            }
        }

        private static void Zoom_OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is not ScrollViewer scrollViewer)
                return;

            FrameworkElement zoomTargetElement = GetChildElement(scrollViewer);
            Point mouseCanvasPosition = e.GetPosition(zoomTargetElement);
            double scaleFactor = e.Delta > 0
              ? GetZoomFactor(zoomTargetElement)
              : -1 * GetZoomFactor(zoomTargetElement);

            ApplyZoomToAttachedElement(mouseCanvasPosition, scaleFactor, zoomTargetElement);

            AdjustScrollViewer(scrollViewer, mouseCanvasPosition, scaleFactor, zoomTargetElement);
        }

        private static void ApplyZoomToAttachedElement(Point mouseCanvasPosition, double scaleFactor, FrameworkElement zoomTargetElement)
        {
            if (!TryGetScaleTransform(zoomTargetElement, out ScaleTransform? scaleTransform))
            {
                throw new InvalidOperationException("No ScaleTransform found");
            }

            if (scaleTransform == null)
            {
                throw new InvalidOperationException("No ScaleTransform found");
            }

            scaleTransform.CenterX = mouseCanvasPosition.X;
            scaleTransform.CenterY = mouseCanvasPosition.Y;

            scaleTransform.ScaleX = Math.Max(0.1, scaleTransform.ScaleX + scaleFactor);
            scaleTransform.ScaleY = Math.Max(0.1, scaleTransform.ScaleY + scaleFactor);
        }

        private static void AdjustScrollViewer(ScrollViewer scrollViewer, Point mouseCanvasPosition, double scaleFactor, FrameworkElement zoomTargetElement)
        {
            scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + mouseCanvasPosition.X * scaleFactor);
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + mouseCanvasPosition.Y * scaleFactor);

            if (scrollViewer.DataContext is TileGridViewModel vm && TryGetScaleTransform(zoomTargetElement, out ScaleTransform? scaleTransform))
            {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                vm.ScrollViewerZoom = scaleTransform.ScaleX;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            }
        }

        private static bool TryGetScaleTransform(FrameworkElement frameworkElement, out ScaleTransform? scaleTransform)
        {
            // C# 8.0 Switch Expression

            scaleTransform = frameworkElement.LayoutTransform switch
            {
                TransformGroup transformGroup => transformGroup.Children.OfType<ScaleTransform>().FirstOrDefault(),
                ScaleTransform transform => transform,
                _ => null
            };

            return scaleTransform != null;
        }
    }
}
