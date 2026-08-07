using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using _init_params_          = m3u8.client__v2.i_m3u8_client.init_params;
using _ChangeSettingsParams_ = m3u8.client__v2.i_m3u8_client.ChangeSettingsParams;

namespace m3u8.client__v2
{
    /// <summary>
    /// __with_HttpClient
    /// </summary>
    internal sealed class m3u8_client__with_HttpClient : m3u8_client_base, i_m3u8_client, IDisposable
    {
        #region [.field's.]
        private HttpClient __HttpClient__;
        #endregion

        #region [.safety/protected props.]
        private HttpClient _HttpClient
        {
            get
            {
                _RwLock.EnterReadLock();
                var v = __HttpClient__;
                _RwLock.ExitReadLock();
                return (v);
            }
            set
            {
                _RwLock.EnterWriteLock();
                __HttpClient__ = value;
                _RwLock.ExitWriteLock();
            }
        }
        #endregion

        #region [.ctor().]
        public m3u8_client__with_HttpClient( HttpClient httpClient, in _init_params_ ip ) : base( ip )
            => _HttpClient = httpClient ?? throw (new ArgumentNullException( nameof(httpClient) ));
        internal m3u8_client__with_HttpClient( in (HttpClient httpClient, IWebProxy webProxy, IDisposable disposableObj) t, in _init_params_ ip )  : base( ip, t.webProxy, t.disposableObj )
            => _HttpClient = t.httpClient ?? throw (new ArgumentNullException( nameof(t.httpClient) ));
        #endregion

#if M3U8_CLIENT_TESTS
        public HttpClient HttpClient => _HttpClient;
#endif
        protected override void ChangeSettings_Impl( in _ChangeSettingsParams_ csp )
        {
            if ( csp.NetHttpClient.HasValue )
            {
                var t = csp.NetHttpClient.Value;
                if ( _HttpClient != t.httpClient )
                {
                    _HttpClient = t.httpClient ?? throw (new ArgumentNullException( nameof( t.httpClient ) ));

                    Exchange_DisposableObj( t.disposableObj );
                }
            }
        }       
        //------------------------------------------------------------------------------------------//

        protected override Task< HttpResponseMessage > SendRequest_Impl( HttpRequestMessage req, CancellationToken ct ) => _HttpClient.SendAsync( req, _HttpCompletionOption, ct );
        //protected override Task< HttpResponseMessage > SendRequest_Impl( 
        //    HttpRequestMessage req, IObjectPool< CancellationTokenSource > _/*timeoutCtsPool*/, CancellationToken ct ) => _HttpClient.SendAsync( req, _HttpCompletionOption, ct );
        protected override Task< HttpResponseMessage > SendRequest_Impl( 
            HttpRequestMessage req, CtsTimerPool _/*timeoutCtsPool*/, CancellationToken ct ) => _HttpClient.SendAsync( req, _HttpCompletionOption, ct );
    }
}
