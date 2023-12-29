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
        private Mutex _Mutex;
        private bool  _OwnedMutex;

        private SingleCopyApplication()
        {
            this._Mutex    = null;
            this.MutexName = Assembly.GetEntryAssembly().FullName + ", " + this.GetType().Name; //"{B6E9F4FB-A7A6-4B1D-9C3A-456C015FE7A5}"; // 

            this._Mutex = new Mutex( true, this.MutexName, out _OwnedMutex );
            if ( !_OwnedMutex )
            {
                //it's not-first copy of application
                this._Mutex.Dispose();
                this._Mutex = null;
            }
        }

        void IDisposable.Dispose() => Release();
        public bool IsFirstCopy => (this._Mutex != null);
        public void Release()
        {
            if ( this._Mutex != null )
            {
                if ( _OwnedMutex )
                {
                    this._Mutex.ReleaseMutex();
                }
                this._Mutex.Dispose();
                this._Mutex = null;
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
