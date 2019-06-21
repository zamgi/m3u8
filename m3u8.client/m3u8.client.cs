using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;

namespace m3u8.ext
{
    /// <summary>
    /// 
    /// </summary>
    internal static class Extensions
    {
        [M(O.AggressiveInlining)] public static bool IsNullOrEmpty( this string s ) => string.IsNullOrEmpty( s );
        [M(O.AggressiveInlining)] public static bool IsNullOrWhiteSpace( this string s ) => string.IsNullOrWhiteSpace( s );
        [M(O.AggressiveInlining)] public static string AsPartExceptionMessage( this string responseText ) => (responseText.IsNullOrWhiteSpace() ? string.Empty : ($", '{responseText}'"));
        [M(O.AggressiveInlining)] public static string CreateExceptionMessage( this HttpResponseMessage response, string responseText ) => ($"{(int) response.StatusCode}, {response.ReasonPhrase}{responseText.AsPartExceptionMessage()}");
        [M(O.AggressiveInlining)] public static bool AnyEx< T >( this IEnumerable< T > seq ) => (seq != null && seq.Any());

        public static string ReadAsStringAsyncEx( this HttpContent content, CancellationToken ct )
        {
            var t = content.ReadAsStringAsync();
            t.Wait( ct );
            return (t.Result);
        }
        public static byte[] ReadAsByteArrayAsyncEx( this HttpContent content, CancellationToken ct )
        {
            var t = content.ReadAsByteArrayAsync();
            t.Wait( ct );
            return (t.Result);
        }
    }

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

        public void Reset( int connectionLimit ) => ServicePointManager.DefaultConnectionLimit = connectionLimit;
    }
}

namespace m3u8
{
    using m3u8.ext;

    /// <summary>
    /// 
    /// </summary>
    public struct m3u8_part_ts
    {
        public m3u8_part_ts( string relativeUrlName, int orderNumber ) : this()
        {
            RelativeUrlName = relativeUrlName;
            OrderNumber     = orderNumber;
        }

        public string RelativeUrlName { get; private set; }
        public int    OrderNumber     { get; private set; }

        public byte[] Bytes { get; private set; }
        public void SetBytes( byte[] bytes ) => Bytes = bytes;

        public Exception Error { get; private set; }
        public void SetError( Exception error ) => Error = error;
#if DEBUG
        public override string ToString() => ($"{OrderNumber}, '{RelativeUrlName}'");
#endif
    }
    /// <summary>
    /// 
    /// </summary>
    public struct m3u8_part_ts_comparer: IComparer< m3u8_part_ts >
    {
        public int Compare( m3u8_part_ts x, m3u8_part_ts y ) => (x.OrderNumber - y.OrderNumber);
    }

    /// <summary>
    /// 
    /// </summary>
    public struct m3u8_file_t
    {
        public IReadOnlyList< m3u8_part_ts > Parts { get; private set; }

        public Uri BaseAddress { get; private set; }

        public string RawText { get; private set; }

        public static m3u8_file_t Parse( string content, Uri baseAddress )
        {
            var lines = from row in content.Split( new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries )
//---.Take( 50 )
                        let line = row.Trim()
                        where (!line.IsNullOrEmpty() && !line.StartsWith( "#" ))
                        select line
                        ;
            var parts = lines.Select( (line, i) => new m3u8_part_ts( line, i ) );
            var o = new m3u8_file_t()
            {
                Parts       = parts.ToList().AsReadOnly(),
                BaseAddress = baseAddress,
                RawText     = content,
            };
            return (o);
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public static class m3u8_Extensions
    {
        public static string Unwrap4DialogMessage( this Exception ex, out bool isCanceledException )
        {
            isCanceledException = false;

            if ( ex is OperationCanceledException cex )
            {
                isCanceledException = true;
                return (cex.Message);
            }

            if ( ex is m3u8_ArgumentException maex )
            {
                return ($"{nameof(m3u8_ArgumentException)}: '{maex.Message} => [{maex.ParamName}]'");
            }

            if ( ex is AggregateException aex )
            {
                if ( aex.InnerExceptions.All( _ex => _ex is OperationCanceledException ) )
                {
                    isCanceledException = true;
                    return (aex.InnerExceptions.FirstOrDefault()?.Message);
                }

                if ( aex.InnerExceptions.Count == 1 )
                {
                    if ( aex.InnerException is m3u8_Exception mex )
                    {
                        return ($"{nameof(m3u8_Exception)}: '{mex.Message}'");
                    }
                    else if ( aex.InnerException is HttpRequestException hrex )
                    {
                        var sb = new StringBuilder( nameof(HttpRequestException) ).Append( ": '" );
                        for ( Exception x = hrex; x != null; x = x.InnerException )
                        {
                            sb.Append( x.Message ).Append( Environment.NewLine );
                        }
                        return (sb.Append( '\'' ).ToString());
                    }
                    else
                    {
                        return ($"{ex.GetType().Name}: '{ex}'");
                    }
                }
            }

            return (ex.ToString());
        }

        [M(O.AggressiveInlining)]
        public static string Unwrap4DialogMessage( this Exception ex, bool ignoreCanceledException = true )
        {
            var message = ex.Unwrap4DialogMessage( out var isCanceledException );
            return ((isCanceledException && ignoreCanceledException) ? null : message);
        }

        internal static string TrimFromBegin( this string s, int maxLength ) => ((maxLength < s.Length) ? s.Substring( s.Length - maxLength ) : s);
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class m3u8_ArgumentException : ArgumentNullException
    {
        public m3u8_ArgumentException( string paramName ) : base( paramName ) { }
    }
    /// <summary>
    /// 
    /// </summary>
    public sealed class m3u8_Exception : HttpRequestException
    {        
        public m3u8_Exception( string message ) : base( message ) { }
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class m3u8_client
    {
        /// <summary>
        /// 
        /// </summary>
        public struct init_params
        {
            public int?  AttemptRequestCount { get; set; }
            public bool? ConnectionClose     { get; set; }
        }

        #region [.field's.]
        private HttpClient _HttpClient; 
        #endregion

        #region [.ctor().]
        public m3u8_client( HttpClient httpClient, in init_params ip )
        {
            _HttpClient = httpClient ?? throw (new ArgumentNullException( nameof(httpClient) ));
            InitParams  = ip;            
        }
        #endregion

        public init_params InitParams { get; }

        public async Task< m3u8_file_t > DownloadFile( Uri url, CancellationToken? cancellationToken = null )
        {
            if ( url == null ) throw (new m3u8_ArgumentException( nameof(url) ));
            //------------------------------------------------------------------//

            var ct = cancellationToken.GetValueOrDefault( CancellationToken.None );
            var attemptRequestCountByPart = InitParams.AttemptRequestCount.GetValueOrDefault( 1 );
//Task.Delay( 5000 ).Wait( ct );
            for ( var attemptRequestCount = attemptRequestCountByPart; 0 < attemptRequestCount; attemptRequestCount-- )
            {
                try
                {
                    using ( var requestMsg = new HttpRequestMessage( HttpMethod.Get, url ) )
                    {
                        requestMsg.Headers.ConnectionClose = InitParams.ConnectionClose; //true => //.KeepAlive = false, .Add("Connection", "close");

                        using ( HttpResponseMessage response = await _HttpClient.SendAsync( requestMsg, ct ) )
                        using ( HttpContent content = response.Content )
                        {
                            if ( !response.IsSuccessStatusCode )
                            {
                                var responseText = content.ReadAsStringAsyncEx( ct );
                                throw (new m3u8_Exception( response.CreateExceptionMessage( responseText ) ));
                            }

                            var text = content.ReadAsStringAsyncEx( ct );
                            var m3u8File = m3u8_file_t.Parse( text, url );
                            return (m3u8File);
                        }
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

            throw (new m3u8_Exception( $"No content found while {attemptRequestCountByPart} attempt requests" ));
        }
        public async Task< m3u8_part_ts > DownloadPart( m3u8_part_ts part, Uri baseAddress, CancellationToken? cancellationToken = null )
        {
            if ( baseAddress == null )                       throw (new m3u8_ArgumentException( nameof(baseAddress) ));
            if ( part.RelativeUrlName.IsNullOrWhiteSpace() ) throw (new m3u8_ArgumentException( nameof(part.RelativeUrlName) ));
            //----------------------------------------------------------------------------------------------------------------//

            var url = new Uri( baseAddress, part.RelativeUrlName );
            var ct = cancellationToken.GetValueOrDefault( CancellationToken.None );
            var attemptRequestCountByPart = InitParams.AttemptRequestCount.GetValueOrDefault( 1 );

            for ( var attemptRequestCount = attemptRequestCountByPart; 0 < attemptRequestCount; attemptRequestCount-- )
            {
                try
                {
                    using ( var requestMsg = new HttpRequestMessage( HttpMethod.Get, url ) )
                    {
                        requestMsg.Headers.ConnectionClose = InitParams.ConnectionClose; //true => //.KeepAlive = false, .Add("Connection", "close");

                        using ( HttpResponseMessage response = await _HttpClient.SendAsync( requestMsg, ct ) )
                        using ( HttpContent content = response.Content )
                        {
                            if ( !response.IsSuccessStatusCode )
                            {
                                var responseText = content.ReadAsStringAsyncEx( ct );
                                throw (new m3u8_Exception( response.CreateExceptionMessage( responseText ) ));
                            }

                            var bytes = content.ReadAsByteArrayAsyncEx( ct ); //---var bytes = await content.ReadAsByteArrayAsync();
                            part.SetBytes( bytes );
                            return (part);
                        }
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

            throw (new m3u8_Exception( $"No content found while {attemptRequestCountByPart} attempt requests" ));
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public static class m3u8_client_factory
    {
        public static m3u8_client Create() => Create( HttpClientFactory.Get( TimeSpan.Zero ) );
        public static m3u8_client Create( in (TimeSpan timeout, int attemptRequestCountByPart) t ) => Create( t.timeout, t.attemptRequestCountByPart );
        public static m3u8_client Create( TimeSpan timeout, int attemptRequestCountByPart = 10 ) => Create( HttpClientFactory.Get( timeout ), attemptRequestCountByPart );
        public static m3u8_client Create( in (HttpClient httpClient, int attemptRequestCountByPart) t ) => Create( t.httpClient, t.attemptRequestCountByPart );
        public static m3u8_client Create( HttpClient httpClient, int attemptRequestCountByPart = 10 )
            => new m3u8_client( httpClient, new m3u8_client.init_params()
                                            {
                                                AttemptRequestCount = Math.Max( attemptRequestCountByPart, 1 )
                                            }
                              );
        public static bool DisposeBy( TimeSpan? timeout ) => HttpClientFactory.DisposeBy( timeout );
        public static void ClearAndDisposeAll() => HttpClientFactory.ClearAndDisposeAll();
    }

    /// <summary>
    /// 
    /// </summary>
    public static class HttpClientFactory
    {
        /// <summary>
        /// 
        /// </summary>
        public struct init_params
        {
            public TimeSpan? Timeout { get; set; }
        }

        private static ConcurrentDictionary< TimeSpan, HttpClient > _HttpClientsByTimeout;

        static HttpClientFactory() => _HttpClientsByTimeout = new ConcurrentDictionary< TimeSpan, HttpClient >();

        public static HttpClient Get( TimeSpan? timeout = null ) => Get( new init_params() { Timeout = timeout } );
        public static HttpClient Get( in init_params ip )
        {
            var key = ip.Timeout.GetValueOrDefault( TimeSpan.Zero );
            if ( _HttpClientsByTimeout.TryGetValue( key, out var hc ) )
            {
                return (hc);
            }

            lock ( _HttpClientsByTimeout )
            {
                if ( !_HttpClientsByTimeout.TryGetValue( key, out hc ) )
                {
                    hc = new HttpClient();
                    if ( ip.Timeout.HasValue )
                    {
                        hc.Timeout = ip.Timeout.Value;
                    }
                    _HttpClientsByTimeout.TryAdd( key, hc );
                }
                return (hc);
            }
        }
        public static bool DisposeBy( TimeSpan? timeout )
        {
            var key = timeout.GetValueOrDefault( TimeSpan.Zero );
            lock ( _HttpClientsByTimeout )
            {
                if ( _HttpClientsByTimeout.TryRemove( key, out var hc ) )
                {
                    hc.Dispose();
                    return (true);
                }
            }
            return (false);
        }
        public static void ClearAndDisposeAll()
        {
            lock ( _HttpClientsByTimeout )
            {
                foreach ( var hc in _HttpClientsByTimeout.Values )
                {
                    hc.Dispose();
                }
                _HttpClientsByTimeout.Clear();
            }
        }
    }
}
