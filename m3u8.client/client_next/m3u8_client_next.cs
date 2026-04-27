using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using _init_params_             = m3u8.i_m3u8_client_next.init_params;
using _DownloadPartInputParams_ = m3u8.i_m3u8_client_next.DownloadPartInputParams;

namespace m3u8
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class m3u8_client_next : i_m3u8_client_next, IDisposable
    {
        #region [.field's.]
        private HttpClient  _HttpClient;
        private IWebProxy   _WebProxy;
        private IDisposable _DisposableObj;
        private bool? _ConnectionClose;
        private int _AttemptRequestCount;
        private HttpCompletionOption _HttpCompletionOption;
        #endregion

        #region [.ctor().]
        public m3u8_client_next( m3u8_client mc )
        {
            var ip = new _init_params_()
            {
                AttemptRequestCount  = mc.InitParams.AttemptRequestCount,
                ConnectionClose      = mc.InitParams.ConnectionClose,
                HttpCompletionOption = mc.InitParams.HttpCompletionOption,
            };
            InitParams = ip;
            Init( mc.HttpClient, ip );
        }
        public m3u8_client_next( HttpClient httpClient, in _init_params_ ip )
        {
            InitParams = ip;
            Init( httpClient, ip );
        }
        private void Init( HttpClient httpClient, in _init_params_ ip )
        {
            _HttpClient = httpClient ?? throw (new ArgumentNullException( nameof( httpClient ) ));            
            _ConnectionClose      = ip.ConnectionClose;
            _AttemptRequestCount  = ip.AttemptRequestCount.GetValueOrDefault( 1 );
            _HttpCompletionOption = ip.HttpCompletionOption.GetValueOrDefault( HttpCompletionOption.ResponseHeadersRead );
        }
        internal m3u8_client_next( in (HttpClient httpClient, IWebProxy webProxy, IDisposable disposableObj) t, in _init_params_ ip ) : this( t.httpClient, ip )
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

        public _init_params_ InitParams { get; }
        public IWebProxy   WebProxy => _WebProxy;
#if M3U8_CLIENT_TESTS
        public HttpClient HttpClient => _HttpClient;
#endif
        private static bool TryGetContentLength( HttpContent responseContent, out (string errorReason, long contentLength, string contentMediaType) t )
        {
            var rch = responseContent.Headers;

            var contentRange = rch.ContentRange;
            if ( (contentRange != null) && contentRange.HasLength )
            {
                var contentLength    = contentRange.Length.Value;
                var contentMediaType = rch.ContentType?.MediaType;
                t = (null, contentLength, contentMediaType);
                return (true);
            }
            else
            {
                if ( rch.ContentLength.HasValue )
                {
                    var contentLength    = rch.ContentLength.Value;
                    var contentMediaType = rch.ContentType?.MediaType;
                    t = (null, contentLength, contentMediaType);
                    return (true);
                }

                if ( contentRange == null )
                {
                    t = ("Content-Range (response-header) is null", 0, null);
                    return (false);
                }
                //if ( !contentRange.HasLength )
                //{
                t = ("Content-Range (response-header) => not has length", 0, null);
                return (false);
                //}
            }
        }
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
                    using ( var req  = CreateRequestGet( url, requestHeaders ) )
                    using ( var resp = await _HttpClient.SendAsync( req, _HttpCompletionOption, ct ).CAX() )
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
                    if ( (attemptRequestCount == 1) || ct.IsCancellationRequested )
                    {
                        throw;
                    }
                }
            }

            throw (new m3u8_Exception( $"No content found while {_AttemptRequestCount} attempt requests." ));
        }

        //------------------------------------------------------------------------------------------//
        public async Task< m3u8_part_ts__v2 > DownloadPart( m3u8_part_ts__v2 part, Uri baseAddress
            , _DownloadPartInputParams_ ip, CancellationToken ct = default, IDictionary< string, string > requestHeaders = null )
        {
            if ( baseAddress == null ) throw (new m3u8_ArgumentException( nameof(baseAddress) ));
            if ( part.Stream == null ) throw (new m3u8_ArgumentException( nameof(part.Stream) ));
            if ( part.RelativeUrlName.IsNullOrWhiteSpace() ) throw (new m3u8_ArgumentException( nameof(part.RelativeUrlName) ));
            if ( ip.ThrottlerBySpeed_User    == null ) throw (new m3u8_ArgumentException( nameof(ip.ThrottlerBySpeed_User) ));
            if ( ip.RespBufPool              == null ) throw (new m3u8_ArgumentException( nameof(ip.RespBufPool) ));
            if ( ip.DownloadThreadsSemaphore == null ) throw (new m3u8_ArgumentException( nameof(ip.DownloadThreadsSemaphore) ));
            //if ( ip.WaitIfPausedEvent        == null ) throw (new m3u8_ArgumentException( nameof(ip.WaitIfPausedEvent) ));
            //----------------------------------------------------------------------------------------------------------------//

            var url = part.GetPartUrl( baseAddress );
            var dpsa = new i_m3u8_client_next.DownloadPartStepActionParams( part );

            for ( var attemptRequestCount = _AttemptRequestCount; 0 < attemptRequestCount; attemptRequestCount-- )
            {
                try
                {
                    using ( var req  = CreateRequestGet( url, requestHeaders ) )
                    using ( var resp = await _HttpClient.SendAsync( req, _HttpCompletionOption, ct ).CAX() )
                    {
                        if ( resp.IsSuccessStatusCode )
                        {
#if NETCOREAPP
                            using var downloadStream = await resp.Content.ReadAsStreamAsync( ct ).CAX();
#else
                            using var downloadStream = await resp.Content.ReadAsStreamAsync( /*ct*/ ).CAX();
#endif
                            dpsa.TotalContentLength = TryGetContentLength( resp.Content, out var x ) ? x.contentLength : null;

                            using var holder = ip.RespBufPool.GetHolder();
                            var buf = holder.Value;
                            for ( var totalBytesReaded = 0L; ; )
                            {
                                #region comm, because fall off by timeout. [.check 'waitIfPausedEvent'.]
                                //if ( !ip.WaitIfPausedEvent.IsSet )
                                //{
                                //    ip.WaitingIfPausedBefore?.Invoke( part );
                                //    ip.WaitIfPausedEvent.Wait( ct );
                                //    ip.WaitingIfPausedAfter?.Invoke( part );
                                //    ip.ThrottlerBySpeed_User.Restart();
                                //}
                                #endregion

                                #region [.throttler by speed.]
                                var instantSpeedInMbps = ip.ThrottlerBySpeed_User.Throttle( ct /*joinedCts.Token*/ );
                                #endregion

                                await ip.DownloadThreadsSemaphore.WaitAsync( ct ).CAX();
                                int bytesReaded;
                                try
                                {
                                    bytesReaded = await downloadStream.ReadAsync( buf, 0, buf.Length, ct ).CAX();
                                }
                                finally
                                {
                                    ip.DownloadThreadsSemaphore.Release();
                                }
                                if ( bytesReaded == 0 )
                                    break;
/*
if ( (new Random()).Next( 10 ) == 0 )
{
    throw new Exception( "(new Random()).Next( 10 ) == 0" );
}
*/
                                await part.Stream.WriteAsync( buf, 0, bytesReaded, ct ).CAX();
                                totalBytesReaded += bytesReaded;

                                ip.ThrottlerBySpeed_User.TakeIntoAccountDownloadedBytes( bytesReaded );

                                dpsa.InstantSpeedInMbps   = instantSpeedInMbps;
                                dpsa.TotalBytesReaded     = totalBytesReaded;
                                dpsa.BytesReaded          = bytesReaded;
                                dpsa.AttemptRequestNumber = _AttemptRequestCount - attemptRequestCount + 1;
                                ip.DownloadPartStepAction?.Invoke( dpsa );
                            }

                            return (part);
                        }

                        throw (await resp.create_m3u8_Exception( ct ).CAX());
                    }
                }
                catch ( Exception ex )
                {
                    dpsa.AttemptRequestNumber = _AttemptRequestCount - attemptRequestCount + 1;
                    ip.DownloadPartStepAction?.Invoke( dpsa );

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
