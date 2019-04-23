using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using m3u8.Properties;
using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;

namespace m3u8.downloader
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

        /*public static Exception ShellExploreAndSelectFile( string filePath )
        {
            try
            {
                var fileLocation = Path.GetFullPath( filePath );
                using ( Process.Start( "explorer", $"/e,/select,\"{fileLocation}\"" ) )
                {
                    return (null);
                }
            }
            catch ( Exception ex )
            {
                return (ex);
            }
        }*/
        /*[M(O.AggressiveInlining)] public static void SetDoubleBuffered( this Control control, bool value = true ) =>
            typeof(Control).GetProperty( "DoubleBuffered", BindingFlags.NonPublic | BindingFlags.SetProperty | BindingFlags.Instance )?.SetValue( control, value );*/

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

        public static void MessageBox_ShowInformation( this IWin32Window owner, string text, string caption ) => MessageBox.Show( owner, text, caption, MessageBoxButtons.OK, MessageBoxIcon.Information );
        public static void MessageBox_ShowError( this IWin32Window owner, string text, string caption ) => MessageBox.Show( owner, text, caption, MessageBoxButtons.OK, MessageBoxIcon.Error );
        public static void MessageBox_ShowError( this Exception ex, string caption ) => MessageBox.Show( ex.ToString(), caption, MessageBoxButtons.OK, MessageBoxIcon.Error );
        public static void MessageBox_ShowError( string text, string caption ) => MessageBox.Show( text, caption, MessageBoxButtons.OK, MessageBoxIcon.Error );
        public static DialogResult MessageBox_ShowQuestion( this IWin32Window owner, string text, string caption
            , MessageBoxButtons buttons = MessageBoxButtons.YesNo, MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button1 )
            => MessageBox.Show( owner, text, caption, buttons, MessageBoxIcon.Question, defaultButton );

        [M(O.AggressiveInlining)] public static string TrimIfLongest( this string s, int maxLength ) => ((maxLength < s.Length) ? (s.Substring( 0, maxLength ) + "..." ) : s);

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
    }
}
