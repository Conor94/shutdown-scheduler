using System;
using System.Globalization;
using System.Windows.Data;

namespace ShutdownSchedulerApplication.Converters
{
    public class ShutdownInTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string shutdownTime && shutdownTime.Length > 0)
            {
                int len = shutdownTime.Length;
                if ((char.IsDigit(shutdownTime[len - 1])) || 
                    ((shutdownTime[len - 1] == '.') && (!shutdownTime.Remove(len - 1, 1).Contains("."))))
                {
                    return shutdownTime;
                }                    
                else
                {
                    return shutdownTime.Remove(len - 1, 1);
                }
            }
            else
            {
                return "";
            }
        }
    }
}
