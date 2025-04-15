using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

public class MarginConverter : IValueConverter
{
    double currentSize;
    int size;
    int j;
    bool isLeft;
    public MarginConverter(double currentSize, int size, int j, bool isLeft)
    {
        this.currentSize = currentSize;
        this.size = size;
        this.j = j;
        this.isLeft = isLeft;
    }
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {

        if (value is double number)
        {
            if (isLeft)
            {
                return currentSize + j * (size * currentSize + currentSize * 4 / 3 / size);
            }
            else
            {
                return size * (currentSize + currentSize / 3);
            }
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value;
    }
}