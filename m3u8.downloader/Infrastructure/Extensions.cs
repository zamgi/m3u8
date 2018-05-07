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
        public static T? Try2Enum< T >( this string s ) where T : struct => (Enum.TryParse<T>( s, true, out var t ) ? t : ((T?) null));

        public static Exception ShellExploreAndSelectFile( string filePath )
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

        public static void SetDoubleBuffered( this Control control, bool value ) =>
            typeof(Control).GetProperty( "DoubleBuffered", BindingFlags.NonPublic | BindingFlags.SetProperty | BindingFlags.Instance )?.SetValue( control, value );
    }
}
