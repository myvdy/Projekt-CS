using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

public class ShipSizeConverter : IValueConverter
{
    int size;
    bool isWidth;
    int top;
    int left;
    public ShipSizeConverter(int size, bool isWidth)
    {
        this.size = size;
        this.isWidth = isWidth;
    }
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double number)
        {
            return isWidth ? number * size / 11 : number / 11;
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double number)
        {
            double k = (parameter is double koef) ? koef : 0.375;
            return number;
        }
        return value;
    }
}
