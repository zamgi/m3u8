using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using m3u8.download.manager.infrastructure;

namespace m3u8.download.manager
{
    /// <summary>
    /// 
    /// </summary>
    internal interface IExternalProgRunner
    {
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
        public FFmpegConverterRunner( string ffmpegFileLocation, ProcessWindowStyle processWindowStyle = ProcessWindowStyle.Normal ) //.Minimized )
        {
            _FFmpegFileLocation = ffmpegFileLocation;
            _ProcessWindowStyle = processWindowStyle;
        }
        public override string ExternalProgFilePath => _FFmpegFileLocation;

        public override void SetExternalProgFilePath( string ffmpegFileLocation ) => _FFmpegFileLocation = ffmpegFileLocation;
        public override bool IsExternalProgFileAreExists() => File.Exists( _FFmpegFileLocation );

        public override bool Run( string outputFileName, bool checkIsExternalProgFileAreExists )
        {
            var suc = (!checkIsExternalProgFileAreExists || IsExternalProgFileAreExists()) && !outputFileName.IsNullOrEmpty();
            if ( suc )
            {
                Run_FFmpeg( outputFileName );
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
                    Run_FFmpeg( fn );
                }
            }
            return (suc);
        }

        private void Run_FFmpeg( string outputFileName )
        {
            const string DEFAULT_EXTENSION = ".mp4";

            var exists_ext = Path.GetExtension( outputFileName );
            var new_fn = Path.GetFileNameWithoutExtension( outputFileName ) + (exists_ext.EqualIgnoreCase( DEFAULT_EXTENSION ) ? "+" : null) + DEFAULT_EXTENSION;
            var new_ffn = Path.Combine( Path.GetDirectoryName( outputFileName ), new_fn );
            FileHelper.RemoveBadFileAttrs( checkExists: true, new_ffn );

            // "D:\(Distributive)\{ScreenToGif}\ffmpeg.exe" -i %1.avi -c:v libx264 -sn -dn %1.mp4
            var psi = new ProcessStartInfo( /*ExternalProgFilePath*/ )
            {
                //Arguments = "/k echo Hello from new console", /* /k в аргументах для cmd.exe — оставляет консоль открытой после выполнения команды. Если нужно сразу закрыть — используйте /c */
                Arguments       = $"/c \"\"{ExternalProgFilePath}\" -y -i \"{outputFileName}\" -c:v libx264 -sn -dn \"{new_ffn}\"\"",                    
                FileName        = "cmd.exe", // программа для запуска                    
                UseShellExecute = true,      // обязательно для показа окна на Windows
                CreateNoWindow  = false,     // явно разрешаем окно
                WindowStyle     = _ProcessWindowStyle
            };

            using var ffmpeg = new Process() { StartInfo = psi };
            var suc = ffmpeg.Start();
            Debug.Assert( suc );

            var task_watch = Task.Run( async () =>
            {
                try
                {
                    ffmpeg.WaitForExit();

                    if ( ffmpeg.ExitCode != 0 )
                    {
                        var isCtrlC = (ffmpeg.ExitCode == 255) /*Ctrl+C(?)*/;

                        await Task.Delay( 250 );
                        FileHelper.DeleteFile_NoThrow( new_fn );
                    }
                }
                catch ( Exception ex ) 
                {
                    Debug.WriteLine( ex );
                    //---ffmpeg.Kill( entireProcessTree: true );

                    //await Task.Delay( 250 );
                    //FileHelper.DeleteFile_NoThrow( new_fn );
                }
            });
        }

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
