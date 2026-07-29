using System;
using System.Net;

namespace m3u8.infrastructure
{
    /// <summary>
    /// 
    /// </summary>
    internal struct DefaultConnectionLimitSaver : IDisposable
    {
#if !(NETCOREAPP)
        private readonly int _DefaultConnectionLimit;
#endif
        private DefaultConnectionLimitSaver( int connectionLimit )
        {
#if !(NETCOREAPP)
            if ( ServicePointManager.DefaultConnectionLimit < connectionLimit )
            {
                _DefaultConnectionLimit = ServicePointManager.DefaultConnectionLimit;
                ServicePointManager.DefaultConnectionLimit = connectionLimit;
            }
            else
            {
                _DefaultConnectionLimit = -1;
            }
#endif
        }
        public void Dispose()
        {
#if !(NETCOREAPP)
            if ( 0 < _DefaultConnectionLimit )
            {
                ServicePointManager.DefaultConnectionLimit = _DefaultConnectionLimit;
            }
#endif
        }

        public void Reset( int connectionLimit )
        {
#if !(NETCOREAPP)
            if ( ServicePointManager.DefaultConnectionLimit < connectionLimit )
            {
                ServicePointManager.DefaultConnectionLimit = connectionLimit;
            }
#endif
        }

        public static DefaultConnectionLimitSaver Create( int connectionLimit ) => new DefaultConnectionLimitSaver( connectionLimit );
    }
    //----------------------------------------------//
}
