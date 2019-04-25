using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using m3u8.download.manager.Properties;

namespace m3u8.download.manager.infrastructure
{
    /// <summary>
    /// 
    /// </summary>
    internal static class NameCleaner
    {
        private static char[] PUNCTUATION_CHARS;
        private static SortedSet< string > EXCLUDES_WORDS_SET;

        static NameCleaner()
        {
            //-1-//
            var lst = new List< char >();
            for ( var c = char.MinValue; ; c++ )
            {
                if ( c == char.MaxValue )
                    break;

                if ( char.IsPunctuation( c ) )
                {
                    lst.Add( c );
                }
            }
            PUNCTUATION_CHARS = lst.ToArray();
            
            //-2-//
            EXCLUDES_WORDS_SET = new SortedSet< string >( StringComparer.InvariantCultureIgnoreCase );
        }

        public static void ResetExcludesWords( IEnumerable< string > excludesWords )
        {
            lock ( EXCLUDES_WORDS_SET )
            {
                EXCLUDES_WORDS_SET.Clear();
                foreach ( var w in (excludesWords ?? Enumerable.Empty< string >()) )
                {
                    var s = w?.Trim();
                    if ( !s.IsNullOrEmpty() )
                    {
                        EXCLUDES_WORDS_SET.Add( w );
                    }
                }
            }
        }
        public static IReadOnlyCollection< string > ExcludesWords => EXCLUDES_WORDS_SET;

        public static string Clean( string name, char wordsSeparator = '.' )
        {
            if ( name.IsNullOrEmpty() || !EXCLUDES_WORDS_SET.Any() )
            {
                return (name);
            }

            var array = name.Split( PUNCTUATION_CHARS, StringSplitOptions.RemoveEmptyEntries );
            var sb = new StringBuilder( name.Length );
            foreach ( var s in array )
            {
                if ( !EXCLUDES_WORDS_SET.Contains( s ) )
                {
                    sb.Append( s ).Append( wordsSeparator );
                }
            }
            if ( sb.Length != 0 )
            {
                sb.Remove( sb.Length - 1, 1 );
            }

            return (sb.ToString());
        }
    }

    /// <summary>
    /// 
    /// </summary>
    internal static class PathnameCleaner
    {
        private static HashSet< char > _InvalidFileNameChars;
        private static HashSet< char > _InvalidPathChars;

        static PathnameCleaner()
        {
            _InvalidFileNameChars = new HashSet< char >( Path.GetInvalidFileNameChars() );
            _InvalidPathChars     = new HashSet< char >( Path.GetInvalidPathChars    () );
        }

        public static string CleanFilename( string filename )
        {
            if ( filename != null )
            {
                filename = new string( (from ch in filename
                                        where (!_InvalidFileNameChars.Contains( ch ))
                                        select ch
                                       ).ToArray()
                                     );
            }
            return (filename);
        }
        public static string CleanPathname( string pathname )
        {
            if ( pathname != null )
            {
                pathname = new string( (from ch in pathname
                                       where (!_InvalidPathChars.Contains( ch ))
                                       select ch
                                      ).ToArray()
                                    );
            }
            return (pathname);
        }

        public static string CleanPathnameAndFilename( string pathnameAndFilename
            , string replacedPathChar = "--"
            , char   replacedNameChar = '-'
            , bool   trimStartDashes  = true )
        {            
            if ( pathnameAndFilename != null )
            {
                var sb = new StringBuilder( pathnameAndFilename.Length + 10 );
                for ( var i = 0; i < pathnameAndFilename.Length; i++ )
                {
                    var ch = pathnameAndFilename[ i ];
                    if ( _InvalidPathChars.Contains( ch ) )
                    {
                        sb.Append( replacedPathChar );
                    }
                    else if ( _InvalidFileNameChars.Contains( ch ) )
                    {
                        switch ( ch )
                        {
                            case '/':
                            case '\\':
                                sb.Append( replacedPathChar );
                            break;

                            default:
                                sb.Append( replacedNameChar );
                            break;
                        }                        
                    }
                    else
                    {
                        sb.Append( ch );
                    }
                }
                pathnameAndFilename = sb.ToString();
                if ( trimStartDashes )
                {
                    pathnameAndFilename = pathnameAndFilename.TrimStart( '-' );
                }
            }
            return (pathnameAndFilename);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    internal static class FileNameCleaner
    {
        private static Settings _Settings;

        static FileNameCleaner() => _Settings = Settings.Default;

        private static string AddOutputFileExtensionIfMissing( this string outputFileName )
        {
            if ( !outputFileName.IsNullOrEmpty() )
            {
                if ( !outputFileName.EndsWith( _Settings.OutputFileExtension, StringComparison.InvariantCultureIgnoreCase ) )
                {
                    if ( _Settings.OutputFileExtension.HasFirstCharNotDot() )
                    {
                        outputFileName += '.';
                    }
                    outputFileName += _Settings.OutputFileExtension;
                }
            }
            return (outputFileName);
        }
        private static string GetFileName_NoThrow( this string path )
        {
            try
            {
                return (Path.GetFileName( path ));
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );
            }
            return (null);
        }

        public static bool TryGetOutputFileNameByUrl( string m3u8FileUrlText, out string outputFileName )
        {
            try
            {
                var m3u8FileUrl = new Uri( m3u8FileUrlText );
                var inputOutputFileName = Uri.UnescapeDataString( m3u8FileUrl.AbsolutePath );

                var fn = PathnameCleaner.CleanPathnameAndFilename( inputOutputFileName );
                if ( !fn.IsNullOrWhiteSpace() )
                {
                    outputFileName = NameCleaner.Clean( fn )
                                                .AddOutputFileExtensionIfMissing();
                    return (!outputFileName.IsNullOrWhiteSpace());
                }
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );
            }

            outputFileName = default;
            return (false);
        }
        public static async Task< string > SetOutputFileNameByUrl_Async( string m3u8FileUrlText, Action< string > setOutputFileNameAction, int millisecondsDelay )
        {
            setOutputFileNameAction( null );
            try
            {                
                var m3u8FileUrl = new Uri( m3u8FileUrlText );
                var inputOutputFileName = Uri.UnescapeDataString( m3u8FileUrl.AbsolutePath );

                var fn = PathnameCleaner.CleanPathnameAndFilename( inputOutputFileName );
                if ( !fn.IsNullOrWhiteSpace() )
                {
                    setOutputFileNameAction( fn ); await Task.Delay( millisecondsDelay );

                    fn = NameCleaner.Clean( fn );
                    setOutputFileNameAction( fn ); await Task.Delay( millisecondsDelay );

                    fn = AddOutputFileExtensionIfMissing( fn );
                    setOutputFileNameAction( fn );

                    return (fn);
                }
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );
            }
            return (null);
        }

        public static string GetOutputFileName( string inputOutputFileName ) => (TryGetOutputFileName( inputOutputFileName, out string outputFileName ) ? outputFileName : null);
        public static bool TryGetOutputFileName( string inputOutputFileName, out string outputFileName )
        {
            var fn = PathnameCleaner.CleanPathnameAndFilename( inputOutputFileName );
            if ( !fn.IsNullOrWhiteSpace() )
            {
                outputFileName = fn.GetFileName_NoThrow()
                                   .AddOutputFileExtensionIfMissing();
                return (!outputFileName.IsNullOrWhiteSpace());
            }
            outputFileName = default;
            return (false);
        }
        public static async Task< string > SetOutputFileName_Async( string inputOutputFileName, Action< string > setOutputFileNameAction, int millisecondsDelay )
        {
            setOutputFileNameAction( null );

            var fn = PathnameCleaner.CleanPathnameAndFilename( inputOutputFileName );
            if ( !fn.IsNullOrWhiteSpace() )
            {
                setOutputFileNameAction( fn ); await Task.Delay( millisecondsDelay );

                fn = fn.GetFileName_NoThrow();
                setOutputFileNameAction( fn ); await Task.Delay( millisecondsDelay );

                fn = AddOutputFileExtensionIfMissing( fn );
                setOutputFileNameAction( fn );

                return (fn);
            }
            return (null);
        }
    }
}
