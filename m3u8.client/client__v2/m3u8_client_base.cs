using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using m3u8.infrastructure;

using _init_params_             = m3u8.client__v2.i_m3u8_client.init_params;
using _ChangeSettingsParams_    = m3u8.client__v2.i_m3u8_client.ChangeSettingsParams;
using _DownloadPartInputParams_ = m3u8.client__v2.i_m3u8_client.DownloadPartInputParams;

namespace m3u8.client__v2
{
    /// <summary>
    /// 
    /// </summary>
    internal abstract class m3u8_client_base : i_m3u8_client, IDisposable
    {
        #region [.field's.]
        private _init_params_ _InitParams;
        private IDisposable   _DisposableObj;
        private bool?         _ConnectionClose;
        private int           __attemptRequestCount__;
        private TimeSpan      __timeout__;
        protected HttpCompletionOption _HttpCompletionOption { get; private set; }

        protected ReaderWriterLockSlim _RwLock { get; private set; } = new ReaderWriterLockSlim( LockRecursionPolicy.SupportsRecursion );
        #endregion

        #region [.safety/protected props.]
        protected TimeSpan _Timeout
        {
            get
            {
                _RwLock.EnterReadLock();
                var v = __timeout__;
                _RwLock.ExitReadLock();
                return (v);
            }
            private set
            {
                _RwLock.EnterWriteLock();
                __timeout__ = value;
                _RwLock.ExitWriteLock();
            }
        }
        protected int _AttemptRequestCount
        {
            get
            {
                _RwLock.EnterReadLock();
                var v = __attemptRequestCount__;
                _RwLock.ExitReadLock();
                return (v);
            }
            private set
            {
                _RwLock.EnterWriteLock();
                __attemptRequestCount__ = value;
                _RwLock.ExitWriteLock();
            }
        }
        #endregion

        #region [.ctor().]
        protected m3u8_client_base( in _init_params_ ip )
        {
            _InitParams           = ip;
            _ConnectionClose      = ip.ConnectionClose;
            _AttemptRequestCount  = ip.AttemptRequestCount.GetValueOrDefault( 1 );
            _Timeout              = ip.Timeout;
            _HttpCompletionOption = ip.HttpCompletionOption.GetValueOrDefault( HttpCompletionOption.ResponseHeadersRead );
        }
        protected m3u8_client_base( in _init_params_ ip, IWebProxy webProxy, IDisposable disposableObj ) : this( ip )
        {
            _InitParams.WebProxy = webProxy;
            _DisposableObj       = disposableObj;
        }

        public virtual void Dispose()
        {
            Dispose_DisposableObj();
            _RwLock.Dispose();
        }
        private void Dispose_DisposableObj()
        {
            if ( _DisposableObj != null )
            {
                _DisposableObj.Dispose();
                _DisposableObj = null;
            }
        }
        protected void Exchange_DisposableObj( IDisposable disposableObj )
        {
            Dispose_DisposableObj();
            _DisposableObj = disposableObj;
        }
        #endregion

        public _init_params_ InitParams => _InitParams;
        public IWebProxy     WebProxy   => _InitParams.WebProxy;
        //------------------------------------------------------------------------------------------//

        protected abstract void ChangeSettings_Impl( in _ChangeSettingsParams_ csp );
        public void ChangeSettings( in _ChangeSettingsParams_ csp )
        {
            if ( csp.Timeout.HasValue ) _Timeout = csp.Timeout.Value;
            if ( csp.AttemptRequestCount.HasValue ) _AttemptRequestCount = csp.AttemptRequestCount.Value;
            _InitParams.WebProxy = csp.WebProxy;

            ChangeSettings_Impl( csp );
        }
        //------------------------------------------------------------------------------------------//

        protected static bool TryGetContentLength( HttpContent responseContent, out (string errorReason, long contentLength, string contentMediaType) t )
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
        //------------------------------------------------------------------------------------------//
        protected abstract Task< HttpResponseMessage > SendRequest_Impl( HttpRequestMessage req, CancellationToken ct );
        //protected abstract Task< HttpResponseMessage > SendRequest_Impl( HttpRequestMessage req, IObjectPool< CancellationTokenSource > timeoutCtsPool, CancellationToken ct );
        protected abstract Task< HttpResponseMessage > SendRequest_Impl( HttpRequestMessage req, CtsTimerPool timeoutCtsPool, CancellationToken ct );
        //------------------------------------------------------------------------------------------//

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

        public async Task< m3u8_part_ts > DownloadPart( m3u8_part_ts part, Uri baseAddress, IDictionary< string, string > requestHeaders, 
            _DownloadPartInputParams_ ip, CancellationToken commonToken )
        {
            if ( baseAddress == null ) throw (new m3u8_ArgumentException( nameof(baseAddress) ));
            if ( part.Stream == null ) throw (new m3u8_ArgumentException( nameof(part.Stream) ));
            if ( part.RelativeUrlName.IsNullOrWhiteSpace() ) throw (new m3u8_ArgumentException( nameof(part.RelativeUrlName) ));
            if ( ip.ThrottlerBySpeed_User    == null ) throw (new m3u8_ArgumentException( nameof(ip.ThrottlerBySpeed_User) ));
            if ( ip.RespBufPool              == null ) throw (new m3u8_ArgumentException( nameof(ip.RespBufPool) ));
            if ( ip.DownloadThreadsSemaphore == null ) throw (new m3u8_ArgumentException( nameof(ip.DownloadThreadsSemaphore) ));
            if ( ip.WaitIfPausedHolder       == null ) throw (new m3u8_ArgumentException( nameof(ip.WaitIfPausedHolder) ));
            if ( ip.TimeoutCtsPool           == null ) throw (new m3u8_ArgumentException( nameof(ip.TimeoutCtsPool) ));
            //----------------------------------------------------------------------------------------------------------------//

            var url = part.GetPartUrl( baseAddress );
            var dpsa = new i_m3u8_client.DownloadPartStepActionParams( part );

            for ( var leftAttemptRequestCount = _AttemptRequestCount; 0 < leftAttemptRequestCount; leftAttemptRequestCount-- )
            {
                using var unionCts = CancellationTokenSource.CreateLinkedTokenSource( commonToken, ip.WaitIfPausedHolder.Token );
                var ct = unionCts.Token;
                var attemptRequestNumber = _AttemptRequestCount - leftAttemptRequestCount + 1;
                try
                {
                    using ( var req  = CreateRequestGet( url, requestHeaders ) )
                    using ( var resp = await SendRequest_Impl( req, ip.TimeoutCtsPool, ct ).CAX() )
                    {
                        if ( resp.IsSuccessStatusCode )
                        {
#if NETCOREAPP
                            using var downloadStream = await resp.Content.ReadAsStreamAsync( ct ).CAX();
#else
                            using var downloadStream = await resp.Content.ReadAsStreamAsync( /*ct*/ ).CAX();
#endif
                            dpsa.TotalContentLength = TryGetContentLength( resp.Content, out var x ) ? x.contentLength : null;

                            using var holder = ip.RespBufPool.GetHolder( out var buf );
                            for ( var totalBytesReaded = 0L; ; )
                            {
                                #region [.throttler by speed.]
                                var instantSpeedInMbps = ip.ThrottlerBySpeed_User.Throttle( ct );
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

                                #region comm.
/*
if ( (new Random()).Next( 10 ) == 0 )
{
    throw new Exception( "(new Random()).Next( 10 ) == 0" );
}
*/
                                #endregion

                                await part.Stream.WriteAsync( buf, 0, bytesReaded, ct ).CAX();
                                totalBytesReaded += bytesReaded;

                                ip.ThrottlerBySpeed_User.TakeIntoAccountDownloadedBytes( bytesReaded );

                                ip.DownloadPartStepAction?.Invoke( dpsa.Set( instantSpeedInMbps, totalBytesReaded, bytesReaded, attemptRequestNumber ) );
                            }

                            return (part);
                        }

                        throw (await resp.create_m3u8_Exception( ct ).CAX());
                    }
                }
                catch ( Exception ex ) when (ip.WaitIfPausedHolder.IsNeedWait || ip.WaitIfPausedHolder.Token.IsCancellationRequested)
                {
                    Debug.WriteLine( ex );

                    ip.WaitIfPausedHolder.Wait_WithCallbacks( part, commonToken );
                    ip.ThrottlerBySpeed_User.Restart();
                    part.Stream.SetLength( 0 );

                    leftAttemptRequestCount++;
                }
                catch ( Exception ex )
                {
                    ip.DownloadPartStepAction?.Invoke( dpsa.SetAttemptRequestNumber( attemptRequestNumber ) );

                    if ( (leftAttemptRequestCount == 1) || /*ct*/commonToken.IsCancellationRequested )
                    {
                        part.SetError( ex );
                        return (part);
                    }
                }

                await Task.Delay( 50 ).CAX();
            }

            throw (new m3u8_Exception( $"No content found while {_AttemptRequestCount} attempt requests." ));
        }
    }
}
