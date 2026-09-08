using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;

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
            var lst = new List< char >( char.MinValue + char.MaxValue );
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

        public static IReadOnlyCollection< string > ResetExcludesWords( IEnumerable< string > excludesWords )
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
                return (EXCLUDES_WORDS_SET);
            }
        }
        public static IReadOnlyCollection< string > ExcludesWords => EXCLUDES_WORDS_SET;

        public static string Clean( string name, char wordsSeparator = '.' )
        {
            if ( name.IsNullOrEmpty() )
            {
                return (name);
            }

            var array = name.Split( PUNCTUATION_CHARS, StringSplitOptions.RemoveEmptyEntries );

            #region [.phase #1.]
            var firstWholeNumbersWord = default(string);
            var words = new List< (string word, bool isOnlyLetters) >( array.Length );
            foreach ( var s in array )
            {
                if ( EXCLUDES_WORDS_SET.Contains( s ) )
                {
                    continue;
                }
                if ( int.TryParse( s, out var _ ) )
                {
                    if ( firstWholeNumbersWord == null )
                    {
                        firstWholeNumbersWord = s;
                    }
                    continue;
                }

                words.Add( (s, s.IsOnlyLetters()) );
            }
            if ( !words.Any() )
            {
                var s = (firstWholeNumbersWord ?? "---");
                words.Add( (s, s.IsOnlyLetters()) );
            }
            #endregion

            #region [.phase #2.]
            const char SPACE = ' ';

            var sb = new StringBuilder( name.Length );
            var prev_word_isOnlyLetters = true;
            foreach ( var t in words )
            {
                if ( t.word.TryParseSE( out var se ) )
                {
                    if ( !sb.IsEmpty() )
                    {
                        sb.ReplaceLastCharIfNotEqual( wordsSeparator );
                    }
                    sb.Append( se ).Append( wordsSeparator );
                    break;
                }

                if ( prev_word_isOnlyLetters ) //&& t.isOnlyLetters )
                {
                    sb.AppendFirstCharToUpper( t.word ).Append( (t.isOnlyLetters ? SPACE : wordsSeparator) );
                }
                else
                {
                    sb.Append( t.word ).Append( wordsSeparator );
                }
                prev_word_isOnlyLetters = t.isOnlyLetters;
            }
            sb.RemoveLastChar();
            #endregion

            return (sb.ToString());
        }

        [M(O.AggressiveInlining)] private static bool IsEmpty( this StringBuilder sb ) => (sb.Length == 0);
        [M(O.AggressiveInlining)] private static StringBuilder RemoveLastChar( this StringBuilder sb ) => sb.Remove( sb.Length - 1, 1 );
        [M(O.AggressiveInlining)] private static void ReplaceLastCharIfNotEqual( this StringBuilder sb, char ch )
        {
            if ( sb[ sb.Length - 1 ] != ch )
            {
                sb.RemoveLastChar().Append( ch );
            }
        }
        [M(O.AggressiveInlining)] private static StringBuilder AppendFirstCharToUpper( this StringBuilder sb, string s ) 
            => sb.Append( char.ToUpperInvariant( s[ 0 ] ) ).Append( s, 1, s.Length - 1 );
        [M(O.AggressiveInlining)] private static bool TryParseSE( this string v, out string se )
        {
            const int SE_LENGTH = 6; // => "s01e01"

            if ( (v.Length == SE_LENGTH) && 
                 (char.ToLowerInvariant( v[ 0 ] ) == 's') && char.IsDigit( v, 1 ) && char.IsDigit( v, 2 ) &&
                 (char.ToLowerInvariant( v[ 3 ] ) == 'e') && char.IsDigit( v, 4 ) && char.IsDigit( v, 5 )
               )
            {
                se = 's' + v.Substring( 1, 2 ) + "-e" + v.Substring( 4 );
                return (true);
            }
            se = v;
            return (false);
        }
        [M(O.AggressiveInlining)] private static bool IsOnlyLetters( this string s ) => s.All( ch => char.IsLetter( ch ) );
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
            , bool   trimStartDashes  = true
            , char?  skipChar         = null )
        {
            if ( pathnameAndFilename != null )
            {
                var buf = new StringBuilder( pathnameAndFilename.Length + 10 );
                for ( var i = 0; i < pathnameAndFilename.Length; i++ )
                {
                    var ch = pathnameAndFilename[ i ];
                    if ( _InvalidPathChars.Contains( ch ) )
                    {
                        buf.Append( (skipChar.HasValue && (ch == skipChar.Value)) ? ch : replacedPathChar );
                    }
                    else if ( _InvalidFileNameChars.Contains( ch ) )
                    {
                        switch ( ch )
                        {
                            case '/':
                            case '\\':
                                buf.Append( (skipChar.HasValue && (ch == skipChar.Value)) ? ch : replacedPathChar );
                                //buf.Append( replacedPathChar );
                            break;

                            default:
                                buf.Append( (skipChar.HasValue && (ch == skipChar.Value)) ? ch : replacedNameChar );
                            break;
                        }
                    }
                    else
                    {
                        buf.Append( ch );
                    }
                }
                pathnameAndFilename = buf.ToString();
                if ( trimStartDashes )
                {
                    pathnameAndFilename = pathnameAndFilename.TrimStart( '-' );
                }
            }
            return (pathnameAndFilename);
        }
    }
}
