using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace m3u8.downloader
{
    /// <summary>
    /// 
    /// </summary>
    internal static class NameCleaner
    {
        private static char[] PUNCTUATION_CHARS;
        private static SortedSet< string > EXCLUDES_WORDS_SET;
        //private static string[] EXCLUDES_WORDS = { "video", "hls", "WEB", "DL", "1O8Op", "720", "playlist", "m3u8" };

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
}
