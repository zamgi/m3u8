using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace m3u8
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class m3u8_client : i_m3u8_client, IDisposable
    {
        #region [.field's.]
        private HttpClient  _HttpClient;
        private IWebProxy   _WebProxy;
        private IDisposable _DisposableObj;        
        private bool?       _ConnectionClose;
        private int         _AttemptRequestCount;
        private HttpCompletionOption _HttpCompletionOption;
        #endregion

        #region [.ctor().]
        public m3u8_client( HttpClient httpClient, in i_m3u8_client.init_params ip )
        {
            _HttpClient = httpClient ?? throw (new ArgumentNullException( nameof(httpClient) ));
            InitParams  = ip;
            _ConnectionClose      = ip.ConnectionClose;
            _AttemptRequestCount  = ip.AttemptRequestCount.GetValueOrDefault( 1 );
            _HttpCompletionOption = ip.HttpCompletionOption.GetValueOrDefault( HttpCompletionOption.ResponseHeadersRead );

        }
        internal m3u8_client( in (HttpClient httpClient, IWebProxy webProxy, IDisposable disposableObj) t, in i_m3u8_client.init_params ip ) : this( t.httpClient, in ip )
        {
            _WebProxy      = t.webProxy;
            _DisposableObj = t.disposableObj;
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
#if M3U8_CLIENT_TESTS
        public HttpClient HttpClient => _HttpClient;        
#else
        internal HttpClient HttpClient => _HttpClient;
#endif
        private HttpRequestMessage CreateRequestGet( Uri url, IDictionary< string, string > requestHeaders = null )
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

        public async Task< m3u8_file_t > DownloadFile( Uri url, CancellationToken ct = default, IDictionary< string, string > requestHeaders = null )
        {
            if ( url == null ) throw (new m3u8_ArgumentException( nameof(url) ));
            //------------------------------------------------------------------//

            for ( var attemptRequestCount = _AttemptRequestCount; 0 < attemptRequestCount; attemptRequestCount-- )
            {
                try
                {
                    using ( var requestMsg  = CreateRequestGet( url, requestHeaders ) )
                    using ( var responseMsg = await _HttpClient.SendAsync( requestMsg, _HttpCompletionOption, ct ).CAX() )
                    using ( var content     = responseMsg.Content )
                    {
                        /*
                        var charSet = content.Headers.ContentType?.CharSet;
                        if ( charSet != null )
                        {
                            content.Headers.ContentType.CharSet = charSet.Replace( "\"", "" ).Replace( "utf8", "utf-8" );
                        }
                        //*/

                        if ( responseMsg.IsSuccessStatusCode )
                        {
#if NETCOREAPP
                            var text = await content.ReadAsStringAsync( ct ).CAX();
#else
                            var text = await content.ReadAsStringAsync( /*ct*/ ).CAX();
#endif
                            var m3u8File = m3u8_file_t.Parse( text, url );
                            return (m3u8File);
                        }

                        throw (await responseMsg.create_m3u8_Exception( ct ).CAX());

                        /*
                        var bytes        = await content.ReadAsByteArrayAsync().CAX();
                        var responseText = Encoding.UTF8.GetString( bytes );
                        throw (new m3u8_Exception( responseMsg.CreateExceptionMessage( responseText ) ));
                        */
                    }
                }
                catch ( Exception /*ex*/ )
                {
                    if ( (attemptRequestCount == 1) || ct.IsCancellationRequested )
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

            for ( var attemptRequestCount = _AttemptRequestCount; 0 < attemptRequestCount; attemptRequestCount-- )
            {
                try
                {
                    using ( var requestMsg  = CreateRequestGet( url ) )
                    using ( var responseMsg = await _HttpClient.SendAsync( requestMsg, _HttpCompletionOption, ct ).CAX() )
                    using ( var content     = responseMsg.Content )
                    {
                        if ( responseMsg.IsSuccessStatusCode )
                        {
#if NETCOREAPP
                            var bytes = await content.ReadAsByteArrayAsync( ct ).CAX();
#else
                            var bytes = await content.ReadAsByteArrayAsync( /*ct*/ ).CAX();
#endif
                            part.SetBytes( bytes );
                            return (part);
                        }

                        throw (await responseMsg.create_m3u8_Exception( ct ).CAX());
                    }
                }
                catch ( Exception ex )
                {
                    if ( (attemptRequestCount == 1) || ct.IsCancellationRequested )
                    {
                        part.SetError( ex );
                        return (part);
                    }
                }
            }

            throw (new m3u8_Exception( $"No content found while {_AttemptRequestCount} attempt requests." ));
        }

        public async Task< m3u8_part_ts__v2 > DownloadPart__v2( m3u8_part_ts__v2 part, Uri baseAddress, CancellationToken ct = default )
        {
            if ( baseAddress == null )                       throw (new m3u8_ArgumentException( nameof(baseAddress) ));
            if ( part.Stream == null )                       throw (new m3u8_ArgumentException( nameof(part.Stream) ));
            if ( part.RelativeUrlName.IsNullOrWhiteSpace() ) throw (new m3u8_ArgumentException( nameof(part.RelativeUrlName) ));            
            //----------------------------------------------------------------------------------------------------------------//

            var url = part.GetPartUrl( baseAddress );

            for ( var attemptRequestCount = _AttemptRequestCount; 0 < attemptRequestCount; attemptRequestCount-- )
            {
                try
                {
                    using ( var req  = CreateRequestGet( url ) )
                    using ( var resp = await _HttpClient.SendAsync( req, _HttpCompletionOption, ct ).CAX() )
                    {
                        if ( resp.IsSuccessStatusCode )
                        {
#if NETCOREAPP
                            await resp.Content.CopyToAsync( part.Stream, ct ).CAX();
#else
                            await resp.Content.CopyToAsync( part.Stream/*, ct*/ ).CAX();
#endif
                            return (part);
                        }

                        throw (await resp.create_m3u8_Exception( ct ).CAX());
                    }
                }
                catch ( Exception ex )
                {
                    if ( (attemptRequestCount == 1) || ct.IsCancellationRequested )
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
