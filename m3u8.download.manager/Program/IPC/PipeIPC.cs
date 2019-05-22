using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Security.Principal;
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
                Task.Run( async () =>
                {
                    for ( var ct = (cts?.Token).GetValueOrDefault( CancellationToken.None ); ; )
                    {
                        using ( var pipeServer = new NamedPipeServerStream( pipeName, PipeDirection.In, 1, PipeTransmissionMode.Message ) )
                        {
                            Debug.WriteLine( $"[SERVER] WaitForConnection... (Current TransmissionMode: '{pipeServer.TransmissionMode}')" );

                            try
                            {
                                await pipeServer.WaitForConnectionAsync( ct );

                                // Read user input and send that to the client process.
                                using ( var sr = new StreamReader( pipeServer ) )
                                {
                                    var line = await sr.ReadLineAsync();
                                    Debug.WriteLine( $"[SERVER] Read from [CLIENT]: {line}" );
                                    if ( BrowserIPC.CommandLine.TryParse4PipeIPC_Multi( line, out var array ) )
                                    {
                                        ReceivedInputParamsArray?.Invoke( array );
                                    }
                                }
                            }                            
                            catch ( Exception ex ) // Catch the IOException that is raised if the pipe is broken or disconnected.
                            {
                                Debug.WriteLine( $"[SERVER] Error: '{ex.Message}'" );
                            }
                        }
                    }
                }, (cts?.Token).GetValueOrDefault( CancellationToken.None ) );
            }
        }

        /// <summary>
        /// 
        /// </summary>
        internal static class NamedPipeClient__out
        {
            public static void Send( string pipeName, in (string m3u8FileUrl, bool autoStartDownload)[] array, int connectMillisecondsTimeout = 5_000 )
            {
                using ( var pipeClient = new NamedPipeClientStream( ".", pipeName, PipeDirection.Out, PipeOptions.None, TokenImpersonationLevel.None, HandleInheritability.Inheritable ) )
                {
                    pipeClient.ConnectAsync( connectMillisecondsTimeout ).Wait( connectMillisecondsTimeout );

                    Debug.WriteLine( $"[CLIENT] Current TransmissionMode: '{pipeClient.TransmissionMode}'" );

                    using ( var sw = new StreamWriter( pipeClient ) )
                    {
                        sw.AutoFlush = true;

                        // Send a 'sync message' and wait for client to receive it.
                        var line = BrowserIPC.CommandLine.Create4PipeIPC( array ); // $"PID: '{Process.GetCurrentProcess().Id}' ('{Assembly.GetEntryAssembly().FullName}') => DateTime.Now: '{DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss.fff")}'";
                        Debug.WriteLine( $"[CLIENT] Send: {line}" );
                        sw.WriteLine( line );
                        pipeClient.WaitForPipeDrain();
                    }
                }

                Debug.WriteLine( "[CLIENT] Client terminating.\r\n" );
            }
        }
    }
}
