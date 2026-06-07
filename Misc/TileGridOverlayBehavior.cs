using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TileBasedLevelEditor.Misc
{
    public static class TileGridOverlayBehavior
    {
        public static readonly DependencyProperty ContentOverlay =
            DependencyProperty.RegisterAttached("Content",
            typeof(object), typeof(TileGridOverlayBehavior));

        public static void SetContent(DependencyObject obj, object value)
        {
            obj.SetValue(ContentOverlay, value);
        }

        public static object GetContent(DependencyObject obj)
        {
            return obj.GetValue(ContentOverlay);
        }
    }
}
