using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TileBasedLevelEditor.Misc
{
    public static class ScrollToCenterBehavior
    {
        public static readonly DependencyProperty ScrollToCenterProperty = DependencyProperty.RegisterAttached(
          "ScrollToCenter", typeof(bool), typeof(ScrollToCenterBehavior), new PropertyMetadata(default(bool), OnScrollToCenterChanged));

        public static void SetScrollToCenter(DependencyObject obj, bool value) => obj.SetValue(ScrollToCenterProperty, value);

        public static bool GetScrollToCenter(DependencyObject obj) => (bool)obj.GetValue(ScrollToCenterProperty);

        public static void OnScrollToCenterChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not ScrollViewer scrollViewer)
                return;

            if ((bool)e.NewValue)
            {
                if (scrollViewer.IsLoaded)
                    ScrollToCenterRequest(scrollViewer, new RoutedEventArgs());
                else
                    scrollViewer.Loaded += ScrollToCenterRequest;
            }
            else
            {
                scrollViewer.Loaded -= ScrollToCenterRequest;
            }
        }

        public static void ScrollToCenterRequest(object sender, RoutedEventArgs e)
        {
            if (sender is not ScrollViewer scrollViewer)
                return;

            scrollViewer.ScrollToHorizontalOffset(scrollViewer.ScrollableWidth / 2.0);
            scrollViewer.ScrollToVerticalOffset(scrollViewer.ScrollableHeight / 2.0);
        }

    }
}
