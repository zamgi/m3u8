using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Windows.Forms;

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
        [M(O.AggressiveInlining)] public static T? Try2Enum< T >( this string s ) where T : struct => (Enum.TryParse< T >( s, true, out var t ) ? t : (T?) null);
        [M(O.AggressiveInlining)] public static bool EqualIgnoreCase( this string s1, string s2 ) => (string.Compare( s1, s2, true ) == 0);
        [M(O.AggressiveInlining)] public static bool ContainsIgnoreCase( this string s1, string s2 ) => ((s1 != null) && (s1.IndexOf( s2, StringComparison.InvariantCultureIgnoreCase ) != -1));

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
        [M(O.AggressiveInlining)] public static void FirstDisplayedScrollingRowIndex( this DataGridView DGV, int rowIndex )
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

        public static bool TryGetM3u8FileUrlsFromClipboard( out IReadOnlyCollection< string > m3u8FileUrls )
        {
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
                        if ( a.EndsWith( Resources.M3U8_EXTENSION, StringComparison.InvariantCultureIgnoreCase ) && hs.Add( a ) )
                        {
                            lst.Add( a );
                        }
                        else
                        {
                            var i = a.IndexOf( Resources.M3U8_EXTENSION + '?', StringComparison.InvariantCultureIgnoreCase );
                            if ( (10 < i) && hs.Add( a ) )
                            {
                                lst.Add( a );
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
        public static IReadOnlyCollection< string > TryGetM3u8FileUrlsFromClipboardOrDefault() => (TryGetM3u8FileUrlsFromClipboard( out var m3u8FileUrls ) ? m3u8FileUrls : new string[ 0 ]);
        public static void CopyM3u8FileUrlToClipboard( string m3u8FileUrl ) => Clipboard.SetText( m3u8FileUrl, TextDataFormat.UnicodeText );

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
    }
}
