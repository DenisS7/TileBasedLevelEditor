using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TileBasedLevelEditor.Misc
{
    public class ListViewFillColumnBehavior
    {
        public static readonly DependencyProperty IsListViewObservedProperty = DependencyProperty.RegisterAttached(
          "IsListViewObserved", typeof(bool), typeof(SizeChangedBehavior), new PropertyMetadata(default(bool), OnIsListViewObservedChanged));

        public static bool GetIsListViewObserved(DependencyObject obj) => (bool)obj.GetValue(IsListViewObservedProperty);
        public static void SetIsListViewObserved(DependencyObject obj, bool value) => obj.SetValue(IsListViewObservedProperty, value);
        public static void OnIsListViewObservedChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not ListView l || e.NewValue is not bool newValue)
                return;

            if (newValue)
            {
                l.SizeChanged += OnElementSizeChanged;
            }
            else
            {
                l.SizeChanged -= OnElementSizeChanged;
            }
        }

        public static void OnElementSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is not ListView l)
                return;

            if (l.View is not GridView view)
                return;

            int nameColumnIndex = -1;
            double otherColumnsWidthSum = 0.0;

            for (int i = 0; i < view.Columns.Count; i++) 
            {
                GridViewColumn item = view.Columns[i];
                if (item.Header.ToString() == "Name")
                    nameColumnIndex = i;
                else
                    otherColumnsWidthSum += item.ActualWidth;
            }

            view.Columns[nameColumnIndex].Width = l.ActualWidth - otherColumnsWidthSum;
        }
    }
}
