using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

using m3u8.download.manager.infrastructure;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    internal static class FileNameCleaner4UI
    {
        private static string AddOutputFileExtensionIfMissing( this string outputFileName, string outputFileExtension )
        {
            if ( !outputFileName.IsNullOrEmpty() )
            {
                if ( !outputFileName.EndsWith( outputFileExtension, StringComparison.InvariantCultureIgnoreCase ) )
                {
                    if ( outputFileExtension.HasFirstCharNotDot() )
                    {
                        outputFileName += '.';
                    }
                    outputFileName += outputFileExtension;
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

        public static bool TryGetOutputFileNameByUrl( string m3u8FileUrlText, string outputFileExtension, out string outputFileName )
        {
            try
            {
                var m3u8FileUrl         = new Uri( m3u8FileUrlText );
                var inputOutputFileName = Uri.UnescapeDataString( m3u8FileUrl.AbsolutePath );

                var fn = PathnameCleaner.CleanPathnameAndFilename( inputOutputFileName );
                if ( !fn.IsNullOrWhiteSpace() )
                {
                    outputFileName = NameCleaner.Clean( fn )
                                                .AddOutputFileExtensionIfMissing( outputFileExtension );
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
        public static async Task< string > SetOutputFileNameByUrl_Async( string m3u8FileUrlText, string outputFileExtension, Action< string > setOutputFileNameAction, int millisecondsDelay )
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

                    fn = AddOutputFileExtensionIfMissing( fn, outputFileExtension );
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

        public static string GetOutputFileName( string inputOutputFileName, string outputFileExtension, char? skipChar = null ) => (TryGetOutputFileName( inputOutputFileName, outputFileExtension, out string outputFileName, skipChar ) ? outputFileName : null);
        public static bool TryGetOutputFileName( string inputOutputFileName, string outputFileExtension, out string outputFileName, char? skipChar = null )
        {
            var fn = PathnameCleaner.CleanPathnameAndFilename( inputOutputFileName, skipChar: skipChar );
            if ( !fn.IsNullOrWhiteSpace() )
            {
                outputFileName = fn.GetFileName_NoThrow()
                                   .AddOutputFileExtensionIfMissing( outputFileExtension );
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
