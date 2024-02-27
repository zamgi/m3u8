using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace m3u8.download.manager.infrastructure
{
    /// <summary>
    /// 
    /// </summary>
    internal static class FileDeleter
    {
        public static bool TryDeleteFiles( string[] fullFileNames, CancellationToken ct, int millisecondsDelay = 100 )
        {
            if ( !fullFileNames.AnyEx() )
            {
                return (true);
            }

            var hs = fullFileNames.ToHashSet( StringComparer.InvariantCultureIgnoreCase );
            Parallel.ForEach( hs, new ParallelOptions() { CancellationToken = ct }, fullFileName =>
            {
                for ( ; !ct.IsCancellationRequested; )
                {
                    try
                    {
                        if ( File.Exists( fullFileName ) )
                        {
                            File.Delete( fullFileName );
                        }
                        return;
                    }
                    catch ( Exception ex )
                    {
                        Debug.WriteLine( ex );
                        Task.Delay( millisecondsDelay ).Wait( ct );
                    }
                }
            });
            return (true);
        }
        public static bool TryDeleteFiles( string[] fullFileNames, CancellationToken ct, Action< string > tryDeleteFileAction, int millisecondsDelay = 100 )
        {
            if ( !fullFileNames.AnyEx() )
            {
                return (true);
            }

            var hs = fullFileNames.ToHashSet( StringComparer.InvariantCultureIgnoreCase );
            Parallel.ForEach( hs, new ParallelOptions() { CancellationToken = ct }, fullFileName =>
            {
                for ( ; !ct.IsCancellationRequested; )
                {                    
                    try
                    {
                        if ( File.Exists( fullFileName ) )
                        {
                            tryDeleteFileAction?.Invoke( fullFileName );

                            File.Delete( fullFileName );
                        }
                        return;
                    }
                    catch ( Exception ex )
                    {
                        Debug.WriteLine( ex );
                        Task.Delay( millisecondsDelay ).Wait( ct );
                    }
                }
            });
            return (true);
        }
        public static Task< bool > TryDeleteFiles_Async( string[] fullFileNames, CancellationToken ct, int millisecondsDelay = 100 )
        {
            if ( !fullFileNames.AnyEx() )
            {
                return (Task.FromResult( true ));
            }

            var task = Task.Run( () =>
            {
                var hs = fullFileNames.ToHashSet( StringComparer.InvariantCultureIgnoreCase );

                Parallel.ForEach( hs, new ParallelOptions() { CancellationToken = ct }, fullFileName =>
                {
                    for ( ; !ct.IsCancellationRequested; )
                    {
                        try
                        {
                            if ( File.Exists( fullFileName ) )
                            {
                                File.Delete( fullFileName );
                            }
                            return;
                        }
                        catch ( Exception ex )
                        {
                            Debug.WriteLine( ex );
                            Task.Delay( millisecondsDelay ).Wait( ct );
                        }
                    }
                });

                //var success = !Extensions.AnyFileExists( hs );
                //return (success);
                return (true);
            }, ct );
            return (task);
        }
    }
}
