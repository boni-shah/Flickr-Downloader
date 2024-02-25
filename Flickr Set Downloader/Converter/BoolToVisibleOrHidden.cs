using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Flickr_Set_Downloader
{
    public class BoolToVisibleOrHidden : IValueConverter
    {
        public BoolToVisibleOrHidden() { }
        public bool Collapse { get; set; }
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool bValue = (bool)value;
            if (bValue)
                return Visibility.Visible;
            else
                if (Collapse)
                    return Visibility.Collapsed;
                else
                    return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Visibility visibility = (Visibility)value;
            if (visibility == Visibility.Visible)
                return true;
            else
                return false;
        }
    }
}
