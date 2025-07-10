using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TileBasedLevelEditor.Misc
{
    class SizeChangedBehavior
    {
        // Required
        public static readonly DependencyProperty IsObservedProperty = DependencyProperty.RegisterAttached(
          "IsObserved", typeof(bool), typeof(SizeChangedBehavior), new PropertyMetadata(default(bool), OnIsObservedChanged));

        public static bool GetIsObserved(DependencyObject obj) => (bool)obj.GetValue(IsObservedProperty);
        public static void SetIsObserved(DependencyObject obj, bool value) => obj.SetValue(IsObservedProperty, value);

        public static void OnIsObservedChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not FrameworkElement f || e.NewValue is not bool newValue)
                return;

            if (newValue)
            {
                f.SizeChanged += OnElementSizeChanged;
            }
            else
            {
                f.SizeChanged -= OnElementSizeChanged;
            }
        }

        public static void OnElementSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is not FrameworkElement f)
                return;

            SetObservedSize(f, f.RenderSize);
        }

        public static readonly DependencyProperty ObservedSizeProperty =
        DependencyProperty.RegisterAttached(
            "ObservedSize",
            typeof(Size),
            typeof(SizeChangedBehavior),
            new FrameworkPropertyMetadata(default(Size), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static Size GetObservedSize(DependencyObject obj) => (Size)obj.GetValue(ObservedSizeProperty);
        public static void SetObservedSize(DependencyObject obj, Size value) => obj.SetValue(ObservedSizeProperty, value);

    }
}
