using System;
using System.Globalization;
using System.Windows.Data;

namespace BliveHelper.Utils.Converter
{
    internal class ValueToBooleanConverter : IValueConverter
    {
        public bool Inverse { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = false;

            if (value is bool boolValue) result = boolValue;
            else if (value is int intValue) result = intValue > 0;
            else if (value is float floatValue) result = floatValue > 0;
            else if (value is double doubleValue) result = doubleValue > 0;
            else if (value is string stringValue) result = !string.IsNullOrWhiteSpace(stringValue);
            else if (value != null) result = true;

            return !Inverse ? result : !result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
