using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using m3u8.infrastructure;

using _init_params_          = m3u8.client__v2.i_m3u8_client.init_params;
using _ChangeSettingsParams_ = m3u8.client__v2.i_m3u8_client.ChangeSettingsParams;

namespace m3u8.client__v2
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class m3u8_client__with_HttpInvoker : m3u8_client_base, i_m3u8_client, IDisposable
    {
        #region [.field's.]
        private HttpMessageInvoker __HttpInvoker__;
        #endregion

        #region [.safety/protected props.]
        private HttpMessageInvoker _HttpInvoker
        {
            get
            {
                _RwLock.EnterReadLock();
                var v = __HttpInvoker__;
                _RwLock.ExitReadLock();
                return (v);
            }
            set
            {
                _RwLock.EnterWriteLock();
                __HttpInvoker__ = value;
                _RwLock.ExitWriteLock();
            }
        }
        #endregion

        #region [.ctor().]
        public m3u8_client__with_HttpInvoker( HttpMessageInvoker httpInvoker, in _init_params_ ip ) : base( ip )
            => _HttpInvoker = httpInvoker ?? throw (new ArgumentNullException( nameof(httpInvoker) ));
        internal m3u8_client__with_HttpInvoker( in (HttpMessageInvoker httpInvoker, IWebProxy webProxy, IDisposable disposableObj) t, in _init_params_ ip ) : base( ip, t.webProxy, t.disposableObj )
            => _HttpInvoker = t.httpInvoker ?? throw (new ArgumentNullException( nameof(t.httpInvoker) ));
        #endregion

#if M3U8_CLIENT_TESTS
        public HttpMessageInvoker HttpInvoker => _HttpInvoker;
#endif
        protected override void ChangeSettings_Impl( in _ChangeSettingsParams_ csp )
        {
            if ( csp.NetHttpInvoker.HasValue )
            {
                var t = csp.NetHttpInvoker.Value;
                if ( _HttpInvoker != t.httpInvoker )
                {
                    _HttpInvoker = t.httpInvoker ?? throw (new ArgumentNullException( nameof(t.httpInvoker) ));

                    Exchange_DisposableObj( t.disposableObj );
                }
            }
        }
        //------------------------------------------------------------------------------------------//

        protected override Task< HttpResponseMessage > SendRequest_Impl( HttpRequestMessage req, CancellationToken ct ) => _HttpInvoker.SendAsync_Ex( req, _Timeout, ct );
        protected override Task< HttpResponseMessage > SendRequest_Impl( 
            HttpRequestMessage req, IObjectPool< CancellationTokenSource > timeoutCtsPool, CancellationToken ct ) => _HttpInvoker.SendAsync_Ex( req, timeoutCtsPool, _Timeout, ct );
    }
}
