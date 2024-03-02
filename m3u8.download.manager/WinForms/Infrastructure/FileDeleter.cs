using System;
using System.Collections.Generic;
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

        public static async Task< bool > TryDeleteFile( string fullFileName, CancellationToken ct, Action< string > tryDeleteFileAction, int millisecondsDelay = 100 )
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
                    return (true);
                }
                catch ( Exception ex )
                {
                    Debug.WriteLine( ex );
                    await Task.Delay( millisecondsDelay, ct );
                }
            }
            return (false);
        }
        public static async Task< bool > TryDeleteFile( string fullFileName, CancellationToken ct, int millisecondsDelay = 100 )
        {
            for ( ; !ct.IsCancellationRequested; )
            {
                try
                {
                    if ( File.Exists( fullFileName ) )
                    {
                        File.Delete( fullFileName );
                    }
                    return (true);
                }
                catch ( Exception ex )
                {
                    Debug.WriteLine( ex );
                    await Task.Delay( millisecondsDelay, ct );
                }
            }
            return (false);
        }

        public static Task DeleteFiles_UseSynchronizationContext( IList< string > fullFileNames, CancellationToken ct, 
            Func< string, CancellationToken, SynchronizationContext, Task< bool > > deleteFilesAction, 
            Action< string > afterSuccesDeleteAction = null )
        {
            var syncCtx = SynchronizationContext.Current;
            var delete_task = fullFileNames.ForEachAsync( (Environment.ProcessorCount << 1), ct, async (fullFileName, ct) =>
            {
                var suc = await deleteFilesAction( fullFileName, ct, syncCtx );
                if ( suc ) afterSuccesDeleteAction?.Invoke( fullFileName );
            });
            return (delete_task);
        }
    }
}
