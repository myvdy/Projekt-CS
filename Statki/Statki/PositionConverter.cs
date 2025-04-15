using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

public class PositionConverter : IValueConverter
{
    private readonly int index;
    private readonly bool isTop;

    public PositionConverter(int index, bool isTop)
    {
        this.index = index;
        this.isTop = isTop;
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double gridDimension)
        {
            return index * (gridDimension / 11);
        }
        return 0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
