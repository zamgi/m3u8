using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static string TrimIfLongest( this string s, int maxLength ) => ((maxLength < s.Length) ? (s.Substring( 0, maxLength ) + "..." ) : s);
    }
}
