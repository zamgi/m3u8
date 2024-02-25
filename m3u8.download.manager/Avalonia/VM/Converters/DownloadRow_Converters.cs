using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;

using m3u8.download.manager.models;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class _DownloadRow_ConverterBase : IValueConverter
    {
        public abstract object Convert( object value, Type targetType, object parameter, CultureInfo culture );
        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture ) => null; // value; //throw (new NotImplementedException());
    }    
    /// <summary>
    /// 
    /// </summary>
    public sealed class ProgressText_Converter : _DownloadRow_ConverterBase
    {
        public override object Convert( object value, Type targetType, object parameter, CultureInfo culture )
            => ((value is DownloadRow row) ? DownloadListUC.GetProgressText( row ) : value);
    }
    /// <summary>
    /// 
    /// </summary>
    public sealed class ProgressPartValue_Converter : _DownloadRow_ConverterBase
    {
        public override object Convert( object value, Type targetType, object parameter, CultureInfo culture )
            => ((value is DownloadRow row) ? DownloadListUC.GetProgressPartValue( row ) : 0d);
    }    

    /// <summary>
    /// 
    /// </summary>
    public sealed class DownloadTimeText_Converter : _DownloadRow_ConverterBase
    {
        public override object Convert( object value, Type targetType, object parameter, CultureInfo culture )
            => ((value is DownloadRow row) ? DownloadListUC.GetDownloadTimeText( row ) : value);
    }
    /// <summary>
    /// 
    /// </summary>
    public sealed class ApproxRemainedTimeText_Converter : _DownloadRow_ConverterBase
    {
        public override object Convert( object value, Type targetType, object parameter, CultureInfo culture )
            => ((value is DownloadRow row) ? DownloadListUC.GetApproxRemainedTimeText( row ) : value);
    }
    /// <summary>
    /// 
    /// </summary>
    public sealed class DownloadSpeedText_Converter : _DownloadRow_ConverterBase
    {
        public override object Convert( object value, Type targetType, object parameter, CultureInfo culture )
            => ((value is DownloadRow row) ? DownloadListUC.GetDownloadSpeedText( row ) : value);
    }
    /// <summary>
    /// 
    /// </summary>
    public sealed class DisplaySizeText_Converter : _DownloadRow_ConverterBase
    {
        public override object Convert( object value, Type targetType, object parameter, CultureInfo culture )
            => ((value is DownloadRow row) ? DownloadListUC.GetDisplaySizeText( row.DownloadBytesLength ) : value);
    }
    /// <summary>
    /// 
    /// </summary>
    public sealed class ApproxRemainedBytesText_Converter : _DownloadRow_ConverterBase
    {
        public override object Convert( object value, Type targetType, object parameter, CultureInfo culture )
            => ((value is DownloadRow row) ? DownloadListUC.GetApproxRemainedBytesText( row ) : value);
    }
    /// <summary>
    /// 
    /// </summary>
    public sealed class ApproxTotalBytesText_Converter : _DownloadRow_ConverterBase
    {
        public override object Convert( object value, Type targetType, object parameter, CultureInfo culture )
            => ((value is DownloadRow row) ? DownloadListUC.GetApproxTotalBytesText( row ) : value);
    }
    /// <summary>
    /// 
    /// </summary>
    public sealed class StatusImage_Converter : _DownloadRow_ConverterBase
    {
        private Dictionary< DownloadStatus, Bitmap > _Dict;
        public StatusImage_Converter() => _Dict = new Dictionary< DownloadStatus, Bitmap >();

        public override object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            if ( value is DownloadStatus status )
            {
                if ( _Dict.TryGetValue( status, out var bitmap ) )
                {
                    return (bitmap);
                }

                var img = default(string);
                switch ( status )
                {
                    case DownloadStatus.Created : img = "created";  break;
                    case DownloadStatus.Started : img = "running";  break;
                    case DownloadStatus.Running : img = "running";  break;
                    case DownloadStatus.Paused  : img = "paused";   break;
                    case DownloadStatus.Wait    : img = "wait";     break;
                    case DownloadStatus.Canceled: img = "canceled"; break;
                    case DownloadStatus.Finished: img = "finished"; break;
                    case DownloadStatus.Error   : img = "error";    break;
                }
                if ( img != null )
                {
                    bitmap = new Bitmap( ResourceLoader._GetResource_( $"/Resources/statuses/{img}.png" ) );
                    _Dict.Add( status, bitmap );                    
                }
                return (bitmap);
            }
            return (value);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class IsLiveStream_Converter : _DownloadRow_ConverterBase
    {
        public override object Convert( object value, Type targetType, object parameter, CultureInfo culture )
            => ((value is DownloadRow row) ? (row.IsLiveStream ? "YES" : "-") : value);
    }
    /// <summary>
    /// 
    /// </summary>
    public sealed class LiveStreamMaxFileSizeInMb_Converter : _DownloadRow_ConverterBase
    {
        public override object Convert( object value, Type targetType, object parameter, CultureInfo culture )
            => ((value is DownloadRow row) ? (row.IsLiveStream ? $"{row.GetLiveStreamMaxFileSizeInMb()} mb" : "-") : value);
    }
    /// <summary>
    /// 
    /// </summary>
    public sealed class LiveStreamMaxFileSizeInMb_ToolTip_Converter : _DownloadRow_ConverterBase
    {
        public override object Convert( object value, Type targetType, object parameter, CultureInfo culture )
            => ((value is DownloadRow row) ? (row.IsLiveStream ? $"Is Live Stream, (max single output file size: {row.GetLiveStreamMaxFileSizeInMb()} mb)" : string.Empty) : value);
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract class _RequestHeaders_Converter_Base : _DownloadRow_ConverterBase
    {
        private StringBuilder _Buf;
        public _RequestHeaders_Converter_Base() => _Buf = new StringBuilder();

        protected string Convert2Text( IDictionary< string, string > requestHeaders, string key_value_separator = ": ", string lines_separator = "; " )
        {
            if ( requestHeaders.AnyEx() )
            {                
                foreach ( var p in requestHeaders )
                {
                    if ( _Buf.Length != 0 ) _Buf.Append( lines_separator );
                    _Buf.Append( p.Key ).Append( key_value_separator ).Append( p.Value );
                }
                var txt = _Buf.ToString();
                _Buf.Clear();
                return (txt);
            }
            return (null);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public sealed class RequestHeaders_Converter : _RequestHeaders_Converter_Base
    {
        public RequestHeaders_Converter() { }
        public override object Convert( object value, Type targetType, object parameter, CultureInfo culture )
            => ((value is DownloadRow row) ? Convert2Text( row.RequestHeaders ) : value);
    }
    /// <summary>
    /// 
    /// </summary>
    public sealed class RequestHeaders_ToolTip_Converter : _RequestHeaders_Converter_Base
    {
        public RequestHeaders_ToolTip_Converter() { }
        public override object Convert( object value, Type targetType, object parameter, CultureInfo culture )
            => ((value is DownloadRow row) ? Convert2Text( row.RequestHeaders, lines_separator: "\r\n" ) : value);
    }
}
