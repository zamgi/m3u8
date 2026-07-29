using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace m3u8.client__v1
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class m3u8_client__with_HttpClient : m3u8_client_base, i_m3u8_client, IDisposable
    {
        #region [.field's.]
        private HttpClient  _HttpClient;
        #endregion

        #region [.ctor().]
        public m3u8_client__with_HttpClient( HttpClient httpClient, in i_m3u8_client.init_params ip ) : base( ip )
            => _HttpClient = httpClient ?? throw (new ArgumentNullException( nameof(httpClient) ));
        internal m3u8_client__with_HttpClient( in (HttpClient httpClient, IWebProxy webProxy, IDisposable disposableObj) t, in i_m3u8_client.init_params ip ) : base( ip, t.webProxy, t.disposableObj )
            => _HttpClient = t.httpClient ?? throw (new ArgumentNullException( nameof(t.httpClient) ));
        #endregion

#if M3U8_CLIENT_TESTS
        public HttpClient HttpClient => _HttpClient;
#else
        internal HttpClient HttpClient => _HttpClient;
#endif

        protected override Task< HttpResponseMessage > SendRequest_Impl( HttpRequestMessage req, CancellationToken ct ) => _HttpClient.SendAsync( req, _HttpCompletionOption, ct );
    }
}
