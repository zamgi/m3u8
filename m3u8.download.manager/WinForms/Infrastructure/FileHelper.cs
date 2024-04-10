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
    internal static class FileHelper
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

        public static bool IsSameDiskDrive( string fileName_1, string fileName_2 )
        {
            if ( (2 < fileName_1?.Length) && (2 < fileName_2?.Length) )
            {
                return (char.ToUpperInvariant( fileName_1[ 0 ] ) == char.ToUpperInvariant( fileName_2[ 0 ] ) &&
                        char.ToUpperInvariant( fileName_1[ 1 ] ) == char.ToUpperInvariant( fileName_2[ 1 ] ));
            }
            return (false);
        }

        public static void DeleteFiles_NoThrow( string[] fileNames )
        {
            if ( fileNames.AnyEx() )
            {
                foreach ( var fileName in fileNames )
                {
                    DeleteFile_NoThrow( fileName );
                }
            }
        }
        public static void DeleteFile_NoThrow( string fileName )
        {
            try
            {
                File.Delete( fileName );
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );
            }
        }
        public static bool TryMoveFile_NoThrow( string sourceFileName, string destFileName, out Exception error )
        {
            try
            {
#if NETCOREAPP
                File.Move( sourceFileName, destFileName, overwrite: true );
#else
                FileHelper.DeleteFile_NoThrow( destFileName );
                File.Move( sourceFileName, destFileName );
#endif
                error = default;
                return (true);
            }
            catch ( Exception ex )
            {
                error = ex;
                return (false);
            }
        }
        public static string GetFirstExistsDirectory( string path )
        {
            for ( var dir = path; !dir.IsNullOrEmpty(); dir = Path.GetDirectoryName( dir ) )
            {
                if ( Directory.Exists( dir ) )
                {
                    return (dir);
                }
            }

            return (null);
        }
        public static (bool success, string outputFileName) TryGetFirstFileExists( ICollection< string > fileNames ) => (TryGetFirstFileExists( fileNames, out var outputFileName ), outputFileName);
        public static bool TryGetFirstFileExists( ICollection< string > fileNames, out string existsFileName )
        {
            if ( fileNames.AnyEx() )
            {
                foreach ( var fileName in fileNames )
                {
                    if ( (fileName != null) && File.Exists( fileName ) )
                    {
                        existsFileName = fileName;
                        return (true);
                    }
                }
            }
            existsFileName = null;
            return (false);
        }
        public static bool AnyFileExists( ICollection< string > fileNames ) => TryGetFirstFileExists( fileNames, out var _ );
    }
}
