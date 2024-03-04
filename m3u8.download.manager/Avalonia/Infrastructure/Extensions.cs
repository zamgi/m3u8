using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using MsBox.Avalonia;
using MsBox.Avalonia.Base;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;

using m3u8.download.manager.controllers;
using m3u8.download.manager.models;
using m3u8.download.manager.Properties;
using m3u8.download.manager.ui;

using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;

namespace m3u8.download.manager
{
    /// <summary>
    /// 
    /// </summary>
    internal static class Extensions
    {
        [M(O.AggressiveInlining)] public static bool IsNullOrEmpty( this string s ) => string.IsNullOrEmpty( s );
        [M(O.AggressiveInlining)] public static bool IsNullOrWhiteSpace( this string s ) => string.IsNullOrWhiteSpace( s );
        [M(O.AggressiveInlining)] public static bool HasFirstCharNotDot( this string s ) => (s != null) && (0 < s.Length) && (s[ 0 ] != '.');
        [M(O.AggressiveInlining)] public static string GetValueIfNotNullOrWhiteSpaceOrDefault( this string s, string defVal ) => (s.IsNullOrWhiteSpace() ? defVal : s);
        [M(O.AggressiveInlining)] public static bool AnyEx< T >( this IEnumerable< T > seq ) => (seq != null) && seq.Any();
        //[M(O.AggressiveInlining)] public static bool AnyEx< T >( this ICollection< T > seq ) => (seq != null) && (0 < seq.Count);
        [M(O.AggressiveInlining)] public static bool AnyEx< T >( this IReadOnlyCollection< T > seq ) => (seq != null) && (0 < seq.Count);
        [M(O.AggressiveInlining)] public static bool AnyEx< T >( this T[] seq ) => (seq != null) && (0 < seq.Length);
        [M(O.AggressiveInlining)] public static bool EqualIgnoreCase( this string s1, string s2 ) => (string.Compare( s1, s2, true ) == 0);
        [M(O.AggressiveInlining)] public static bool ContainsIgnoreCase( this string s1, string s2 ) => ((s1 != null) && s1.Contains( s2, StringComparison.InvariantCultureIgnoreCase ));

        public static T[] ToArrayEx< T >( this IReadOnlyList< T > seq )
        {
            var array = new T[ seq.Count ];
            for ( var i = seq.Count - 1; 0 <= i; i-- )
            {
                array[ i ] = seq[ i ];
            }
            return (array);
        }
        public static T[] ToArrayEx< T >( this IReadOnlyCollection< T > seq )
        {
            var array = new T[ seq.Count ];
            var i = 0;
            foreach ( var t in seq )
            {
                array[ i++ ] = t;
            }
            return (array);
        }
        public static List< T > ToList< T >( this IEnumerable< T > seq, int capacity )
        {
            var list = new List< T >( capacity );
            list.AddRange( seq );
            return (list);
        }

        /// <summary>
        /// Copy user settings from previous application version if necessary
        /// </summary>
        [M(O.AggressiveInlining)] public static void UpgradeIfNeed( this Settings settings )
        {
            // Copy user settings from previous application version if necessary
            if ( !settings._IsUpgradedInThisVersion )
            {
                settings.Upgrade();
                settings._IsUpgradedInThisVersion = true;
                settings.MaxCrossDownloadInstance = settings.MaxCrossDownloadInstanceSaved;
                settings.SaveNoThrow();
            }
        }
        [M(O.AggressiveInlining)] public static void SaveNoThrow( this Settings settings )
        {
            try
            {
                settings.Save();
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );
            }
        }
        public static IEnumerable< string > GetNameCleanerExcludesWords( this Settings settings ) => settings.NameCleanerExcludesWords?.Cast< string >();
        public static void ResetNameCleanerExcludesWords( this Settings settings, IReadOnlyCollection< string > excludesWords )
        {
            if ( settings.NameCleanerExcludesWords == null )
            {
                settings.NameCleanerExcludesWords = new StringCollection();
            }
            else
            {
                settings.NameCleanerExcludesWords.Clear();
            }
            settings.NameCleanerExcludesWords.AddRange( excludesWords.ToArrayEx() );
        }
        public static string GetDownloadListColumnsInfoJson( this SettingsPropertyChangeController settingsController ) => settingsController.Settings.DownloadListColumnsInfoJson;
        public static void SetDownloadListColumnsInfoJson( this SettingsPropertyChangeController settingsController, string json ) => settingsController.Settings.DownloadListColumnsInfoJson = json;

        [M(O.AggressiveInlining)] public static string TrimIfLongest( this string s, int maxLength ) => ((maxLength < s.Length) ? (s.Substring( 0, maxLength ) + "..." ) : s);

        public static string ToJSON< T >( this T t )
        {
            var ser = new DataContractJsonSerializer( typeof(T) );

            using ( var ms = new MemoryStream() )
            {                
                ser.WriteObject( ms, t );
                var json = Encoding.UTF8.GetString( ms.GetBuffer(), 0, (int) ms.Position );
                //var json = Encoding.UTF8.GetString( ms.ToArray() );
                return (json);
            }
        }
        public static T FromJSON< T >( string json )
        {
            var ser = new DataContractJsonSerializer( typeof(T) );

            using ( var ms = new MemoryStream( Encoding.UTF8.GetBytes( json ) ) )
            {
                var t = (T) ser.ReadObject( ms );
                return (t);
            }
        }

        public static bool TryGetM3u8FileUrl( string m3u8FileUrlText, out (Uri m3u8FileUrl, Exception error) t )
        {
            try
            {
                var m3u8FileUrl = new Uri( m3u8FileUrlText );
                if ( (m3u8FileUrl.Scheme != Uri.UriSchemeHttp) && (m3u8FileUrl.Scheme != Uri.UriSchemeHttps) )
                {
                    throw (new ArgumentException( $"Only '{Uri.UriSchemeHttp}' and '{Uri.UriSchemeHttps}' schemes are allowed.", nameof( m3u8FileUrl ) ));
                }
                t = (m3u8FileUrl, null);
                return (true);
            }
            catch ( Exception ex )
            {
                t = (null, ex);
                return (false);
            }
        }

        [M(O.AggressiveInlining)] public static void Cancel_NoThrow( this CancellationTokenSource cts )
        {
            try
            {
                cts.Cancel();
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );
            }
        }
        [M(O.AggressiveInlining)] public static void Set_NoThrow( this ManualResetEventSlim evt )
        {
            try
            {
                evt.Set();
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );
            }
        }
        [M(O.AggressiveInlining)] public static bool Reset_NoThrow( this ManualResetEventSlim evt )
        {
            try
            {
                evt.Reset();
                return (true);
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );
            }
            return (false);
        }

        public static void CancelAll( this DownloadController controller, IEnumerable<DownloadRow> rows )
        {
            foreach ( var row in rows )
            {
                controller.Cancel( row );
            }
        }
        public static void RemoveAllFinished( this DownloadListModel model ) => model.RemoveRows( model.GetAllFinished().ToList() );

        [M(O.AggressiveInlining)] public static bool IsFinished( this DownloadRow    row    ) => (row.Status == DownloadStatus.Finished);
        [M(O.AggressiveInlining)] public static bool IsFinishedOrError( this DownloadRow row ) => row.Status switch { DownloadStatus.Finished => true, DownloadStatus.Error => true, _ => false };
        [M(O.AggressiveInlining)] public static bool IsFinished( this DownloadStatus status ) => (status     == DownloadStatus.Finished);
        [M(O.AggressiveInlining)] public static bool IsError   ( this DownloadRow    row    ) => (row.Status == DownloadStatus.Error);
        [M(O.AggressiveInlining)] public static bool IsRunning ( this DownloadRow    row    ) => (row.Status == DownloadStatus.Running);
        [M(O.AggressiveInlining)] public static bool IsWait    ( this DownloadRow    row    ) => (row.Status == DownloadStatus.Wait);
        [M(O.AggressiveInlining)] public static bool IsPaused  ( this DownloadRow    row    ) => (row.Status == DownloadStatus.Paused);
        [M(O.AggressiveInlining)] public static bool IsPaused  ( this DownloadStatus status ) => (status     == DownloadStatus.Paused);
        [M(O.AggressiveInlining)] public static bool IsRunningOrPaused( this DownloadStatus status ) => status switch { DownloadStatus.Started => true, DownloadStatus.Running => true, DownloadStatus.Paused => true, _ => false };

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

        [M(O.AggressiveInlining)] public static void Dispose_NoThrow( this IDisposable disposable )
        {
            try
            {
                disposable?.Dispose();
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );
            }
        }

        //----------------------------------------------------------------------------------//
        public static Window GetMainWindow() => ((Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) ? desktop.MainWindow : null);
        public static Window GetTopWindow() => ((Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) ? (desktop.Windows.LastOrDefault() ?? desktop.MainWindow) : null);

        public static Task ShowDialogEx( this Window dialog ) => dialog.ShowDialog( GetMainWindow() );

        #region [.MessageBox's.]
        public static IMsBox< ButtonResult > Create_MsBoxStandardWindow( string text, string caption, ButtonEnum buttons, Icon icon, FontFamily fontFamily = null )
        {
            static ClickEnum get_EnterDefaultButton( ButtonEnum buttons ) => buttons switch
            {
                ButtonEnum.Ok          => ClickEnum.Ok,
                ButtonEnum.YesNo       => ClickEnum.Yes,
                ButtonEnum.OkCancel    => ClickEnum.Ok,
                ButtonEnum.OkAbort     => ClickEnum.Ok,
                ButtonEnum.YesNoCancel => ClickEnum.Yes,
                ButtonEnum.YesNoAbort  => ClickEnum.Yes,
                _ => ClickEnum.Ok,
            };
            static ClickEnum get_EscDefaultButton( ButtonEnum buttons ) => buttons switch
            { 
                ButtonEnum.Ok          => ClickEnum.Ok,
                ButtonEnum.YesNo       => ClickEnum.No,
                ButtonEnum.OkCancel    => ClickEnum.Cancel,
                ButtonEnum.OkAbort     => ClickEnum.Abort,
                ButtonEnum.YesNoCancel => ClickEnum.Cancel,
                ButtonEnum.YesNoAbort  => ClickEnum.Abort,
                _ => ClickEnum.Cancel,
            };

            var p = new MessageBoxStandardParams()
            { 
                ButtonDefinitions     = buttons,
                EnterDefaultButton    = get_EnterDefaultButton( buttons ),
                EscDefaultButton      = get_EscDefaultButton  ( buttons ),
                Icon                  = icon,
                ContentTitle          = caption,
                ContentMessage        = text,
                CanResize             = true,
                WindowIcon            = new WindowIcon( ResourceLoader._GetResource_( "/Resources/m3u8_32x36.ico" ) ),
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
            };
            if ( fontFamily != null ) p.FontFamily = fontFamily;

            var msgbox = MessageBoxManager.GetMessageBoxStandard( p );

            #region comm. not-allowed anymore. [.adjustment of the created window (through reflection).]
            /*
            var window_field = msgbox.GetType().GetField( "_window", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic );
            if ( (window_field != null) && (window_field.GetValue( msgbox ) is Window window) )
            {
                window.ShowInTaskbar = false;
                #region comm.
                //window.Opened += async (s, e) =>
                //{
                //    var w = window.Width;
                //    window.Width = w - 1;
                //    await Task.Delay( 1 );
                //    window.Width = w;
                //};
                #endregion
            }
            //*/
            #endregion

            return (msgbox);
        }
        public static async Task< ButtonResult > ShowEx( this IMsBox< ButtonResult > msgbox )
        {
            var window = GetTopWindow();
            if ( window != null )
            {
                return (await msgbox.ShowWindowDialogAsync( window ));
            }
            else
            {
                return (await msgbox.ShowWindowAsync());
            }
        }

        public static Task MessageBox_Show( string text, string caption, Icon icon ) => MessageBox_ShowWithOkButton( text, caption, icon );
        public static Task MessageBox_ShowError( string text, string caption ) => MessageBox_ShowWithOkButton( text, caption, Icon.Error );
        public static Task MessageBox_ShowError( this Exception ex, string caption ) => MessageBox_ShowError( ex.ToString(), caption );
        public static Task MessageBox_ShowInformation( this Window window, string text, string caption ) => Create_MsBoxStandardWindow( text, caption, ButtonEnum.Ok, Icon.Info ).ShowWindowDialogAsync( window );
        public static Task MessageBox_ShowError( this Window window, string text, string caption ) => Create_MsBoxStandardWindow( text, caption, ButtonEnum.Ok, Icon.Error ).ShowWindowDialogAsync( window );
        public static Task< ButtonResult > MessageBox_ShowQuestion( this Window window, string text, string caption, ButtonEnum buttons = ButtonEnum.YesNo ) => Create_MsBoxStandardWindow( text, caption, buttons, Icon.Info ).ShowWindowDialogAsync( window );
        private static async Task MessageBox_ShowWithOkButton( string text, string caption, Icon icon )
        {
            var msgbox = Create_MsBoxStandardWindow( text, caption, ButtonEnum.Ok, icon );
            var window = GetTopWindow();
            if ( window != null )
            {
                await msgbox.ShowWindowDialogAsync( window );
            }
            else
            {
                await msgbox.ShowAsync();
            }
        }
        #endregion

        #region [.allowed Command by current status.]
        [M(O.AggressiveInlining)] public static bool StartDownload_IsAllowed ( this DownloadStatus status ) => (status == DownloadStatus.Created ) ||
                                                                                                               (status == DownloadStatus.Paused  ) ||
                                                                                                               (status == DownloadStatus.Canceled) ||
                                                                                                               (status == DownloadStatus.Finished) ||
                                                                                                               (status == DownloadStatus.Error   );
        [M(O.AggressiveInlining)] public static bool CancelDownload_IsAllowed( this DownloadStatus status ) => (status == DownloadStatus.Started ) ||
                                                                                                               (status == DownloadStatus.Running ) ||
                                                                                                               (status == DownloadStatus.Wait    ) ||
                                                                                                               (status == DownloadStatus.Paused  );
        [M(O.AggressiveInlining)] public static bool PauseDownload_IsAllowed ( this DownloadStatus status ) => (status == DownloadStatus.Started ) ||
                                                                                                               (status == DownloadStatus.Running );
        #endregion

        public static string GetSpeedText( long downloadBytes, double elapsedSeconds, double? instantSpeedInMbps )
        {
            string speedText;
            //if ( downloadBytes < 1_024 ) speedText = GetSpeedInBps( downloadBytes, elapsedSeconds ).ToString("N2") + " bps"; //" bit/s";
            if ( downloadBytes < 100_024 ) speedText = GetSpeedInKbps( downloadBytes, elapsedSeconds ).ToString("N2") + " Kbps"; //" Kbit/s";
            else                           speedText = GetSpeedInMbps( downloadBytes, elapsedSeconds ).ToString("N1") + " Mbps"; //" Mbit/s";
            if ( instantSpeedInMbps.HasValue )
            {
                speedText += $" (↑{instantSpeedInMbps:N1} Mbps)";
            }
            return (speedText);
        }
        public static double GetMbps( long downloadBytes ) => (downloadBytes * 1.0 / (1_048_576 / 8));
        public static double GetSpeedInMbps( long downloadBytes, double elapsedSeconds ) => (8 * (downloadBytes / elapsedSeconds) / 1_048_576); //" Mbps"; //" Mbit/s";
        public static double GetSpeedInKbps( long downloadBytes, double elapsedSeconds ) => (8 * (downloadBytes / elapsedSeconds) / 1_024); //" Kbps"; //" Kbit/s";
        public static double GetSpeedInBps( long downloadBytes, double elapsedSeconds ) => (8 * (downloadBytes / elapsedSeconds)); //" bps"; //" bit/s";
        public static string GetSizeFormatted( long sizeInBytes )
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
            return (sizeInBytes.ToString("#,#"/*"N0"*/) + " bytes");
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
            const string HH_MM_SS = "hh\\:mm\\:ss";
            const string MM_SS    = "mm\\:ss";

            if ( 1 < ts.TotalHours   ) return (ts.ToString( HH_MM_SS ));
            if ( 1 < ts.TotalSeconds ) return (':' + ts.ToString( MM_SS ));
            return (ts.ToString());
        }

        [M(O.AggressiveInlining)] public static ConfiguredTaskAwaitable< T > CAX< T >( this Task< T > task ) => task.ConfigureAwait( false );
        [M(O.AggressiveInlining)] public static ConfiguredTaskAwaitable CAX( this Task task ) => task.ConfigureAwait( false );

        [M(O.AggressiveInlining)] public static void Invoke( this SynchronizationContext ctx, Action action ) => ctx.Send( _ => action(), null );

        [M(O.AggressiveInlining)] public static T Find_Ex< T >( this Window window, string name ) where T : class => window.Find< T >( name ) ?? (window.TryFindResource( name, out var x ) ? (T) x : null);       
        [M(O.AggressiveInlining)] public static T Find_Ex< T >( this UserControl uc, string name ) where T : class => uc.Find< T >( name ) ?? (uc.TryFindResource( name, out var x ) ? (T) x : null);        
        [M(O.AggressiveInlining)] public static MenuItem Find_MenuItem( this ContextMenu contextMenu, string name ) => contextMenu.Items.Cast< MenuItem >().First( m => m.Name == name );
    }
}
