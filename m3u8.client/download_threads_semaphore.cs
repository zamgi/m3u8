using System;
using System.Threading;

namespace m3u8
{
    /// <summary>
    /// 
    /// </summary>
    internal interface I_download_threads_semaphore : IDisposable
    {
        bool UseCrossDownloadInstanceParallelism { get; }

        void Wait( CancellationToken ct );
        void Release();
    }
}
