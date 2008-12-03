using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace SEWilson.ScreenSaver.TheArmory.UI
{
    [ValueConversion(typeof(int), typeof(Visibility))]
    public class IntegerVisibilityConverter : IValueConverter
    {
        public int MinValueInclusive = 1;
        public int MaxValueInclusive = int.MaxValue;
        public Visibility VisibilityOutOfRange = Visibility.Collapsed;
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int i = int.Parse(System.Convert.ToString(value));
            return (i <= MaxValueInclusive) && (i >= MinValueInclusive)
                ? Visibility.Visible
                : VisibilityOutOfRange;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
