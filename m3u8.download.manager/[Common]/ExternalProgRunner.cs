using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using m3u8.download.manager.infrastructure;
using m3u8.helpers;

using static m3u8.download.manager.IExternalProgRunner;

namespace m3u8.download.manager
{
    /// <summary>
    /// 
    /// </summary>
    internal interface IExternalProgRunner
    {
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
        public virtual StatusTypeEnum GetStatus( string outputFileName ) => Queue.Contains( outputFileName ) ? StatusTypeEnum.InQueue : StatusTypeEnum.None;
        public virtual void RemoveFromInnerQueue( string outputFileName ) { }

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
    internal sealed class FFmpegConverterRunner : ExternalProgRunnerBase
    {
        private string             _FFmpegFileLocation;
        private ProcessWindowStyle _ProcessWindowStyle;
        private readonly object __outputFileNames__lock;
        private BlockingCollection< string > __outputFileNames__;
        private HashSet< string > _OutputFileNamesSet;
        private Task _Run_FFmpegTask;
        public FFmpegConverterRunner( string ffmpegFileLocation, ProcessWindowStyle processWindowStyle = ProcessWindowStyle.Normal ) //.Minimized )
        {
            _FFmpegFileLocation = ffmpegFileLocation;
            _ProcessWindowStyle = processWindowStyle;
            __outputFileNames__lock = new object();
            __outputFileNames__ = new BlockingCollection< string >();
            _OutputFileNamesSet = new HashSet< string >( StringComparer.InvariantCultureIgnoreCase );
            _Run_FFmpegTask     = Task.Run( Run_FFmpegTask_Routine );
        }
        public override string ExternalProgFilePath => _FFmpegFileLocation;

        private static bool IsEquals( string s_1, string s_2 ) => (string.Compare( s_1, s_2, true ) == 0);
        private BlockingCollection< string > _OutputFileNames
        {
            get
            {
                lock ( __outputFileNames__lock ) return (__outputFileNames__);
            }
            set
            {
                lock ( __outputFileNames__lock )
                {
                    __outputFileNames__ = value;
                }
            }
        }

        public override void SetExternalProgFilePath( string ffmpegFileLocation ) => _FFmpegFileLocation = ffmpegFileLocation;
        public override bool IsExternalProgFileAreExists() => File.Exists( _FFmpegFileLocation );

        public override bool Run( string outputFileName, bool checkIsExternalProgFileAreExists )
        {
            var suc = (!checkIsExternalProgFileAreExists || IsExternalProgFileAreExists()) && !outputFileName.IsNullOrEmpty();
            if ( suc )
            {
                Add2OutputFileNames/*Run_FFmpeg*/( outputFileName );
            }
            return (suc);
        }
        public override bool Run( IReadOnlyCollection< string > outputFileNames, bool _/*runEachFileAsSeparate*/, bool checkIsExternalProgFileAreExists )
        {
            var suc = (!checkIsExternalProgFileAreExists || IsExternalProgFileAreExists()) && outputFileNames.AnyEx();
            if ( suc )
            {
                foreach ( var fn in outputFileNames )
                {
                    Add2OutputFileNames/*Run_FFmpeg*/( fn );
                }
            }
            return (suc);
        }

        public override StatusTypeEnum GetStatus( string outputFileName )
        {
            if ( Queue.Contains( outputFileName ) )
            {
                return (StatusTypeEnum.InQueue);
            }
            lock ( _OutputFileNamesSet )
            {
                if ( _OutputFileNamesSet.Contains( outputFileName ) )
                {
                    return (IsInProcessNow( outputFileName ) ? StatusTypeEnum.InProcessNow : StatusTypeEnum.InProcessInnerQueue);
                }
            }
            return (StatusTypeEnum.None);
            //return (base.GetStatus( outputFileName ));
        }
        public override void RemoveFromInnerQueue( string outputFileName )
        {
            lock ( _OutputFileNamesSet )
            {
                var suc = _OutputFileNamesSet.Remove( outputFileName );
                if ( suc )
                {
                    var new_OutputFileNames = new BlockingCollection< string >();
                    var filtered = _OutputFileNames.Where( fn => !IsEquals( fn, outputFileName ) );
                    foreach ( var fn in filtered )
                    {
                        new_OutputFileNames.Add( fn );
                    }
                    _OutputFileNames = new_OutputFileNames;
                }
            }
        }

        private void Add2OutputFileNames( string outputFileName )
        {
            lock ( _OutputFileNamesSet )
            {
                if ( _OutputFileNamesSet.Add( outputFileName ) )
                {
                    _OutputFileNames.Add( outputFileName );
                }
            }
        }

        private bool IsInProcessNow( string outputFileName ) => IsEquals( _InProcessNow_outputFileName, outputFileName );
        private string _InProcessNow_outputFileName;
        private void Run_FFmpegTask_Routine()
        {
            while ( true )
            {
                var outputFileName = _OutputFileNames.Take();
                _InProcessNow_outputFileName = outputFileName;
                var (suc, error) = Run_FFmpeg( outputFileName );
                _InProcessNow_outputFileName = null;
                if ( error != null )
                {
                    Debug.WriteLine( error );
                }
                lock ( _OutputFileNamesSet )
                {
                    _OutputFileNamesSet.Remove( outputFileName );
                }
            }
        }
        private (bool suc, Exception error) Run_FFmpeg( string outputFileName )
        {
            const string DEFAULT_EXTENSION = ".mp4";

            var exists_ext = Path.GetExtension( outputFileName );
            var new_fn     = Path.GetFileNameWithoutExtension( outputFileName ) + (exists_ext.EqualIgnoreCase( DEFAULT_EXTENSION ) ? "+" : null) + DEFAULT_EXTENSION;
            var new_ffn    = Path.Combine( Path.GetDirectoryName( outputFileName ), new_fn );
            FileHelperEx.RemoveBadFileAttrs( checkExists: true, new_ffn );

            // "D:\(Distributive)\{ScreenToGif}\ffmpeg.exe" -i %1.avi -c:v libx264 -sn -dn %1.mp4
            var psi = new ProcessStartInfo( /*ExternalProgFilePath*/ )
            {
                //Arguments = "/k echo Hello from new console", /* /k в аргументах для cmd.exe — оставляет консоль открытой после выполнения команды. Если нужно сразу закрыть — используйте /c */
                Arguments       = $"-y -i \"{outputFileName}\" -c:v libx264 -sn -dn \"{new_ffn}\"",
                FileName        = ExternalProgFilePath, // программа для запуска
                UseShellExecute = false, // Отключаем оболочку для прямой работы с процессом
                CreateNoWindow  = false, // явно разрешаем окно
                WindowStyle     = _ProcessWindowStyle,
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
                        return (false, error);
                    }

                    return (true, default);
                }
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );
                //---ffmpeg.Kill( entireProcessTree: true );

                ///*await*/ Task.Delay( 250 ).Wait();
                //FileHelper.DeleteFile_NoThrow( new_fn );
                return (false, ex);
            }
        }

        //private void Run_FFmpeg__PREV( string outputFileName )
        //{
        //    const string DEFAULT_EXTENSION = ".mp4";

        //    var exists_ext = Path.GetExtension( outputFileName );
        //    var new_fn     = Path.GetFileNameWithoutExtension( outputFileName ) + (exists_ext.EqualIgnoreCase( DEFAULT_EXTENSION ) ? "+" : null) + DEFAULT_EXTENSION;
        //    var new_ffn    = Path.Combine( Path.GetDirectoryName( outputFileName ), new_fn );
        //    FileHelperEx.RemoveBadFileAttrs( checkExists: true, new_ffn );

        //    // "D:\(Distributive)\{ScreenToGif}\ffmpeg.exe" -i %1.avi -c:v libx264 -sn -dn %1.mp4
        //    var psi = new ProcessStartInfo( /*ExternalProgFilePath*/ )
        //    {
        //        //Arguments = "/k echo Hello from new console", /* /k в аргументах для cmd.exe — оставляет консоль открытой после выполнения команды. Если нужно сразу закрыть — используйте /c */
        //        Arguments       = $"/c \"\"{ExternalProgFilePath}\" -y -i \"{outputFileName}\" -c:v libx264 -sn -dn \"{new_ffn}\"\"",                    
        //        FileName        = "cmd.exe", // программа для запуска                    
        //        UseShellExecute = true,      // обязательно для показа окна на Windows
        //        CreateNoWindow  = false,     // явно разрешаем окно
        //        WindowStyle     = _ProcessWindowStyle
        //    };

        //    using var ffmpeg = new Process() { StartInfo = psi };
        //    var suc = ffmpeg.Start();
        //    Debug.Assert( suc );

        //    var task_watch = Task.Run( async () =>
        //    {
        //        try
        //        {
        //            ffmpeg.Refresh();
        //            ffmpeg.WaitForExit();

        //            if ( ffmpeg.ExitCode != 0 )
        //            {
        //                var isCtrlC = (ffmpeg.ExitCode == 255) /*Ctrl+C(?)*/;

        //                await Task.Delay( 250 );
        //                FileHelper.DeleteFile_NoThrow( new_fn );
        //            }
        //        }
        //        catch ( Exception ex ) 
        //        {
        //            Debug.WriteLine( ex );
        //            //---ffmpeg.Kill( entireProcessTree: true );

        //            //await Task.Delay( 250 );
        //            //FileHelper.DeleteFile_NoThrow( new_fn );
        //        }
        //    });
        //}

        public override string ToString() => ExternalProgFilePath;
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

        public override string ToString() => string.Join(", ", _ExternalProgRunners.Select( e => $"[{Path.GetFileName( e.ExternalProgFilePath )}].Queue = {e.Queue.Count}" ) );
    }
}
