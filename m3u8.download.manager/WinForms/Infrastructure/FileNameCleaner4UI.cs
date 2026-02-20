using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

#if !(NETCOREAPP)
using System.Linq;
#endif
using System.Threading.Tasks;
#if AVALONIA
using Avalonia.Controls;
#else
using System.Windows.Forms;
#endif

using m3u8.download.manager.infrastructure;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    internal static class FileNameCleaner4UI
    {
        public const int MAX_FileName_LENGTH = 100;
        public const int MAX_FULLPATH_LENGTH = 250;

        private static string CutAtEndByMaxLen( this string s, int maxLength ) => (s != null) && (maxLength < s.Length) ? s.Substring( 0, maxLength ) : s;
        private static bool HasValidExtension( string outputFileName, out string extension )
        {
            const int DEFAULT_EXTENSION_LEN = 4;
            const string MP3_EXT = "mp3";
            const string MP4_EXT = "mp4";

            var ext = Path.GetExtension( outputFileName );
            if ( (ext != null) && (ext.Length == DEFAULT_EXTENSION_LEN) )
            {
                extension = ext;
#if NETCOREAPP
                static bool is_match( ReadOnlySpan< char > ext, ReadOnlySpan< char > pattern ) => char.ToLower( ext[ 0 ] ) == char.ToLower( pattern[ 0 ] ) &&
                                                                                                  char.ToLower( ext[ 1 ] ) == char.ToLower( pattern[ 1 ] ) &&
                                                                                                  char.ToLower( ext[ 2 ] ) == char.ToLower( pattern[ 2 ] );
                var sp = ext.AsSpan( 1 );
                if ( is_match( sp, MP3_EXT ) || is_match( sp, MP4_EXT ) ) return (true);
                
                for ( var i = 0; i < sp.Length; i++ )
                {
                    if ( !char.IsLetter( sp[ i ] ) ) return (false);
                }
                return (true);
#else
                static bool is_match( string ext, string pattern ) => char.ToLower( ext[ 1 ] ) == char.ToLower( pattern[ 0 ] ) &&
                                                                      char.ToLower( ext[ 2 ] ) == char.ToLower( pattern[ 1 ] ) &&
                                                                      char.ToLower( ext[ 3 ] ) == char.ToLower( pattern[ 2 ] );
                if ( is_match( ext, MP3_EXT ) || is_match( ext, MP4_EXT ) ) return (true);

                return (ext.Skip( 1 ).All( c => char.IsLetter( c ) ));
#endif
            }
            else
            {
                extension = default;
            }
            return (false);
        }
        private static string AddOutputFileExtensionIfMissing( this string outputFileName, string outputFileExtension, int maxOutputFileNameLength )
        {
            if ( !outputFileName.IsNullOrEmpty() )
            {
                static string cut_outputFileName_if( string outputFileName, string ext, int maxOutputFileNameLength ) 
                {
                    if ( (maxOutputFileNameLength < outputFileName.Length) && 
                         (ext.Length < outputFileName.Length) &&
                         (ext.Length < maxOutputFileNameLength)
                       )
                    {
                        outputFileName = outputFileName.Substring( 0, outputFileName.Length - ext.Length )
                                                       .CutAtEndByMaxLen( maxOutputFileNameLength - ext.Length )
                                                       + ext;
                    }
                    return (outputFileName);
                }

                if ( outputFileName.EndsWith( outputFileExtension, StringComparison.InvariantCultureIgnoreCase ) )
                {
                    outputFileName = cut_outputFileName_if( outputFileName, outputFileExtension, maxOutputFileNameLength );
                }
                else if ( HasValidExtension( outputFileName, out var validExtension ) )
                {
                    outputFileName = cut_outputFileName_if( outputFileName, validExtension, maxOutputFileNameLength );
                }
                else
                {
                    outputFileName = outputFileName.CutAtEndByMaxLen( maxOutputFileNameLength - outputFileExtension.Length );

                    if ( outputFileExtension.HasFirstCharNotDot() ) outputFileName += '.';
                    outputFileName += outputFileExtension;
                }

                //if ( !outputFileName.EndsWith( outputFileExtension, StringComparison.InvariantCultureIgnoreCase )
                //     && !HasValidExtension( outputFileName )
                //   )
                //{
                //    outputFileName = outputFileName.CutAtEndByMaxLen( maxOutputFileNameLength - outputFileExtension.Length );

                //    if ( outputFileExtension.HasFirstCharNotDot() ) outputFileName += '.';
                //    outputFileName += outputFileExtension;
                //}
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

        public static bool TryGetOutputFileNameByUrl( string m3u8FileUrlText, string outputFileExtension, out string outputFileName
            , int maxOutputFileNameLength = MAX_FileName_LENGTH )
        {
            try
            {
                var m3u8FileUrl         = new Uri( m3u8FileUrlText );
                var inputOutputFileName = Uri.UnescapeDataString( m3u8FileUrl.AbsolutePath );

                var fn = PathnameCleaner.CleanPathnameAndFilename( inputOutputFileName );
                if ( !fn.IsNullOrWhiteSpace() )
                {
                    outputFileName = NameCleaner.Clean( fn )
                                                .AddOutputFileExtensionIfMissing( outputFileExtension, maxOutputFileNameLength );
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
        public static async Task< string > SetOutputFileNameByUrl_Async( string m3u8FileUrlText, string outputFileExtension, Action< string > setOutputFileNameAction, int millisecondsDelay
            , int maxOutputFileNameLength = MAX_FileName_LENGTH )
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

                    fn = fn.AddOutputFileExtensionIfMissing( outputFileExtension, maxOutputFileNameLength );
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

        public static string GetOutputFileName( string inputOutputFileName, string outputFileExtension, char? skipChar = null
            , int maxOutputFileNameLength = MAX_FileName_LENGTH ) => (TryGetOutputFileName( inputOutputFileName, outputFileExtension, out string outputFileName, skipChar, maxOutputFileNameLength ) ? outputFileName : null);
        public static bool TryGetOutputFileName( string inputOutputFileName, string outputFileExtension, out string outputFileName, char? skipChar = null
            , int maxOutputFileNameLength = MAX_FileName_LENGTH )
        {
            var fn = PathnameCleaner.CleanPathnameAndFilename( inputOutputFileName, skipChar: skipChar );
            if ( !fn.IsNullOrWhiteSpace() )
            {
                outputFileName = fn.GetFileName_NoThrow()
                                   .AddOutputFileExtensionIfMissing( outputFileExtension, maxOutputFileNameLength );
                return (!outputFileName.IsNullOrWhiteSpace());
            }
            outputFileName = default;
            return (false);
        }


        public static bool TryCutFileNameIfFullPathTooLong( string directoryPath, string fileName, out string cuttedFileName
            , int maxFullpathLength = MAX_FULLPATH_LENGTH )
        {
            var fullpath = Path.Combine( directoryPath, fileName );
            var excess = fullpath.Length - maxFullpathLength;
            if ( 0 < excess )
            {
                var remain_fn_len = fileName.Length - excess;
                if ( 0 < remain_fn_len )
                {
                    var ext      = Path.GetExtension( fileName );
                    var fn_noext = Path.GetFileNameWithoutExtension( fileName );
                    var cut      = remain_fn_len - ext.Length;
                    if ( (0 < cut) && (cut < fn_noext.Length) )
                    {
                        cuttedFileName = fn_noext.Substring( 0, cut ) + ext;
                    }
                    else
                    {
                        cuttedFileName = /*from start*/fileName.Substring( 0, remain_fn_len ); //---/*from end*/fileName.Substring( excess, remain_fn_len );
                    }

                    Debug.Assert( Path.Combine( directoryPath, cuttedFileName ).Length <= maxFullpathLength );
                    return (true);
                }
            }

            cuttedFileName = default;
            return (false);
        }


        /// <summary>
        /// 
        /// </summary>
        public sealed class Processor : IDisposable
        {
            public Processor( TextBox fileNameTextBox, Func< string > getFileNameAction, Action< string > setFileNameAction ) { }
            public void Dispose() { }

            public void FileNameTextBox_TextChanged( Action< string > setFileNameFinishAction = null ) { }
        }
        #region comm. Processor__PREV.
        /*
        /// <summary>
        /// 
        /// </summary>
        public sealed class Processor__PREV : IDisposable
        {
            private TextBox _FileNameTextBox;
            private Func< string >   _GetFileNameAction;
            private Action< string > _SetFileNameAction;

            public Processor__PREV( TextBox fileNameTextBox, Func< string > getFileNameAction, Action< string > setFileNameAction )
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
                        if ( fn != null )
                        {
                            setFileNameFinishAction?.Invoke( fn );
                        }

                    }, TaskScheduler.FromCurrentSynchronizationContext() );
            }

            private async Task< string > SetFileName_Async( int millisecondsDelay )
            {
                var inputOutputFileName = _GetFileNameAction();
                var selectionStart      = _FileNameTextBox.SelectionStart;

                _SetFileNameAction( null );
                
                var fn = PathnameCleaner.CleanPathnameAndFilename( inputOutputFileName );
                if ( !fn.IsNullOrWhiteSpace() )
                {
                    _SetFileNameAction( fn );
                        _FileNameTextBox.SelectionStart = Math.Min( selectionStart, fn.Length );
                        selectionStart = _FileNameTextBox.SelectionStart;
                    await Task.Delay( millisecondsDelay );

                    fn = fn.GetFileName_NoThrow();
                    if ( fn != null )
                    { 
                        _SetFileNameAction( fn ); 
                            _FileNameTextBox.SelectionStart = Math.Min( selectionStart, fn.Length );
                            selectionStart = _FileNameTextBox.SelectionStart;
                        await Task.Delay( millisecondsDelay );

                        fn = AddOutputFileExtensionIfMissing( fn );
                        _SetFileNameAction( fn );
                            _FileNameTextBox.SelectionStart = Math.Min( selectionStart, fn.Length );

                        return (fn);
                    }
                }
                return (null);
            }
        }
        //*/
        #endregion
    }
}
