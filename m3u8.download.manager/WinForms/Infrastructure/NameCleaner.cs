using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using m3u8.download.manager.Properties;
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
        /*public static async Task< string > SetOutputFileName_Async( string inputOutputFileName, Action< string > setOutputFileNameAction, int millisecondsDelay )
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
        }*/
        /*public static async Task< string > SetOutputFileName_Async(
            TextBox textBox, string inputOutputFileName, Action< string > setOutputFileNameAction, int millisecondsDelay )
        {
            var selectionStart = textBox.SelectionStart;
            setOutputFileNameAction( null );

            var fn = PathnameCleaner.CleanPathnameAndFilename( inputOutputFileName );
            if ( !fn.IsNullOrWhiteSpace() )
            {
                setOutputFileNameAction( fn );
                    textBox.SelectionStart = Math.Min( selectionStart, fn.Length );
                    selectionStart = textBox.SelectionStart;
                await Task.Delay( millisecondsDelay );

                fn = fn.GetFileName_NoThrow();
                setOutputFileNameAction( fn ); 
                    textBox.SelectionStart = Math.Min( selectionStart, fn.Length );
                    selectionStart = textBox.SelectionStart;
                await Task.Delay( millisecondsDelay );

                fn = AddOutputFileExtensionIfMissing( fn );
                setOutputFileNameAction( fn );
                    textBox.SelectionStart = Math.Min( selectionStart, fn.Length );

                return (fn);
            }
            return (null);
        }*/

        /// <summary>
        /// 
        /// </summary>
        public sealed class Processor : IDisposable
        {
            private TextBox _FileNameTextBox;
            private Func< string >   _GetFileNameAction;
            private Action< string > _SetFileNameAction;

            public Processor( TextBox fileNameTextBox
                , Func< string >   getFileNameAction
                , Action< string > setFileNameAction )
            {
                _FileNameTextBox   = fileNameTextBox;
                _GetFileNameAction = getFileNameAction;
                _SetFileNameAction = setFileNameAction;
            }
            public void Dispose() => CancelAndDispose_Cts_FileNameTextBox_TextChanged();

            private CancellationTokenSource _Cts_FileNameTextBox_TextChanged;
            private void CancelAndDispose_Cts_FileNameTextBox_TextChanged()
            {
                if ( _Cts_FileNameTextBox_TextChanged != null )
                {
                    _Cts_FileNameTextBox_TextChanged.Cancel();
                    _Cts_FileNameTextBox_TextChanged.Dispose();
                    _Cts_FileNameTextBox_TextChanged = null;
                }
            }

            public void FileNameTextBox_TextChanged( Action< string > setFileNameFinishAction = null )
            {
                CancelAndDispose_Cts_FileNameTextBox_TextChanged();

                _Cts_FileNameTextBox_TextChanged = new CancellationTokenSource();
                Task.Delay( 1_500, _Cts_FileNameTextBox_TextChanged.Token )
                    .ContinueWith( async t =>
                    {
                        if ( t.IsCanceled )
                            return;

                        var fn = await SetFileName_Async( millisecondsDelay: 150 );
                        setFileNameFinishAction?.Invoke( fn );

                    }, TaskScheduler.FromCurrentSynchronizationContext() );
            }

            private async Task< string > SetFileName_Async( int millisecondsDelay )
            {
                var inputOutputFileName = _GetFileNameAction();
                var selectionStart = _FileNameTextBox.SelectionStart;

                _SetFileNameAction( null );
                
                var fn = PathnameCleaner.CleanPathnameAndFilename( inputOutputFileName );
                if ( !fn.IsNullOrWhiteSpace() )
                {
                    _SetFileNameAction( fn );
                        _FileNameTextBox.SelectionStart = Math.Min( selectionStart, fn.Length );
                        selectionStart = _FileNameTextBox.SelectionStart;
                    await Task.Delay( millisecondsDelay );

                    fn = fn.GetFileName_NoThrow();
                    _SetFileNameAction( fn ); 
                        _FileNameTextBox.SelectionStart = Math.Min( selectionStart, fn.Length );
                        selectionStart = _FileNameTextBox.SelectionStart;
                    await Task.Delay( millisecondsDelay );

                    fn = AddOutputFileExtensionIfMissing( fn );
                    _SetFileNameAction( fn );
                        _FileNameTextBox.SelectionStart = Math.Min( selectionStart, fn.Length );

                    return (fn);
                }
                return (null);
            }
        }
    }
}
