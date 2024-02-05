using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using m3u8.download.manager.controllers;
using m3u8.download.manager.models;
using m3u8.download.manager.Properties;

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
        [M(O.AggressiveInlining)] public static bool AnyEx< T >( this IEnumerable< T > seq ) => (seq != null) && seq.Any();
        [M(O.AggressiveInlining)] public static bool AnyEx< T >( this IList< T > seq ) => (seq != null) && (0 < seq.Count);
        [M(O.AggressiveInlining)] public static bool AnyEx< T >( this ICollection< T > seq ) => (seq != null) && (0 < seq.Count);
        [M(O.AggressiveInlining)] public static bool AnyEx< T >( this T[] seq ) => (seq != null) && (0 < seq.Length);
        [M(O.AggressiveInlining)] public static bool AnyEx_< T >( this IReadOnlyList< T > seq ) => (seq != null) && (0 < seq.Count);
        [M(O.AggressiveInlining)] public static bool AnyEx_< T >( this IReadOnlyCollection< T > seq ) => (seq != null) && (0 < seq.Count);
        [M(O.AggressiveInlining)] public static T? Try2Enum< T >( this string s ) where T : struct => (Enum.TryParse< T >( s, true, out var t ) ? t : null);
        [M(O.AggressiveInlining)] public static bool EqualIgnoreCase( this string s1, string s2 ) => (string.Compare( s1, s2, true ) == 0);
        [M(O.AggressiveInlining)] public static bool ContainsIgnoreCase( this string s1, string s2 ) => ((s1 != null) && (s1.IndexOf( s2, StringComparison.InvariantCultureIgnoreCase ) != -1));
        public static void Remove< T >( this HashSet< T > hs, IEnumerable< T > seq )
        {
            if ( seq != null )
            {
                foreach ( var t in seq )
                {
                    hs.Remove( t );
                }
            }
        }
        public static void Add< T >( this HashSet< T > hs, IEnumerable< T > seq )
        {
            if ( seq != null )
            {
                foreach ( var t in seq )
                {
                    hs.Add( t );
                }
            }
        }
        public static void RemoveIf< T >( this HashSet< T > hs, T t )
        {
            if ( t != null )
            {
                hs.Remove( t );
            }
        }
        public static void AddIf< T >( this HashSet< T > hs, T t )
        {
            if ( t != null )
            {
                hs.Add( t );
            }
        }

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
        //public static T[] ToArray< T >( this IEnumerable< T > seq, int size )
        //{
        //    var array = new T[ size ];
        //    var i = 0;
        //    foreach ( var t in seq )
        //    {
        //        array[ i++ ] = t;
        //    }
        //    return (array);
        //}
        public static void Replace< T >( this List< T > lst, IEnumerable< T > seq )
        {
            lst.Clear();
            if ( seq != null )
            {
                lst.AddRange( seq );
            }
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
        public static string GetFirstExistsDirectory( string path )
        {
            for ( var dir = path; !dir.IsNullOrEmpty(); dir = Path.GetDirectoryName( dir ) )
            {
                if ( Directory.Exists( dir ) )
                {
                    return (dir);
                }
            }

            return (null);
        }
        public static (bool success, string outputFileName) TryGetFirstFileExists( ICollection<string> fileNames ) => (TryGetFirstFileExists( fileNames, out var outputFileName ), outputFileName);
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
        //public static bool TryGetFirstFileExistsNonZeroLength( ICollection< string > fileNames, out string existsFileName )
        //{
        //    if ( fileNames.AnyEx() )
        //    {
        //        foreach ( var fileName in fileNames )
        //        {
        //            if ( (fileName != null) && File.Exists( fileName ) && (0 < new FileInfo( fileName ).Length) )
        //            {
        //                existsFileName = fileName;
        //                return (true);
        //            }
        //        }
        //    }
        //    existsFileName = null;
        //    return (false);
        //}
        public static bool AnyFileExists( ICollection< string > fileNames ) => TryGetFirstFileExists( fileNames, out var _ );

        public static void MessageBox_ShowInformation( this IWin32Window owner, string text, string caption ) => MessageBox.Show( owner, text, caption, MessageBoxButtons.OK, MessageBoxIcon.Information );
        public static void MessageBox_ShowError( this IWin32Window owner, string text, string caption ) => MessageBox.Show( owner, text, caption, MessageBoxButtons.OK, MessageBoxIcon.Error );
        public static void MessageBox_ShowError( this Exception ex, string caption ) => MessageBox_ShowError( ex.ToString(), caption );
        public static void MessageBox_ShowError( string text, string caption )
        {
            var form = Application.OpenForms.Cast< Form >().FirstOrDefault();
            if ( (form != null) && !form.IsDisposed && form.IsHandleCreated )
            {
                form.MessageBox_ShowError( text, caption );
            }
            else
            {
                MessageBox.Show( text, caption, MessageBoxButtons.OK, MessageBoxIcon.Error );
            }            
        }
        public static DialogResult MessageBox_ShowQuestion( this IWin32Window owner, string text, string caption
            , MessageBoxButtons buttons = MessageBoxButtons.YesNo, MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button1 )
            => MessageBox.Show( owner, text, caption, buttons, MessageBoxIcon.Question, defaultButton );

        public static void SetEnabledAllChildControls( this Control parentControl, bool enabled ) => parentControl.Controls.Cast< Control >().ToList().ForEach( c => c.Enabled = enabled );
        public static bool IsSelected( this TabPage tabPage )
        {
            var tabControl = (TabControl) tabPage.Parent;
            if ( tabControl != null )
            {
                var selIdx = tabControl.SelectedIndex;
                if ( selIdx != -1 )
                {
                    return (tabControl.TabPages[ selIdx ] == tabPage);
                }
            }
            return (false);
        }

        [M(O.AggressiveInlining)] public static string TrimIfLongest( this string s, int maxLength ) => ((maxLength < s.Length) ? (s.Substring( 0, maxLength ) + "..." ) : s);

        [M(O.AggressiveInlining)] public static void SetRowInvisible( this DataGridView DGV, int index )
        {
#if DEBUG
            if ( (index < 0) || (DGV.RowCount <= index) )
            {
                return;
            }
#endif
            try
            {
                DGV.Rows[ index ].Visible = false;
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );
            }
        }
        [M(O.AggressiveInlining)] public static void ClearHeaderSortGlyphDirection( this DataGridView DGV ) => DGV.SetHeaderSortGlyphDirection( -1, SortOrder.None );
        [M(O.AggressiveInlining)] public static void SetHeaderSortGlyphDirection( this DataGridView DGV, int columnIndex, SortOrder sortOrder )
        {
            foreach ( var c in DGV.Columns.Cast< DataGridViewColumn >() )
            {
                c.HeaderCell.SortGlyphDirection = ((c.Index == columnIndex) ? sortOrder : SortOrder.None);
            }
        }
        [M(O.AggressiveInlining)] public static void SetFirstDisplayedScrollingRowIndex( this DataGridView DGV, int rowIndex )
        {
            try
            {
                DGV.FirstDisplayedScrollingRowIndex = rowIndex;
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );
            }
        }
        [M(O.AggressiveInlining)] public static int GetFirstDisplayedScrollingRowIndex( this DataGridView DGV, int defVal = 0 )
        {
            try
            {
                return (DGV.FirstDisplayedScrollingRowIndex);
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );
                return (defVal);
            }
        }
        [M(O.AggressiveInlining)] public static void SetHandCursorIfNonHand( this DataGridView DGV )
        {
            if ( DGV.Cursor != Cursors.Hand )
            {
                DGV.Cursor = Cursors.Hand;
            }
        }
        [M(O.AggressiveInlining)] public static void SetDefaultCursorIfHand( this DataGridView DGV )
        {
            if ( DGV.Cursor == Cursors.Hand )
            {
                DGV.Cursor = Cursors.Default;
            }
        }
        [M(O.AggressiveInlining)] public static bool IsSelected( this DataGridViewElementStates state ) => ((state & DataGridViewElementStates.Selected) == DataGridViewElementStates.Selected);
        public static void SetForeColor4ParentOnly< T >( this Control parent, Color foreColor ) where T : Control
        {
            if ( parent is T )
            {
                var fc = parent.ForeColor;
                parent.ForeColor = foreColor;
                foreach ( var child in parent.Controls.Cast< Control >() )
                {
                    child.ForeColor = fc;
                }
            }
            foreach ( var child in parent.Controls.Cast< Control >() )
            {
                SetForeColor4ParentOnly< T >( child, foreColor );
            }
        }

        public static bool TryGetM3u8FileUrlsFromClipboard( out IReadOnlyCollection< string > m3u8FileUrls )
        {
            var M3U8_EXTENSION_Q = Resources.M3U8_EXTENSION + '?';
            try
            {
                var text = Clipboard.GetText( TextDataFormat.Text )?.Trim();
                if ( text.IsNullOrEmpty() )
                {
                    text = Clipboard.GetText( TextDataFormat.UnicodeText )?.Trim();
                }
                if ( !text.IsNullOrEmpty() )
                {
                    var array = text.Split( new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries );
                    var hs  = new HashSet< string >( StringComparer.InvariantCultureIgnoreCase );
                    var lst = new List< string >( array.Length );
                    foreach ( var a in array )
                    {
                        var u = a.Trim();
                        if ( u.EndsWith( Resources.M3U8_EXTENSION, StringComparison.InvariantCultureIgnoreCase ) && hs.Add( u ) )
                        {
                            lst.Add( u );
                        }
                        else
                        {
                            var i = u.IndexOf( M3U8_EXTENSION_Q, StringComparison.InvariantCultureIgnoreCase );
                            if ( (10 < i) && hs.Add( u ) )
                            {
                                lst.Add( u );
                            }
                        }
                    }
                    m3u8FileUrls = lst;
                    return (m3u8FileUrls.Any());
                }
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );
            }

            m3u8FileUrls = default;
            return (false);
        }
        public static bool TryGetHttpUrlsFromClipboard( out IReadOnlyCollection< string > urls )
        {
            const string HTTP  = "http://";
            const string HTTPS = "https://";
            try
            {
                var text = Clipboard.GetText( TextDataFormat.Text )?.Trim();
                if ( text.IsNullOrEmpty() ) text = Clipboard.GetText( TextDataFormat.UnicodeText )?.Trim();

                if ( !text.IsNullOrEmpty() )
                {
                    var array = text.Split( new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries );
                    var hs  = new HashSet< string >( StringComparer.InvariantCultureIgnoreCase );
                    var lst = new List< string >( array.Length );
                    foreach ( var a in array )
                    {
                        var u = a.Trim();
                        if ( (u.StartsWith( HTTP , StringComparison.InvariantCultureIgnoreCase ) ||
                              u.StartsWith( HTTPS, StringComparison.InvariantCultureIgnoreCase )) && hs.Add( u ) )
                        {
                            lst.Add( u );
                        }
                    }
                    urls = lst;
                    return (urls.Any());
                }
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );
            }

            urls = default;
            return (false);
        }
        public static IReadOnlyCollection< string > TryGetM3u8FileUrlsFromClipboardOrDefault() => (TryGetM3u8FileUrlsFromClipboard( out var m3u8FileUrls ) ? m3u8FileUrls : new string[ 0 ]);
        public static void CopyUrlsToClipboard( IEnumerable< DownloadRow > rows ) => Clipboard.SetText( string.Join( "\r\n", rows.Select( r => r.Url ) ), TextDataFormat.UnicodeText );

        public static string ToJSON< T >( this T t )
        {
            var ser = new DataContractJsonSerializer( typeof(T) );

            using ( var ms = new MemoryStream() )
            {                
                ser.WriteObject( ms, t );
                var json = Encoding.UTF8.GetString( ms.GetBuffer(), 0, (int) ms.Position );
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

        public static bool TryGetData< T >( this IDataObject d, out T t )
        {
            if ( d.GetData( typeof(T) ) is T x )
            {
                t = x;
                return (true);
            }
            t = default;
            return (false);
        }
        public static void SetDataEx< T >( this IDataObject d, T t ) => d.SetData( typeof(T), t );
        //public static T GetData< T >( this IDataObject d ) => (d.GetData( typeof(T) ) is T t) ? t : default;

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

        public static void CancelAll( this DownloadController controller, IEnumerable< DownloadRow > rows )
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
        [M(O.AggressiveInlining)] public static bool IsRunningOrPaused( this DownloadStatus status ) => status switch { DownloadStatus.Running => true, DownloadStatus.Paused => true, _ => false };

        [M(O.AggressiveInlining)] public static bool IsColumnSortable( this DataGridView dgv, int columnIndex )
            => /*(0 <= columnIndex) && */ (columnIndex < 0) || (dgv.Columns[ columnIndex ].SortMode != DataGridViewColumnSortMode.NotSortable);

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

        public static void SetDoubleBuffered< T >( this T t, bool doubleBuffered ) where T : Control
        {          
            //Control.DoubleBuffered = true;
            var field = typeof(T).GetProperty( "DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance );
            field?.SetValue( t, doubleBuffered );
        }

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
#if NETCOREAPP
        [M(O.AggressiveInlining)] public static ConfiguredValueTaskAwaitable< T > CAX< T >( this in ValueTask< T > task ) => task.ConfigureAwait( false );
        [M(O.AggressiveInlining)] public static ConfiguredValueTaskAwaitable CAX( this in ValueTask task ) => task.ConfigureAwait( false );
#endif
        [M(O.AggressiveInlining)] public static void Invoke( this SynchronizationContext ctx, Action action ) => ctx.Send( _ => action(), null );
    }
}
