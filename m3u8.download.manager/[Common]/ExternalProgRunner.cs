using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using m3u8.client;
using m3u8.download.manager.controllers;
using m3u8.download.manager.infrastructure;
using m3u8.helpers;

using static m3u8.download.manager.IExternalProgRunner;

using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;

namespace m3u8.download.manager
{
    /// <summary>
    /// 
    /// </summary>
    internal interface IExternalProgRunner
    {
        public delegate void FinishSuccessProcessFileDelegate( string inputFileName, string convertedFileName );
        public delegate void FinishFailProcessFileDelegate( string inputFileName, string convertedFileName, Exception error );

        /// <summary>
        /// 
        /// </summary>
        public enum StatusTypeEnum
        {
            None,
            InQueue,
            InProcessInnerQueue,
            InProcessNow
        }
        StatusTypeEnum GetStatus( string outputFileName );
        void RemoveFromInnerQueue( string outputFileName );

        event FinishSuccessProcessFileDelegate FinishSuccessProcessFile;
        event FinishFailProcessFileDelegate FinishFailProcessFile;

        int DegreeOfParallelism { get; set; }
        string ExternalProgFilePath { get; }
        bool IsExternalProgFileAreExists();
        void SetExternalProgFilePath( string externalProgFilePath );
        HashSet< string > Queue { get; }
        bool Run( string outputFileName, bool checkIsExternalProgFileAreExists );
        bool Run( IReadOnlyCollection< string > outputFileNames, bool runEachFileAsSeparate, bool checkIsExternalProgFileAreExists );
    }

    /// <summary>
    /// 
    /// </summary>
    internal abstract class ExternalProgRunnerBase : IExternalProgRunner
    {
        protected ExternalProgRunnerBase() => Queue = new HashSet< string >( StringComparer.InvariantCultureIgnoreCase );
        public HashSet< string > Queue { get; }

        public event FinishSuccessProcessFileDelegate FinishSuccessProcessFile;
        public event FinishFailProcessFileDelegate FinishFailProcessFile;
        protected void Raise_FinishSuccessProcessFile( string inputFileName, string convertedFileName ) => FinishSuccessProcessFile?.Invoke( inputFileName, convertedFileName );
        protected void Raise_FinishFailProcessFile( string inputFileName, string convertedFileName, Exception error ) => FinishFailProcessFile?.Invoke( inputFileName, convertedFileName, error );

        public virtual StatusTypeEnum GetStatus( string outputFileName ) => Queue.Contains( outputFileName ) ? StatusTypeEnum.InQueue : StatusTypeEnum.None;
        public virtual void RemoveFromInnerQueue( string outputFileName ) { }


        public abstract int DegreeOfParallelism { get; set; }
        public abstract string ExternalProgFilePath { get; }
        public abstract bool IsExternalProgFileAreExists();
        public abstract bool Run( string outputFileName, bool checkIsExternalProgFileAreExists );
        public abstract bool Run( IReadOnlyCollection< string > outputFileNames, bool runEachFileAsSeparate, bool checkIsExternalProgFileAreExists );
        public abstract void SetExternalProgFilePath( string externalProgFilePath );
    }

    /// <summary>
    /// 
    /// </summary>
    internal sealed class ExternalProgRunner : ExternalProgRunnerBase
    {
        private string _ExternalProgFilePath;
        public ExternalProgRunner( string externalProgFilePath ) => _ExternalProgFilePath = externalProgFilePath;
        public override int DegreeOfParallelism { get => int.MaxValue; set { } }
        public override string ExternalProgFilePath => _ExternalProgFilePath;

        public override void SetExternalProgFilePath( string externalProgFilePath ) => _ExternalProgFilePath = externalProgFilePath;
        public override bool IsExternalProgFileAreExists() => File.Exists( ExternalProgFilePath );
        public override bool Run( string outputFileName, bool checkIsExternalProgFileAreExists )
        {
            var suc = (!checkIsExternalProgFileAreExists || IsExternalProgFileAreExists()) && !outputFileName.IsNullOrEmpty();
            if ( suc )
            {
                ExternalProg_Run( ExternalProgFilePath, $"\"{outputFileName}\"" );
            }
            return (suc);
        }
        public override bool Run( IReadOnlyCollection< string > outputFileNames, bool runEachFileAsSeparate, bool checkIsExternalProgFileAreExists )
        {
            var suc = (!checkIsExternalProgFileAreExists || IsExternalProgFileAreExists()) && outputFileNames.AnyEx();
            if ( suc )
            {
                if ( runEachFileAsSeparate ) //run file by file
                {
                    var buf = new StringBuilder( 0x100 );
                    foreach ( var fn in outputFileNames )
                    {
                        var args = buf.Clear().Append( '"' ).Append( fn ).Append( '"' ).ToString();

                        ExternalProg_Run( ExternalProgFilePath, args );
                    }
                }
                else //run all files as single args
                {
                    var buf = new StringBuilder( 0x100 * outputFileNames.Count );
                    foreach ( var fn in outputFileNames )
                    {
                        buf.Append( '"' ).Append( fn ).Append( '"' ).Append( ' ' );
                    }
                    var args = buf.ToString( 0, buf.Length - 1 );

                    ExternalProg_Run( ExternalProgFilePath, args );
                }
            }
            return (suc);
        }

        private static void ExternalProg_Run( string externalProgFilePath, string args )
        {
            using ( Process.Start( externalProgFilePath, args ) ) {; }
        }

        public override string ToString() => ExternalProgFilePath;
    }

    /// <summary>
    /// 
    /// </summary>
    internal sealed class FFmpegConverterRunner : ExternalProgRunnerBase, IDisposable
    {
        private string              _FFmpegFileLocation;
        private ProcessWindowStyle  _ProcessWindowStyle;
        private OutputFileNamesList _OutputFileNamesList;
        private HashSet< string >   _InProcessNow_OutputFileNamesSet;
        private Task                _Run_FFmpegTask;
        private CancellationTokenSourceWrapper _TokenSource;
        private SemaphoreHolder     _SemaphoreHolder;
        private bool                _IsDisposed;
        private int                 _RealRunningTasks;
        public FFmpegConverterRunner( string ffmpegFileLocation, int degreeOfParallelism, ProcessWindowStyle processWindowStyle = ProcessWindowStyle.Minimized )
        {
            _FFmpegFileLocation  = ffmpegFileLocation;
            _ProcessWindowStyle  = processWindowStyle;
            _OutputFileNamesList = new OutputFileNamesList();
            _InProcessNow_OutputFileNamesSet = new HashSet< string >( StringComparer.InvariantCultureIgnoreCase );
            _Run_FFmpegTask      = Task.Run( Run_FFmpegTask_Routine );
            _TokenSource         = new CancellationTokenSourceWrapper();
            _SemaphoreHolder     = new SemaphoreHolder( degreeOfParallelism );
            _RealRunningTasks    = 0;
        }
        public void Dispose()
        {
            _IsDisposed = true;
            _TokenSource.CancelAndDisposeWithLock_NoThrow();
            _SemaphoreHolder.Dispose_NoThrow();
            _OutputFileNamesList.Dispose();
        }

        public override int DegreeOfParallelism
        {
            get => _SemaphoreHolder.MaxCount;
            set
            {
                value = Math.Max( value, 1 );
                if ( _SemaphoreHolder.MaxCount != value )
                {
                    var busyCount = Math.Max( _SemaphoreHolder.BusyCount, Volatile.Read( ref _RealRunningTasks ) );
                    _SemaphoreHolder.ResetSemaphore( value, busyCount, releaseWorking: false );
                    _TokenSource.CancelWithLock_NoThrow();
                }
            }
        }
        public override string ExternalProgFilePath => _FFmpegFileLocation;

        public override void SetExternalProgFilePath( string ffmpegFileLocation ) => _FFmpegFileLocation = ffmpegFileLocation;
        public override bool IsExternalProgFileAreExists() => File.Exists( _FFmpegFileLocation );

        public override bool Run( string outputFileName, bool checkIsExternalProgFileAreExists )
        {
            var suc = (!checkIsExternalProgFileAreExists || IsExternalProgFileAreExists()) && !outputFileName.IsNullOrEmpty();
            if ( suc )
            {
                _OutputFileNamesList.Add( outputFileName );
            }
            return (suc);
        }
        public override bool Run( IReadOnlyCollection< string > outputFileNames, bool _/*runEachFileAsSeparate*/, bool checkIsExternalProgFileAreExists )
        {
            var suc = (!checkIsExternalProgFileAreExists || IsExternalProgFileAreExists()) && outputFileNames.AnyEx();
            if ( suc )
            {
                _OutputFileNamesList.Add( outputFileNames );
            }
            return (suc);
        }

        public override StatusTypeEnum GetStatus( string outputFileName )
        {
            if ( Queue.Contains( outputFileName ) )
            {
                return (StatusTypeEnum.InQueue);
            }
            if ( _OutputFileNamesList.Contains( outputFileName ) )
            {
                return (IsInProcessNow( outputFileName ) ? StatusTypeEnum.InProcessNow : StatusTypeEnum.InProcessInnerQueue);
            }
            return (StatusTypeEnum.None);
            //return (base.GetStatus( outputFileName ));
        }
        public override void RemoveFromInnerQueue( string outputFileName ) => _OutputFileNamesList.Remove( outputFileName );
        
        [M(O.AggressiveInlining)] private bool IsInProcessNow( string outputFileName ) => _InProcessNow_OutputFileNamesSet.ContainsWithLock( outputFileName );

        //---[M(O.AggressiveInlining)] private bool IsInProcessNow( string outputFileName ) => _InProcessNow_outputFileName.EqualIgnoreCase( outputFileName );
        //---private string _InProcessNow_outputFileName;
        //private void Run_FFmpegTask_Routine__PREV()
        //{
        //    while ( true )
        //    {
        //        var outputFileName = _OutputFileNamesList.Take();
        //        _InProcessNow_OutputFileNamesSet.AddWithLock( outputFileName ); //---_InProcessNow_outputFileName = outputFileName;
        //        var (suc, convertedFileName, error) = Run_FFmpeg( outputFileName );
        //        _InProcessNow_OutputFileNamesSet.RemoveWithLock( outputFileName ); //---_InProcessNow_outputFileName = null;                
        //        _OutputFileNamesList.Remove( outputFileName );

        //        if ( suc )
        //        {
        //            Raise_FinishSuccessProcessFile( outputFileName, convertedFileName );
        //        }
        //        else //if ( error != null )
        //        {
        //            Raise_FinishFailProcessFile( outputFileName, convertedFileName, error );
        //        }
        //    }
        //}
        private bool Enter2Semaphore()
        {
        ONE_MORE_TIME_AFTER_TEMP_BREAK:
            if ( _IsDisposed )
            {
                return (false);
            }

            try
            {
                _SemaphoreHolder.Wait( _TokenSource.TokenWithLock_NoThrow() );
                return (true);
            }
            catch ( Exception ex ) when (_IsDisposed)
            {
                Debug.WriteLine( ex );
                return (false);
            }
            catch ( Exception ex ) when (_TokenSource.IsCancellationRequestedWithLock_NoThrow())
            {
                Debug.WriteLine( ex );

                if ( _IsDisposed )
                {
                    return (false);
                }

                _TokenSource.ResetWithLock_NoThrow();

                goto ONE_MORE_TIME_AFTER_TEMP_BREAK;
            }
        }
        private void Run_FFmpegTask_Routine()
        {
            while ( true )
            {
                if ( !Enter2Semaphore() )
                {
                    break;
                }

                if ( !_OutputFileNamesList.TryTake( out var outputFileName ) )
                {
                    _SemaphoreHolder.Release_NoThrow();
                    outputFileName = _OutputFileNamesList.Take();
                    if ( !Enter2Semaphore() )
                    {
                        break;
                    }
                }
                
                Interlocked.Increment( ref _RealRunningTasks );
                var run_ffmpeg_task = Task.Run(() =>
                {
                    try
                    {
                        _InProcessNow_OutputFileNamesSet.AddWithLock( outputFileName );
                        var (suc, convertedFileName, error) = Run_FFmpeg( outputFileName );
                        _InProcessNow_OutputFileNamesSet.RemoveWithLock( outputFileName );
                        _OutputFileNamesList.Remove( outputFileName );

                        if ( suc )
                        {
                            Raise_FinishSuccessProcessFile( outputFileName, convertedFileName );
                        }
                        else //if ( error != null )
                        {
                            Raise_FinishFailProcessFile( outputFileName, convertedFileName, error );
                        }
                    }
                    finally
                    {
                        var realRunningTasks = Interlocked.Decrement( ref _RealRunningTasks );
                        if ( realRunningTasks < _SemaphoreHolder.MaxCount ) _SemaphoreHolder.Release_NoThrow();
                    }
                });
            }
        }
        private (bool suc, string convertedFileName, Exception error) Run_FFmpeg( string outputFileName )
        {
            const string DEFAULT_EXTENSION = ".mp4";

            var exists_ext = Path.GetExtension( outputFileName );
            var new_fn = Path.GetFileNameWithoutExtension( outputFileName ) + (exists_ext.EqualIgnoreCase( DEFAULT_EXTENSION ) ? "+" : null) + DEFAULT_EXTENSION;
            var new_ffn = Path.Combine( Path.GetDirectoryName( outputFileName ), new_fn );
            FileHelperEx.RemoveBadFileAttrs( checkExists: true, new_ffn );

            // "D:\(Distributive)\{ScreenToGif}\ffmpeg.exe" -i %1.avi -c:v libx264 -sn -dn %1.mp4
            var psi = new ProcessStartInfo( /*ExternalProgFilePath*/ )
            {
                //Arguments = "/k echo Hello from new console", /* /k в аргументах для cmd.exe — оставляет консоль открытой после выполнения команды. Если нужно сразу закрыть — используйте /c */
                Arguments = $"-y -i \"{outputFileName}\" -c:v libx264 -sn -dn \"{new_ffn}\"",
                FileName = ExternalProgFilePath, // программа для запуска
                UseShellExecute = false, // Отключаем оболочку для прямой работы с процессом
                CreateNoWindow = false, // явно разрешаем окно
                WindowStyle = _ProcessWindowStyle,
                //RedirectStandardError  = true,
                //RedirectStandardOutput = true
            };

            try
            {
                using ( var ffmpeg = Process.Start( psi ) )
                {
                    ffmpeg.Refresh();
                    // Ожидаем завершения работы FFmpeg
                    ffmpeg.WaitForExit();

                    // Проверяем код возврата (0 — успех, все остальное — ошибка)
                    if ( ffmpeg.ExitCode != 0 )
                    {
                        var isCtrlC = (ffmpeg.ExitCode == 255) /*Ctrl+C(?)*/;

                        /*await*/
                        Task.Delay( 250 ).Wait();
                        FileHelper.DeleteFile_NoThrow( new_ffn );

                        //var errorLog = ffmpeg.StandardError.ReadToEnd();
                        var error = new Exception( $"FFmpeg exited with an error (Code: {ffmpeg.ExitCode})." );//$"FFmpeg завершился с ошибкой (Код: {ffmpeg.ExitCode}). Лог: {errorLog}" );
                        return (false, new_ffn, error);
                    }

                    return (true, new_ffn, default);
                }
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );
                //---ffmpeg.Kill( entireProcessTree: true );

                ///*await*/ Task.Delay( 250 ).Wait();
                //FileHelper.DeleteFile_NoThrow( new_fn );
                return (false, new_ffn, ex);
            }
        }

        public override string ToString() => ExternalProgFilePath;
    }

    /// <summary>
    /// 
    /// </summary>
    internal sealed class OutputFileNamesList : IDisposable
    {
        private readonly object __outputFileNames__lock;
        private BlockingCollection< string > __outputFileNames__;
        private HashSet< string > _OutputFileNamesSet;
        private CancellationTokenSourceWrapper _TokenSource;
        public OutputFileNamesList()
        {
            __outputFileNames__lock = new object();
            __outputFileNames__ = new BlockingCollection< string >();
            _OutputFileNamesSet = new HashSet< string >( StringComparer.InvariantCultureIgnoreCase );
            _TokenSource = new CancellationTokenSourceWrapper();
        }
        public void Dispose() => _TokenSource.CancelAndDisposeWithLock_NoThrow();

        private BlockingCollection< string > _OutputFileNames
        {
            get
            {
                lock ( __outputFileNames__lock )
                {
                    return (__outputFileNames__);
                }
            }
            set
            {
                lock ( __outputFileNames__lock )
                {
                    __outputFileNames__ = value;
                }
            }
        }

        public bool Add( string outputFileName )
        {
            lock ( _OutputFileNamesSet )
            {
                var suc = _OutputFileNamesSet.Add( outputFileName );
                if ( suc )
                {
                    _OutputFileNames.Add( outputFileName );
                }
                return (suc);
            }
        }
        public void Add( IReadOnlyCollection< string > outputFileNames )
        {
            lock ( _OutputFileNamesSet )
            {
                foreach ( var fn in outputFileNames )
                {
                    var suc = _OutputFileNamesSet.Add( fn );
                    if ( suc )
                    {
                        _OutputFileNames.Add( fn );
                    }
                }
            }
        }
        public bool Remove( string outputFileName )
        {
            lock ( _OutputFileNamesSet )
            {
                var suc = _OutputFileNamesSet.Remove( outputFileName );
                if ( suc )
                {
                    var temp = new BlockingCollection< string >();
                    var filtered = _OutputFileNames.Where( fn => !fn.EqualIgnoreCase( outputFileName ) );
                    foreach ( var fn in filtered )
                    {
                        temp.Add( fn );
                    }
                    _OutputFileNames = temp;
                    _TokenSource.CancelWithLock_NoThrow();
                }
                return (suc);
            }
        }
        public bool Contains( string outputFileName )
        {
            lock ( _OutputFileNamesSet )
            {
                return (_OutputFileNamesSet.Contains( outputFileName ));
            }
        }
        public string Take()
        {
            while ( true )
            {
                try
                {
                    var outputFileName = _OutputFileNames.Take( _TokenSource.TokenWithLock_NoThrow() );
                    return (outputFileName);
                }
                catch ( Exception ex ) when (_TokenSource.IsCancellationRequestedWithLock_NoThrow())
                {
                    Debug.WriteLine( ex );

                    _TokenSource.ResetWithLock_NoThrow();
                }
            }
        }
        public bool TryTake( out string outputFileName ) => _OutputFileNames.TryTake( out outputFileName );
        //public bool TryTake( out string outputFileName, int millisecondsTimeout, CancellationToken ct ) => _OutputFileNames.TryTake( out outputFileName, millisecondsTimeout, ct );
    }

    /// <summary>
    /// 
    /// </summary>
    internal sealed class ExternalProgRunner_Queues
    {
        private IExternalProgRunner[] _ExternalProgRunners;
        public ExternalProgRunner_Queues( params IExternalProgRunner[] externalProgRunners ) => _ExternalProgRunners = externalProgRunners;

        public bool Contains( string item ) => _ExternalProgRunners.Any( e => e.Queue.Contains( item ) );
        public void Remove( IReadOnlyCollection< string > seq ) => _ExternalProgRunners.ForEach( e => e.Queue.Remove( seq ) );
        public IList< (HashSet< string > queue, bool suc) > Remove( string item ) => _ExternalProgRunners.SelectToList( e => (e.Queue, suc: e.Queue.Remove( item )) );
        public void RemoveAllExcept( IReadOnlyCollection< string > seq ) => _ExternalProgRunners.ForEach( e => e.Queue.RemoveAllExcept( seq ) );
        public void Clear() => _ExternalProgRunners.ForEach( e => e.Queue.Clear() );
        public bool Any() => _ExternalProgRunners.Any( e => e.Queue.Any() );

        public override string ToString() => string.Join( ", ", _ExternalProgRunners.Select( e => $"[{Path.GetFileName( e.ExternalProgFilePath )}].Queue = {e.Queue.Count}" ) );
    }


    /// <summary>
    /// 
    /// </summary>
    internal static class ExternalProgRunner_Extensions
    {
        public static bool ContainsWithLock< T >( this ISet< T > set, T key )
        {
            lock ( set )
            {
                return (set.Contains( key ));
            }
        }
        public static bool AddWithLock< T >( this ISet< T > set, T key )
        {
            lock ( set )
            {
                return (set.Add( key ));
            }
        }
        public static bool RemoveWithLock< T >( this ISet< T > set, T key )
        {
            lock ( set )
            {
                return (set.Remove( key ));
            }
        }

        public static void Release_NoThrow( this SemaphoreHolder semaphoreHolder )
        {
            try
            {
                semaphoreHolder.Release();
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );
            }
        }

        public static void CancelWithLock_NoThrow( this CancellationTokenSourceWrapper cts )
        {
            lock ( cts )
            {
                try
                {
                    cts.Cancel();
                }
                catch ( Exception ex )
                {
                    Debug.WriteLine( ex );
                }
            }
        }
        public static void CancelAndDisposeWithLock_NoThrow( this CancellationTokenSourceWrapper cts )
        {
            lock ( cts )
            {
                try
                {
                    cts.Cancel();
                }
                catch ( Exception ex )
                {
                    Debug.WriteLine( ex );
                }

                cts.Dispose_NoThrow();
            }
        }
        public static void ResetWithLock_NoThrow( this CancellationTokenSourceWrapper cts )
        {
            try
            {
                lock ( cts )
                {
                    cts.Reset();
                }
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );
            }
        }
        public static bool IsCancellationRequestedWithLock_NoThrow( this CancellationTokenSourceWrapper cts, bool defVal = true )
        {
            try
            {
                lock ( cts )
                {
                    return (cts.IsCancellationRequested);
                }
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );
                return (defVal);
            }
        }
        public static CancellationToken TokenWithLock_NoThrow( this CancellationTokenSourceWrapper cts, CancellationToken defVal = default )
        {
            try
            {
                lock ( cts )
                {
                    return (cts.Token);
                }
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );
                return (defVal);
            }
        }
    }
}
