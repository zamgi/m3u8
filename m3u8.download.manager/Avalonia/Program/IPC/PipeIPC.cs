using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace m3u8.download.manager.ipc
{
    /// <summary>
    /// 
    /// </summary>
    internal static class PipeIPC
    { 
        /// <summary>
        /// 
        /// </summary>
        internal static class NamedPipeServer__in
        {
            /// <summary>
            /// 
            /// </summary>
            public delegate void ReceivedInputParamsArrayEventHandler( (string m3u8FileUrl, bool autoStartDownload)[] array );

            public static event ReceivedInputParamsArrayEventHandler ReceivedInputParamsArray;

            public static void RunListener( string pipeName, CancellationTokenSource cts = null )
            {
                var ct = (cts?.Token).GetValueOrDefault( CancellationToken.None );

                Task.Run( async () =>
                {
                    for ( ; ; )
                    {
                        using ( var pipeServer = new NamedPipeServerStream( pipeName, PipeDirection.In ) )
                        {
#if DEBUG
                            Debug.WriteLine( $"[SERVER] WaitForConnection... (Current TransmissionMode: '{pipeServer.TransmissionMode}')" );
#endif
                            try
                            {
                                await pipeServer.WaitForConnectionAsync( ct ).ConfigureAwait( false );

                                // Read user input and send that to the client process.
                                using ( var sr = new StreamReader( pipeServer ) )
                                {
                                    var line = await sr.ReadLineAsync().ConfigureAwait( false );
#if DEBUG
                                    Debug.WriteLine( $"[SERVER] Read from [CLIENT]: '{line}'." ); Debug.WriteLine( string.Empty );
#endif
                                    if ( BrowserIPC.CommandLine.TryParse4PipeIPC_Multi( line, out var array ) )
                                    {
                                        ReceivedInputParamsArray?.Invoke( array );
                                    }
                                }
                            }                            
                            catch ( Exception ex ) // Catch the IOException that is raised if the pipe is broken or disconnected.
                            {
                                Debug.WriteLine( $"[SERVER] Error: '{ex.Message}'" ); Debug.WriteLine( string.Empty );
                            }
                        }
                    }
                }, ct );
            }
        }

        /// <summary>
        /// 
        /// </summary>
        internal static class NamedPipeClient__out
        {
            public static void Send_TryFewTimes( string pipeName, (string m3u8FileUrl, bool autoStartDownload)[] array, int connectMillisecondsTimeout = 5_000, int fewTimes = 20 )
            {
                for ( var i = 0; i < fewTimes; i++ )
                {
                    try 
                    {
                        Send( pipeName, array, connectMillisecondsTimeout );
                        return;
                    }
                    catch ( Exception ex )
                    {
                        Debug.WriteLine( ex );

                        Task.Delay( 250 ).Wait();
                    }
                }
            }

            public static void Send( string pipeName, (string m3u8FileUrl, bool autoStartDownload)[] array, int connectMillisecondsTimeout = 5_000 )
            {
                using ( var pipeClient = new NamedPipeClientStream( ".", pipeName, PipeDirection.Out ) )
                {
                    pipeClient.ConnectAsync( connectMillisecondsTimeout ).Wait( connectMillisecondsTimeout );
#if DEBUG
                    Debug.WriteLine( $"[CLIENT] Current TransmissionMode: '{pipeClient.TransmissionMode}'" ); 
#endif
                    using ( var sw = new StreamWriter( pipeClient ) )
                    {
                        sw.AutoFlush = true;

                        // Send a 'sync message' and wait for client to receive it.
                        var line = BrowserIPC.CommandLine.Create4PipeIPC( array ); // $"PID: '{Process.GetCurrentProcess().Id}' ('{Assembly.GetEntryAssembly().FullName}') => DateTime.Now: '{DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss.fff")}'";
#if DEBUG
                        Debug.WriteLine( $"[CLIENT] Send: '{line}'" ); 
#endif
                        sw.WriteLine( line );
                        pipeClient.WaitForPipeDrain_NoThrow();
                    }
                }
#if DEBUG
                Debug.WriteLine( "[CLIENT] Client terminating.\r\n" ); 
#endif
            }
        }

        private static void WaitForPipeDrain_NoThrow( this NamedPipeClientStream pipeClient )
        {
            try
            {
                pipeClient.WaitForPipeDrain();
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );
            }
        }
    }
}
