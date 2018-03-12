using GalaSoft.MvvmLight;
using System;
using System.Globalization;
using System.Windows.Data;

namespace LoadingWindow.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class ValueConverterViewModel : ViewModelBase
    {
        [ValueConversion(typeof(int), typeof(double))]
        public class ValueToAngleConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return (double)(((int)value * 0.01) * 100);
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return (int)((double)value / 100) * 100;
            }
        }
    }
}