using System;
using System.Threading;
using System.Threading.Tasks;

namespace m3u8
{
    /// <summary>
    /// 
    /// </summary>
    internal interface I_download_threads_semaphore : IDisposable
    {
        bool ShareMaxDownloadThreadsBetweenAllDownloadsInstance { get; } //bool UseCrossDownloadInstanceParallelism { get; }

        void Wait( CancellationToken ct );
        Task WaitAsync( CancellationToken ct );
        void Release();
    }
}
