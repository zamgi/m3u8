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
        bool ShareMaxDownloadThreadsBetweenAllDownloadsInstance { get; }
        
        //WaitHandle GetAvailableWaitHandle();

        void Wait( CancellationToken ct );
        Task WaitAsync( CancellationToken ct );
        bool Release();
        bool Release_NoThrow();
    }
}
