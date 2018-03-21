using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace m3u8.downloader
{
    /// <summary>
    /// 
    /// </summary>
    internal static class Extensions
    {
        public static bool IsNullOrEmpty( this string s )
        {
            return (string.IsNullOrEmpty( s ));
        }
        public static bool IsNullOrWhiteSpace( this string s )
        {
            return (string.IsNullOrWhiteSpace( s ));
        }

        public static bool HasFirstCharNotDot( this string s )
        {
            return ((0 < (s?.Length).GetValueOrDefault()) && (s[ 0 ] != '.'));
        }

        public static bool AnyEx< T >( this IEnumerable< T > seq )
        {
            return (seq != null && seq.Any());
        }

        public static T? Try2Enum< T >( this string s ) 
            where T : struct
        {
            T t;
            if ( Enum.TryParse< T >( s, true, out t ) )
            {
                return (t);
            }
            return (null);
        }

        public static Exception ShellExploreAndSelectFile( string filePath )
        {
            try
            {
                var fileLocation = Path.GetFullPath( filePath );
                Process.Start( "explorer", "/e,/select,\"" + fileLocation + "\"" );
                return (null);
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

        public static void SetDoubleBuffered( this Control control, bool value )
        {
            typeof(Control).GetProperty( "DoubleBuffered", BindingFlags.NonPublic | BindingFlags.SetProperty | BindingFlags.Instance )?.SetValue( control, value );
        }
    }
}
