using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace TileBasedLevelEditor.Converters
{
    public class GridTileToPixelConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            //System.Diagnostics.Debug.WriteLine($"V1: {values[0]}        V2: {values[1]}         V3:{values[2]}");
            if (values[0] is not int index || values[1] is not int tileSize || values[2] is not int margin)
                return 0;

            return (double)(index * (tileSize + margin));
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
