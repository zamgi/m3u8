using System;
using System.Collections.Generic;
using System.Linq;

using m3u8.download.manager.controllers;
using m3u8.download.manager.infrastructure;
using m3u8.download.manager.models;

using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;

namespace m3u8.download.manager
{
    /// <summary>
    /// 
    /// </summary>
    internal static partial class Extensions_4_DownloadRow
    {
        public static void CancelAll( this DownloadController controller, IEnumerable< DownloadRow > rows )
        {
            foreach ( var row in rows )
            {
                controller.Cancel( row );
            }
        }
        public static void RemoveAllFinished( this DownloadListModel model ) => model.RemoveRows( model.GetAllFinished().ToList() );

        [M(O.AggressiveInlining)] public static bool IsFinished( this DownloadRow row ) => (row.Status == DownloadStatus.Finished);
        [M(O.AggressiveInlining)] public static bool IsFinishedOrError( this DownloadRow row ) => row.Status switch { DownloadStatus.Finished => true, DownloadStatus.Error => true, _ => false };
        [M(O.AggressiveInlining)] public static bool IsFinishedOrErrorOrCreated( this DownloadRow row ) => row.Status switch { DownloadStatus.Finished => true, DownloadStatus.Error => true, DownloadStatus.Created => true, _ => false };
        [M(O.AggressiveInlining)] public static bool IsFinished( this DownloadStatus status ) => (status     == DownloadStatus.Finished);
        [M(O.AggressiveInlining)] public static bool IsError   ( this DownloadRow    row    ) => (row.Status == DownloadStatus.Error);
        [M(O.AggressiveInlining)] public static bool IsRunning ( this DownloadRow    row    ) => (row.Status == DownloadStatus.Running);
        [M(O.AggressiveInlining)] public static bool IsWait    ( this DownloadRow    row    ) => (row.Status == DownloadStatus.Wait);
        [M(O.AggressiveInlining)] public static bool IsPaused  ( this DownloadRow    row    ) => (row.Status == DownloadStatus.Paused);
        [M(O.AggressiveInlining)] public static bool IsPaused  ( this DownloadStatus status ) => (status     == DownloadStatus.Paused);
        [M(O.AggressiveInlining)] public static bool IsRunningOrPaused( this DownloadStatus status ) => status switch { DownloadStatus.Started => true, DownloadStatus.Running => true, DownloadStatus.Paused => true, _ => false };
        [M(O.AggressiveInlining)] public static bool IsRunningOrStarted( this DownloadStatus status ) => status switch { DownloadStatus.Started => true, DownloadStatus.Running => true, _ => false };
        [M(O.AggressiveInlining)] public static bool HasAnyFailedDownloadParts( this DownloadRow row ) => (row.FailedDownloadParts != 0);

        [M(O.AggressiveInlining)] public static long? GetApproxRemainedBytes( this DownloadRow row )
        {
            var processedParts = (row.SuccessDownloadParts + row.FailedDownloadParts);
            if ( processedParts != 0 )
            {
                var d                    = row.DownloadBytesLength;
                var singlePartApproxSize = (1.0 * d / processedParts);
                var approxTotalBytes     = singlePartApproxSize * row.TotalParts;
                var approxRemainedBytes  = Convert.ToInt64( approxTotalBytes - d );
                return (approxRemainedBytes);
            }
            return (null);
        }
        [M(O.AggressiveInlining)] public static long? GetApproxTotalBytes( this DownloadRow row )
        {
            var processedParts = (row.SuccessDownloadParts + row.FailedDownloadParts);
            if ( processedParts != 0 )
            {
                var singlePartApproxSize = (1.0 * row.DownloadBytesLength / processedParts);
                var approxTotalBytes     = Convert.ToInt64( singlePartApproxSize * row.TotalParts );
                return (approxTotalBytes);
            }
            return (null);
        }
        [M(O.AggressiveInlining)] public static long GetLiveStreamMaxFileSizeInMb( this DownloadRow row ) => (row.LiveStreamMaxFileSizeInBytes >> 20);

        [M(O.AggressiveInlining)] public static int CompareTo< T >( in this T? x, in T? y ) where T : struct, IComparable< T >
        {
            if ( x.HasValue )
            {
                if ( y.HasValue )
                {
                    return (x.Value.CompareTo( y.Value ));
                }
                return (1);
            }
            else if ( y.HasValue )
            {
                return (-1);
            }
            return (0);            
        }
        [M(O.AggressiveInlining)] public static byte Min( byte b1, byte b2 ) => ((b1 < b2) ? b1 : b2);

        //public static string GetSpeedText( long downloadBytes, double elapsedSeconds, double? instantSpeedInMbps )
        //{
        //    string speedText;
        //    //if ( downloadBytes < 1_024 ) speedText = GetSpeedInBps( downloadBytes, elapsedSeconds ).ToString("N2") + " bps"; //" bit/s";
        //    if ( downloadBytes < 100_024 ) speedText = GetSpeedInKbps( downloadBytes, elapsedSeconds ).ToString("N2") + " Kbps"; //" Kbit/s";
        //    else                           speedText = GetSpeedInMbps( downloadBytes, elapsedSeconds ).ToString("N1") + " Mbps"; //" Mbit/s";
        //    if ( instantSpeedInMbps.HasValue )
        //    {
        //        speedText += $" (↑{instantSpeedInMbps:N1} Mbps)";
        //    }
        //    return (speedText);
        //}
        private static string GetSpeedText( double speedInBps, long downloadBytes, double? instantSpeedInMbps )
        {
            string speedText;
            //if ( downloadBytes < 1_024 ) speedText = speedInBps.ToString("N2") + " bps"; //" bit/s";
            if ( downloadBytes < 100_024 ) speedText = GetSpeedInKbps( speedInBps ).ToString("N2") + " Kbps"; //" Kbit/s";
            else                           speedText = GetSpeedInMbps( speedInBps ).ToString("N1") + " Mbps"; //" Mbit/s";
            if ( instantSpeedInMbps.HasValue )
            {
                speedText += $" (↑{instantSpeedInMbps:N1} Mbps)";
            }
            return (speedText);
        }
        [M(O.AggressiveInlining)] public static double GetMbps( long downloadBytes ) => (downloadBytes * 1.0 / (1_048_576 / 8));
        [M(O.AggressiveInlining)] private static double GetSpeedInBps( long downloadBytes, double elapsedSeconds ) => (8 * (downloadBytes / elapsedSeconds)); //" bps"; //" bit/s";
        [M(O.AggressiveInlining)] public static double GetSpeedInMbps( long downloadBytes, double elapsedSeconds ) => GetSpeedInMbps( GetSpeedInBps( downloadBytes, elapsedSeconds ) ); //" Mbps"; //" Mbit/s";
        [M(O.AggressiveInlining)] private static double GetSpeedInMbps( double speedInBps ) => speedInBps / 1_048_576; //" Mbps"; //" Mbit/s";
        //[M(O.AggressiveInlining)] private static double GetSpeedInKbps( long downloadBytes, double elapsedSeconds ) => GetSpeedInKbps( GetSpeedInBps( downloadBytes, elapsedSeconds ) ); //" Kbps"; //" Kbit/s";
        [M(O.AggressiveInlining)] private static double GetSpeedInKbps( double speedInBps ) => speedInBps / 1_024; //" Kbps"; //" Kbit/s";
   
        [M(O.AggressiveInlining)] public static string GetSizeFormatted( long sizeInBytes )
        {
            static string to_text( float f ) => f.ToString( (f == Math.Ceiling( f )) ? "N0" : "N2" );

            const float KILOBYTE = 1024;
            const float MEGABYTE = KILOBYTE * KILOBYTE;
            const float GIGABYTE = MEGABYTE * KILOBYTE;

            if ( GIGABYTE < sizeInBytes )
                return (to_text( sizeInBytes / GIGABYTE ) + " GB");
            if ( MEGABYTE < sizeInBytes )
                return (to_text( sizeInBytes / MEGABYTE) + " MB");
            if ( KILOBYTE < sizeInBytes )
                return (to_text( sizeInBytes / KILOBYTE) + " KB");
            return ((sizeInBytes != 0) ? sizeInBytes.ToString("#,#"/*"N0"*/) + " bytes" : "0 bytes");
        }
        [M(O.AggressiveInlining)] public static string GetDisplaySizeText( long sizeInBytes )
        {
            if ( sizeInBytes == 0 )
            {
                return ("-");
            }

            static string to_text( float f ) => f.ToString( (f == Math.Ceiling( f )) ? "N0" : "N2" );

            const float KILOBYTE = 1024;
            const float MEGABYTE = KILOBYTE * KILOBYTE;
            const float GIGABYTE = MEGABYTE * KILOBYTE;

            if ( GIGABYTE < sizeInBytes )
                return (to_text( sizeInBytes / GIGABYTE ) + " GB");
            if ( MEGABYTE < sizeInBytes )
                return (to_text( sizeInBytes / MEGABYTE) + " MB");
            if ( KILOBYTE < sizeInBytes )
                return (to_text( sizeInBytes / KILOBYTE ) + " KB");
            return ((sizeInBytes / KILOBYTE).ToString("N1") + " KB");
        }

        public static string GetSizeInMbFormatted( long sizeInBytes )
        {
            var sizeInMb = sizeInBytes >> 20;
            return ((0 < sizeInMb) ? sizeInMb.ToString("0,0") : "0");
        }
        public static string GetSizeInMbFormatted( ulong sizeInBytes )
        {
            var sizeInMb = sizeInBytes >> 20;
            return ((0 < sizeInMb) ? sizeInMb.ToString("0,0") : "0");
        }
        public static string GetElapsedFormatted( this in TimeSpan ts )
        {
            if ( 1 < ts.TotalHours   ) return (ts.ToString( HH_MM_SS ));
            if ( 1 < ts.TotalSeconds ) return (':' + ts.ToString( MM_SS ));
            return (ts.ToString());
        }

        [M(O.AggressiveInlining)] public static bool TryGetApproxRemainedTime( this DownloadRow row, out TimeSpan approxRemainedTime )
        {
            if ( row.Status == DownloadStatus.Running)
            {
                var totalBytes = row.GetApproxTotalBytes();
                if ( totalBytes.HasValue )
                {
                    var elapsedSeconds = row.GetElapsed().TotalSeconds;
                    var downloadBytes  = row.GetDownloadBytesLengthAfterLastRun();                    
                    if ( (1_000 < downloadBytes) && (2.5 <= elapsedSeconds) )
                    {
                        var remainedBytes = totalBytes.Value - (row.DownloadBytesLength - downloadBytes);
                        var remainedTime  = TimeSpan.FromSeconds( (remainedBytes - downloadBytes) * (elapsedSeconds / downloadBytes) );
                        approxRemainedTime = remainedTime;
                        return (true);
                    }
                }
            }
            approxRemainedTime = default;
            return (true);
        }
        [M(O.AggressiveInlining)] public static bool TryGetDownloadSpeedInBps( this DownloadRow row, out double speedInBps )
        {
            if ( !row.Status.IsPaused() )
            {
                var elapsedSeconds = row.GetElapsed4SpeedMeasurement().TotalSeconds;
                var downloadBytes  = row.GetDownloadBytesLengthAfterLastRun();
                if ( (1_024 < downloadBytes) && (2.5 <= elapsedSeconds) )
                {
                    speedInBps = GetSpeedInBps( downloadBytes, elapsedSeconds );
                    return (true);
                }
            }
            speedInBps = default;
            return (false);
        }
        [M(O.AggressiveInlining)] public static bool TryGetDownloadSpeedText( this DownloadRow row, out string speedText )
        {
            if ( row.TryGetDownloadSpeedInBps( out var speedInBps ) )
            {
                var downloadBytes = row.GetDownloadBytesLengthAfterLastRun();
                speedText = GetSpeedText( speedInBps, downloadBytes, row.GetInstantSpeedInMbps() );
                return (true);
            }
            speedText = default;
            return (false);
        }

        private const string CREATED_DT = "HH:mm:ss  (yyyy.MM.dd)";
        private const string HH_MM_SS   = "hh\\:mm\\:ss";
        private const string MM_SS      = "mm\\:ss";

        [M(O.AggressiveInlining)] private static bool TryGetDownloadProgressText( this DownloadRow row, out string progressText )
        {
            var st = row.Status;
            switch ( st )
            {
                case DownloadStatus.Created:
                case DownloadStatus.Started:
                case DownloadStatus.Wait   :
                    progressText = null;
                    return (false);

                default:
                    string percentText;
                    if ( 0 < row.TotalParts )
                    {
                        var part    = (1.0 * row.SuccessDownloadParts) / row.TotalParts;
                        var percent = (row.TotalParts <= (row.SuccessDownloadParts + row.FailedDownloadParts)) ? 100 : Extensions_4_DownloadRow.Min( (byte) (100 * part), 99 );
                        percentText = percent.ToString();
                    }
                    else if ( st == DownloadStatus.Canceled ) //not-started
                    {
                        progressText = null;
                        return (false);
                    }
                    else
                    {
                        percentText = "-";
                    }

                    var failedParts = ((row.FailedDownloadParts != 0) ? $", [failed: {row.FailedDownloadParts}]" : null);
                    progressText = $"{percentText}%  ({row.SuccessDownloadParts} of {row.TotalParts}{failedParts})";
                    return (true);
            }
        }
        [M(O.AggressiveInlining)] private static bool TryGetDownloadProgressPartValue( this DownloadRow row, out double part )
        {
            var st = row.Status;
            switch ( st )
            {
                case DownloadStatus.Created:
                case DownloadStatus.Started:
                case DownloadStatus.Wait   :
                    part = 0d;
                    return (false);

                default:
                    if ( 0 < row.TotalParts )
                    {
                        part = 100 * ((1.0 * row.SuccessDownloadParts) / row.TotalParts);
                    }
                    else if ( st == DownloadStatus.Canceled ) //not-started
                    {
                        part = 0d;
                        return (false);
                    }
                    else
                    {
                        part = 0d;
                    }
                    return (true);
            }
        }
        [M(O.AggressiveInlining)] public static string GetProgressText( this DownloadRow row ) => (TryGetDownloadProgressText( row, out var progressText ) ? progressText : "-");
        [M(O.AggressiveInlining)] public static double GetProgressPartValue( this DownloadRow row ) => (TryGetDownloadProgressPartValue( row, out var part ) ? part : 0d);
        [M(O.AggressiveInlining)] public static bool TryGetDownloadProgress( this DownloadRow row, out (double suc, double fail) parts, out string progressText )
        {
            var st = row.Status;
            switch ( st )
            {
                case DownloadStatus.Created:
                case DownloadStatus.Started:
                case DownloadStatus.Wait   :
                    parts        = default;
                    progressText = null;
                    return (false);

                default:
                    (var totalParts, var successDownloadParts, var failedDownloadParts) = (row.TotalParts, row.SuccessDownloadParts, row.FailedDownloadParts);
                    string percentText;
                    if ( 0 < totalParts )
                    {
                        var suc  = (1.0 * successDownloadParts) / totalParts;
                        var fail = (1.0 * failedDownloadParts ) / totalParts;
                        parts = (suc, fail);
                        var percent = (totalParts <= (successDownloadParts + failedDownloadParts)) ? 100 : Min( (byte) (100 * suc), 99 );
                        percentText = percent.ToString();
                    }
                    else if ( st == DownloadStatus.Canceled ) //not-started
                    {
                        parts        = default;
                        progressText = null;
                        return (false);
                    }
                    else
                    {
                        parts       = (0, 0);
                        percentText = "-";
                    }

                    var failedParts = ((failedDownloadParts != 0) ? $", [failed: {failedDownloadParts}]" : null);
                    progressText = $"{percentText}%  ({successDownloadParts} of {totalParts}{failedParts})";
                    return (true);
            }
        }
        [M(O.AggressiveInlining)] public static string GetDownloadTimeText( this DownloadRow row )
        {
            if ( row.Status == DownloadStatus.Created )
            {
                return (row.CreatedOrStartedDateTime.ToString( CREATED_DT ));
            }
            return (row.GetElapsed().ToString( HH_MM_SS ));
        }
        [M(O.AggressiveInlining)] public static string GetDownloadSpeedText( this DownloadRow row, string defVal = "" ) => row.TryGetDownloadSpeedText( out var speedText ) ? speedText : defVal;
        [M(O.AggressiveInlining)] public static string GetApproxRemainedTimeText( this DownloadRow row, string defVal = "" ) => row.TryGetApproxRemainedTime( out var remainedTime ) ? remainedTime.ToString( HH_MM_SS ) : defVal;
        [M(O.AggressiveInlining)] public static string GetApproxRemainedBytesText( this DownloadRow row, string defVal = "" )
        {
            var size = row.GetApproxRemainedBytes();
            return (size.HasValue ? /*FileHelper.*/GetDisplaySizeText( size.Value ) : defVal);
        }
        [M(O.AggressiveInlining)] public static string GetApproxTotalBytesText( this DownloadRow row, string defVal = "" )
        {
            var size = row.GetApproxTotalBytes();
            return (size.HasValue ? /*FileHelper.*/GetDisplaySizeText( size.Value ) : defVal);
        }

        [M(O.AggressiveInlining)] public static string GetDownloadInfoText( this DownloadRow row )
        {
            var st = row.Status;
            switch ( st )
            {
                case DownloadStatus.Created: return ($"[created]: {row.CreatedOrStartedDateTime.ToString( CREATED_DT )}");
                case DownloadStatus.Started: return ($"{row.GetElapsed().ToString( HH_MM_SS )}");
                case DownloadStatus.Wait   : return ($"(wait), ({row.GetElapsed().ToString( HH_MM_SS )})");
            }

            var ts           = row.GetElapsed();
            var elapsed      = ((1 < ts.TotalHours) ? ts.ToString( HH_MM_SS ) : (':' + ts.ToString( MM_SS )));
            var percent      = ((0 < row.TotalParts) ? Convert.ToByte( (100.0 * row.SuccessDownloadParts) / row.TotalParts ).ToString() : "-");
            //var failedParts  = ((row.FailedDownloadParts != 0) ? $", [failed: {row.FailedDownloadParts}]" : null);
            var downloadInfo = $"{percent}%, ({elapsed})";
            
            #region [.speed.]
            if ( row.TryGetDownloadSpeedText( out var speedText ) ) downloadInfo += $", [{speedText}]";
            #endregion

            return (downloadInfo);
        }
    }
}
