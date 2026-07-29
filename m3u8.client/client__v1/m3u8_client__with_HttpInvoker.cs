using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using m3u8.infrastructure;

namespace m3u8.client__v1
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class m3u8_client__with_HttpInvoker : m3u8_client_base, i_m3u8_client, IDisposable
    {
        #region [.field's.]
        private HttpMessageInvoker _HttpInvoker;
        #endregion

        #region [.ctor().]
        public m3u8_client__with_HttpInvoker( HttpMessageInvoker httpInvoker, in i_m3u8_client.init_params ip ) : base( ip )
            => _HttpInvoker = httpInvoker ?? throw (new ArgumentNullException( nameof(httpInvoker) ));
        internal m3u8_client__with_HttpInvoker( in (HttpMessageInvoker httpInvoker, IWebProxy webProxy, IDisposable disposableObj) t, in i_m3u8_client.init_params ip ) : base( ip, t.webProxy, t.disposableObj )
            => _HttpInvoker = t.httpInvoker ?? throw (new ArgumentNullException( nameof(t.httpInvoker) ));
        #endregion

#if M3U8_CLIENT_TESTS
        public HttpMessageInvoker HttpInvoker => _HttpInvoker;        
#else
        internal HttpMessageInvoker HttpInvoker => _HttpInvoker;
#endif

        protected override Task< HttpResponseMessage > SendRequest_Impl( HttpRequestMessage req, CancellationToken ct ) => _HttpInvoker.SendAsync_Ex( req, _Timeout, ct );
    }
}
