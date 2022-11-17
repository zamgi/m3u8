using System;
using System.Globalization;

using Avalonia.Data.Converters;
using Avalonia.Media;

using m3u8.download.manager.models;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class RequestRowType2Foreground_Converter : IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            if ( value is RequestRowTypeEnum rrt )
            {
                switch ( rrt )
                {
                    case RequestRowTypeEnum.Error: return (Brushes.Red);
                    default: return (Brushes.Black);
                }
            }
            return (value);
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture ) => throw (new NotImplementedException());
    }
}
