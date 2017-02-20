using System;
using System.Globalization;
using System.Windows.Data;

namespace DataSpider.WPF.ValueConvertor
{
    public class NullableValueConvertor : BaseValueConvertor, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value, targetType);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value, targetType);
        }
    }
}
