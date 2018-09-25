using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace m3u8.downloader
{
    /// <summary>
    /// 
    /// </summary>
    internal static class Extensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty( this string s ) => string.IsNullOrEmpty( s );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrWhiteSpace( this string s ) => string.IsNullOrWhiteSpace( s );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasFirstCharNotDot( this string s ) => ((0 < (s?.Length).GetValueOrDefault()) && (s[ 0 ] != '.'));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AnyEx< T >( this IEnumerable< T > seq ) => (seq != null && seq.Any());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T? Try2Enum< T >( this string s ) where T : struct => (Enum.TryParse< T >( s, true, out var t ) ? t : (T?) null);

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

        /*[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetDoubleBuffered( this Control control, bool value ) =>
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendException( this TextBox textBox, Exception ex )
        {
            textBox.AppendText( "\r\n FAILED-------------------------------------------------------------------------------------------------------------------------FAILED \r\n" );
            textBox.AppendText( ex.ToString().Trim( '\r', '\n' ) );
            textBox.AppendText( "\r\n FAILED-------------------------------------------------------------------------------------------------------------------------FAILED \r\n" );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendEmptyLine( this TextBox textBox ) => textBox.AppendText( Environment.NewLine );
    }
}
