using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace SEWilson.ScreenSaver.TheArmory.UI
{
    [ValueConversion(typeof(string), typeof(Visibility))]
    public class StringIsNullOrEmptyVisibilityConverter : IValueConverter
    {
        public Visibility VisibilityWhenNullOrEmpty = Visibility.Collapsed;
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return string.IsNullOrEmpty(System.Convert.ToString(value ?? "")) 
                ? Visibility.Visible 
                : VisibilityWhenNullOrEmpty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
