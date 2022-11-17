using System;
using System.Reflection;
using System.Threading;

namespace m3u8.download.manager.ipc
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class SingleCopyApplication : IDisposable
    {
        public string MutexName { get; }
        private Mutex Mutex     { get; set; }
        private bool _OwnedMutex;

        private SingleCopyApplication()
        {
            this.Mutex     = null;
            this.MutexName = (Assembly.GetEntryAssembly().GetName().Name + ", {B6E9F4FB-A7A6-4B1D-9C3A-456C015FE7A5}"); //(Assembly.GetEntryAssembly().FullName + ", " + this.GetType().Name); //"{B6E9F4FB-A7A6-4B1D-9C3A-456C015FE7A5}"; // 

            this.Mutex = new Mutex( true, this.MutexName, out _OwnedMutex );
            if ( !_OwnedMutex )
            {
                //it's not-first copy of application
                this.Mutex.Dispose();
                this.Mutex = null;
            }
        }

        void IDisposable.Dispose() => Release();
        public bool IsFirstCopy => (this.Mutex != null);
        public void Release()
        {
            if ( this.Mutex != null )
            {
                if ( _OwnedMutex )
                {
                    this.Mutex.ReleaseMutex();
                }
                this.Mutex.Dispose();
                this.Mutex = null;
            }
        }
#if DEBUG
        public override string ToString() => $"IsFirstCopy: {IsFirstCopy}";
#endif

        #region [.Singleton.]
        private static volatile SingleCopyApplication _OneCopyApplication;
        public static SingleCopyApplication Current
        {
            get 
            { 
                if ( _OneCopyApplication == null )
                {
                    _OneCopyApplication = new SingleCopyApplication();
                }
                return (_OneCopyApplication);
            }
        }
        #endregion
    }
}
