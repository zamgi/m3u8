using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using MessageBox.Avalonia;
using MessageBox.Avalonia.BaseWindows;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;

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
        [M(O.AggressiveInlining)] public static bool HasFirstCharNotDot( this string s ) => ((0 < (s?.Length).GetValueOrDefault()) && (s[ 0 ] != '.'));
        [M(O.AggressiveInlining)] public static bool AnyEx< T >( this IEnumerable< T > seq ) => (seq != null && seq.Any());
        [M(O.AggressiveInlining)] public static T? Try2Enum< T >( this string s ) where T : struct => (Enum.TryParse< T >( s, true, out var t ) ? t : (T?) null);
        [M(O.AggressiveInlining)] public static bool EqualIgnoreCase( this string s1, string s2 ) => (string.Compare( s1, s2, true ) == 0);
        [M(O.AggressiveInlining)] public static bool ContainsIgnoreCase( this string s1, string s2 ) => ((s1 != null) && s1.Contains( s2, StringComparison.InvariantCultureIgnoreCase ));

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
            settings.NameCleanerExcludesWords.AddRange( excludesWords.ToArray() );
        }
        public static string GetDownloadListColumnsWidthJson( this SettingsPropertyChangeController settingsController ) => settingsController.Settings.DownloadListColumnsWidthJson;
        public static void SetDownloadListColumnsWidthJson( this SettingsPropertyChangeController settingsController, string json ) => settingsController.Settings.DownloadListColumnsWidthJson = json;

        public static void DeleteFiles_NoThrow( string[] fileNames )
        {
            if ( fileNames.AnyEx() )
            {
                foreach ( var fileName in fileNames )
                {
                    DeleteFile_NoThrow( fileName );
                }
            }
        }
        public static void DeleteFile_NoThrow( string fileName )
        {
            try
            {
                File.Delete( fileName );
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );
            }
        }
        public static bool TryGetFirstFileExists( ICollection< string > fileNames, out string existsFileName )
        {
            if ( fileNames.AnyEx() )
            {
                foreach ( var fileName in fileNames )
                {
                    if ( (fileName != null) && File.Exists( fileName ) )
                    {
                        existsFileName = fileName;
                        return (true);
                    }
                }
            }
            existsFileName = null;
            return (false);
        }
        public static bool AnyFileExists( ICollection< string > fileNames ) => TryGetFirstFileExists( fileNames, out var _ );

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

        public static void RemoveAll( this DownloadListModel model, IEnumerable< DownloadRow > rows )
        {
            var array = rows?.ToArray();
            if ( array.AnyEx() )
            {
                model.BeginUpdate();
                foreach ( var row in array )
                {
                    model.RemoveRow( row );
                }
                model.EndUpdate();
            }
        }
        public static void RemoveAllFinished( this DownloadListModel model )
        {
            var allFinished = model.GetAllFinished();
            model.RemoveAll( allFinished );
        }

        [M(O.AggressiveInlining)] public static bool IsFinished( this DownloadRow    row    ) => (row.Status == DownloadStatus.Finished);
        [M(O.AggressiveInlining)] public static bool IsFinished( this DownloadStatus status ) => (status     == DownloadStatus.Finished);
        [M(O.AggressiveInlining)] public static bool IsError   ( this DownloadRow    row    ) => (row.Status == DownloadStatus.Error);
        [M(O.AggressiveInlining)] public static bool IsRunning ( this DownloadRow    row    ) => (row.Status == DownloadStatus.Running);
        [M(O.AggressiveInlining)] public static bool IsWait    ( this DownloadRow    row    ) => (row.Status == DownloadStatus.Wait);
        [M(O.AggressiveInlining)] public static bool IsPaused  ( this DownloadRow    row    ) => (row.Status == DownloadStatus.Paused);
        [M(O.AggressiveInlining)] public static bool IsPaused  ( this DownloadStatus status ) => (status     == DownloadStatus.Paused);

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
                disposable.Dispose();
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
        private static IMsBoxWindow< ButtonResult > Create_MsBoxStandardWindow( string text, string caption, ButtonEnum buttons, Icon icon, bool closeByEscape = true )
            => Create_MsBoxStandardWindow( text, caption, buttons, icon, out var _, closeByEscape );
        public static IMsBoxWindow< ButtonResult > Create_MsBoxStandardWindow( string text, string caption, ButtonEnum buttons, Icon icon
            , out MessageBox.Avalonia.Views.MsBoxStandardWindow window, bool closeByEscape = true )
        {
            var p = new MessageBoxStandardParams()
            {
                ButtonDefinitions = buttons,
                Icon              = icon,
                ContentTitle      = caption,
                ContentMessage    = text,
            };
            var msgbox = MessageBoxManager.GetMessageBoxStandardWindow( p );
            p.Window.Icon = new WindowIcon( ResourceLoader._GetResource_( "/Resources/m3u8_32x36.ico" ) );
            if ( closeByEscape )
            {
                p.Window.KeyDown += (s, e) =>
                {
                    if ( e.Key == Key.Escape )
                    {
                        ((Window) s).Close();
                    }
                };
            }
            window = p.Window;
            return (msgbox);
        }
        public static async Task< ButtonResult > ShowEx( this IMsBoxWindow< ButtonResult > msgbox )
        {
            var window = GetTopWindow();
            if ( window != null )
            {
                return (await msgbox.ShowDialog( window ));
            }
            else
            {
                return (await msgbox.Show());
            }
        }

        public static Task MessageBox_ShowInformation( string text, string caption ) => MessageBox_ShowWithOkButton( text, caption, Icon.Info );
        public static Task MessageBox_ShowError( string text, string caption ) => MessageBox_ShowWithOkButton( text, caption, Icon.Error );
        public static Task MessageBox_ShowError( this Exception ex, string caption ) => MessageBox_ShowError( ex.ToString(), caption );
        public static Task MessageBox_ShowInformation( this Window window, string text, string caption ) => Create_MsBoxStandardWindow( text, caption, ButtonEnum.Ok, Icon.Info ).ShowDialog( window );
        public static Task MessageBox_ShowError( this Window window, string text, string caption ) => Create_MsBoxStandardWindow( text, caption, ButtonEnum.Ok, Icon.Error ).ShowDialog( window );
        public static Task< ButtonResult > MessageBox_ShowQuestion( this Window window, string text, string caption, ButtonEnum buttons = ButtonEnum.YesNo ) => Create_MsBoxStandardWindow( text, caption, buttons, Icon.Info, false ).ShowDialog( window );
        private static async Task MessageBox_ShowWithOkButton( string text, string caption, Icon icon )
        {
            var msgbox = Create_MsBoxStandardWindow( text, caption, ButtonEnum.Ok, icon );
            var window = GetTopWindow();
            if ( window != null )
            {
                await msgbox.ShowDialog( window );
            }
            else
            {
                await msgbox.Show();
            }
        }       
        #endregion

        public static async Task< (bool success, IReadOnlyCollection< string > m3u8FileUrls) > TryGetM3u8FileUrlsFromClipboard()
        {
            try
            {
                var text = (await Application.Current.Clipboard.GetTextAsync())?.Trim();
                if ( !text.IsNullOrEmpty() )
                {
                    var array = text.Split( new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries );
                    var hs  = new HashSet< string >( StringComparer.InvariantCultureIgnoreCase );
                    var m3u8FileUrls = new List< string >( array.Length );
                    foreach ( var a in array )
                    {
                        if ( a.EndsWith( Resources.M3U8_EXTENSION, StringComparison.InvariantCultureIgnoreCase ) && hs.Add( a ) )
                        {
                            m3u8FileUrls.Add( a );
                        }
                        else
                        {
                            var i = a.IndexOf( Resources.M3U8_EXTENSION + '?', StringComparison.InvariantCultureIgnoreCase );
                            if ( (10 < i) && hs.Add( a ) )
                            {
                                m3u8FileUrls.Add( a );
                            }
                        }
                    }
                    return (m3u8FileUrls.Any(), m3u8FileUrls);
                }
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );
            }

            return (false, default);
        }
        public static async Task< IReadOnlyCollection< string > > TryGetM3u8FileUrlsFromClipboardOrDefault()
        {
            var t = await TryGetM3u8FileUrlsFromClipboard();
            return (t.success ? t.m3u8FileUrls : new string[ 0 ]);
        }
        public static Task CopyM3u8FileUrlToClipboard( string m3u8FileUrl ) => Application.Current.Clipboard.SetTextAsync( m3u8FileUrl );

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
    }
}
