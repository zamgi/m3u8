using System;
using System.Net;

namespace m3u8.downloader
{
    /// <summary>
    /// 
    /// </summary>
    internal struct DefaultConnectionLimitSaver : IDisposable
    {
        private int _DefaultConnectionLimit;
        private DefaultConnectionLimitSaver( int connectionLimit )
        {
            _DefaultConnectionLimit = ServicePointManager.DefaultConnectionLimit;
            ServicePointManager.DefaultConnectionLimit = connectionLimit;
        }
        public static DefaultConnectionLimitSaver Create( int connectionLimit ) => new DefaultConnectionLimitSaver( connectionLimit );
        public void Dispose() => ServicePointManager.DefaultConnectionLimit = _DefaultConnectionLimit;
    }
}
