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
    public sealed class RequestRowType_2_Foreground_Converter : IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            if ( value is RequestRowTypeEnum rrt )
            {
                switch ( rrt )
                {
                    case RequestRowTypeEnum.Error: return (Brushes.Red);
                    default: return (Brushes.DimGray/*Black*/);
                }
            }
            return (value);
        }
        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture ) => throw (new NotImplementedException());
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class RequestRowType_2_SelectedForeground_Converter : IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            if ( value is RequestRowTypeEnum rrt )
            {
                switch ( rrt )
                {
                    case RequestRowTypeEnum.Error: return (Brushes.OrangeRed/*new SolidColorBrush( Color.FromRgb( 0xff, 0x36, 0x0 ) )*/);
                    default: return (Brushes.White/*new SolidColorBrush( Color.FromRgb( 57, 57, 57 ) )*/);
                }
            }
            return (value);
        }
        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture ) => throw (new NotImplementedException());
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class RequestRowType_2_Background_Converter : IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            if ( value is RequestRowTypeEnum rrt )
            {
                switch ( rrt )
                {
                    case RequestRowTypeEnum.Error: return (Brushes.LightYellow);
                    default: return (Brushes.White);
                }
            }
            return (value);
        }
        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture ) => throw (new NotImplementedException());
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class RequestRowType_2_SelectedBackground_Converter : IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            if ( value is RequestRowTypeEnum rrt )
            {
                switch ( rrt )
                {
                    case RequestRowTypeEnum.Error: return (new SolidColorBrush( Color.FromRgb( 0xe0, 0xeb, 0xeb ) )/*Brushes.Khaki*/);
                    default: return (Brushes.CadetBlue/*LightSkyBlue*/);
                }
            }
            return (value);
        }
        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture ) => throw (new NotImplementedException());
    }
}
