using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public abstract class m3u8_client_base : i_m3u8_client, IDisposable
    {
        #region [.field's.]
        private IWebProxy   _WebProxy;
        private IDisposable _DisposableObj;        
        private bool?       _ConnectionClose;
        private int         _AttemptRequestCount;        
        protected HttpCompletionOption _HttpCompletionOption { get; private set; }
        protected TimeSpan _Timeout { get; private set; }
        #endregion

        #region [.ctor().]
        protected m3u8_client_base( in i_m3u8_client.init_params ip )
        {
            InitParams            = ip;
            _ConnectionClose      = ip.ConnectionClose;
            _AttemptRequestCount  = ip.AttemptRequestCount.GetValueOrDefault( 1 );
            _Timeout              = ip.Timeout;
            _HttpCompletionOption = ip.HttpCompletionOption.GetValueOrDefault( HttpCompletionOption.ResponseHeadersRead );

        }
        protected m3u8_client_base( in i_m3u8_client.init_params ip, IWebProxy webProxy, IDisposable disposableObj ) : this( ip )
        {
            _WebProxy      = webProxy;
            _DisposableObj = disposableObj;
        }

        public void Dispose()
        {
            if ( _DisposableObj != null )
            {
                _DisposableObj.Dispose();
                _DisposableObj = null;
            }
        }
        #endregion

        public i_m3u8_client.init_params InitParams { get; }
        public IWebProxy WebProxy => _WebProxy;

        protected HttpRequestMessage CreateRequestGet( Uri url, IDictionary< string, string > requestHeaders = null )
        {
            var req = new HttpRequestMessage( HttpMethod.Get, url );
            req.Headers.ConnectionClose = _ConnectionClose;
            if ( requestHeaders != null )
            {
                foreach ( var header in requestHeaders )
                {
                    var suc = req.Headers.TryAddWithoutValidation( header.Key, header.Value );
                    Debug.Assert( suc );
                }
            }            
            return (req);
        }

        protected abstract Task< HttpResponseMessage > SendRequest_Impl( HttpRequestMessage req, CancellationToken ct );

        public Task< m3u8_file_t > DownloadFile( Uri url, CancellationToken ct = default ) => DownloadFile( url, requestHeaders: null, ct );
        public async Task< m3u8_file_t > DownloadFile( Uri url, IDictionary< string, string > requestHeaders = null, CancellationToken ct = default )
        {
            if ( url == null ) throw (new m3u8_ArgumentException( nameof(url) ));
            //------------------------------------------------------------------//

            for ( var leftAttemptRequestCount = _AttemptRequestCount; 0 < leftAttemptRequestCount; leftAttemptRequestCount-- )
            {
                try
                {
                    using ( var req  = CreateRequestGet( url, requestHeaders ) )
                    using ( var resp = await SendRequest_Impl( req, ct ).CAX() )
                    using ( var content = resp.Content )
                    {
                        if ( resp.IsSuccessStatusCode )
                        {
#if NETCOREAPP
                            var text = await content.ReadAsStringAsync( ct ).CAX();
#else
                            var text = await content.ReadAsStringAsync( /*ct*/ ).CAX();
#endif
                            var m3u8File = m3u8_file_t.Parse( text, url );
                            return (m3u8File);
                        }

                        throw (await resp.create_m3u8_Exception( ct ).CAX());
                    }
                }
                catch ( Exception /*ex*/ )
                {
                    if ( (leftAttemptRequestCount == 1) || ct.IsCancellationRequested )
                    {
                        throw;
                    }
                }
            }

            throw (new m3u8_Exception( $"No content found while {_AttemptRequestCount} attempt requests." ));
        }
        public async Task< m3u8_part_ts > DownloadPart( m3u8_part_ts part, Uri baseAddress, CancellationToken ct = default )
        {
            if ( baseAddress == null )                       throw (new m3u8_ArgumentException( nameof(baseAddress) ));
            if ( part.RelativeUrlName.IsNullOrWhiteSpace() ) throw (new m3u8_ArgumentException( nameof(part.RelativeUrlName) ));
            //----------------------------------------------------------------------------------------------------------------//

            var url = part.GetPartUrl( baseAddress );

            for ( var leftAttemptRequestCount = _AttemptRequestCount; 0 < leftAttemptRequestCount; leftAttemptRequestCount-- )
            {
                try
                {
                    using ( var req  = CreateRequestGet( url ) )
                    using ( var resp = await SendRequest_Impl( req, ct ).CAX() )
                    using ( var content = resp.Content )
                    {
                        if ( resp.IsSuccessStatusCode )
                        {
                            var bytes = await content.ReadAsByteArrayAsync_Ex( ct ).CAX();
                            part.SetBytes( bytes );
                            return (part);
                        }

                        throw (await resp.create_m3u8_Exception( ct ).CAX());
                    }
                }
                catch ( Exception ex )
                {
                    if ( (leftAttemptRequestCount == 1) || ct.IsCancellationRequested )
                    {
                        part.SetError( ex );
                        return (part);
                    }
                }
            }

            throw (new m3u8_Exception( $"No content found while {_AttemptRequestCount} attempt requests." ));
        }
    }
}
