using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;

namespace MyHome
{
    public class ShutterStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            Debug.WriteLine("Status: " + value + ", Parameter: " + parameter + ", Type: " + targetType);
            SolidColorBrush brush = new SolidColorBrush();
            Color targetColor = (Color)XamlBindingHelper.ConvertValue(typeof(Color), parameter);
            brush.Color = ((bool) value) ? targetColor : Colors.Gray;

            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
