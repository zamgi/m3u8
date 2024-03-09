using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
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
        [M(O.AggressiveInlining)] public static bool AnyEx< T >( this IEnumerable< T > seq ) => (seq != null) && seq.Any();
        [M(O.AggressiveInlining)] public static bool AnyEx< T >( this IReadOnlyList< T > seq ) => (seq != null) && (0 < seq.Count);

        public static FileStream File_Open4Write( string fileName, FileShare fileShare = FileShare.Read )
        {
            var fs = new FileStream( fileName, FileMode.OpenOrCreate, FileAccess.Write, fileShare ); //---var fs = File.OpenWrite( fileName );
            fs.SetLength( 0 );
            return (fs);
        }

        [M(O.AggressiveInlining)] public static ConfiguredTaskAwaitable< T > CAX< T >( this Task< T > task ) => task.ConfigureAwait( false );
        [M(O.AggressiveInlining)] public static ConfiguredTaskAwaitable CAX( this Task task ) => task.ConfigureAwait( false );
    }

    /// <summary>
    /// 
    /// </summary>
    internal struct DefaultConnectionLimitSaver : IDisposable
    {
        private readonly int _DefaultConnectionLimit;
        private DefaultConnectionLimitSaver( int connectionLimit )
        {
            if ( ServicePointManager.DefaultConnectionLimit < connectionLimit )
            {
                _DefaultConnectionLimit = ServicePointManager.DefaultConnectionLimit;
                ServicePointManager.DefaultConnectionLimit = connectionLimit;
            }
            else
            {
                _DefaultConnectionLimit = -1;
            }
        }        
        public void Dispose()
        {
            if ( 0 < _DefaultConnectionLimit )
            {
                ServicePointManager.DefaultConnectionLimit = _DefaultConnectionLimit;
            }
        }

        public void Reset( int connectionLimit )
        {
            if ( ServicePointManager.DefaultConnectionLimit < connectionLimit )
            {
                ServicePointManager.DefaultConnectionLimit = connectionLimit;
            }
        }

        public static DefaultConnectionLimitSaver Create( int connectionLimit ) => new DefaultConnectionLimitSaver( connectionLimit );
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
        /// <summary>
        /// 
        /// </summary>
        public readonly struct Comparer: IComparer< m3u8_part_ts >
        {
            public static Comparer Inst { get; } = new Comparer();
            public int Compare( m3u8_part_ts x, m3u8_part_ts y ) => (x.OrderNumber - y.OrderNumber);
        }

        public m3u8_part_ts( string relativeUrlName, int orderNumber ) => (RelativeUrlName, OrderNumber) = (relativeUrlName, orderNumber);

        public string RelativeUrlName { get; }
        public int    OrderNumber     { get; }

        public byte[] Bytes { get; private set; }
        public void SetBytes( byte[] bytes ) => Bytes = bytes;

        public Exception Error { get; private set; }
        public void SetError( Exception error ) => Error = error;
#if DEBUG
        public override string ToString() => $"{OrderNumber}, '{RelativeUrlName}'";
#endif
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
#if DEBUG
        public override string ToString() => $"Parts: {Parts?.Count.ToString() ?? "-"}";
#endif
    }
    //----------------------------------------------//

    /// <summary>
    /// 
    /// </summary>
    public struct m3u8_part_ts__v2 : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        public readonly struct Comparer: IComparer< m3u8_part_ts__v2 >
        {
            public static Comparer Inst { get; } = new Comparer();
            public int Compare( m3u8_part_ts__v2 x, m3u8_part_ts__v2 y ) => (x.OrderNumber - y.OrderNumber);
        }

        public m3u8_part_ts__v2( string relativeUrlName, int orderNumber ) : this() => (RelativeUrlName, OrderNumber) = (relativeUrlName, orderNumber);
        public void Dispose()
        {
            if ( _Holder != null )
            {
                _Holder.Dispose();
                _Holder = null;
            }
        }

        public string RelativeUrlName { get; }
        public int    OrderNumber     { get; }

        private IObjectHolder< Stream > _Holder;
        public Stream Stream { get; private set; }
        public void SetStreamHolder( IObjectHolder< Stream > holder )
        {
            _Holder = holder;
            Stream  = holder.Value;
            Stream.SetLength( 0 );
        }

        public Exception Error { get; private set; }
        public void SetError( Exception error ) => Error = error;
#if DEBUG
        public override string ToString() => $"{OrderNumber}, '{RelativeUrlName}'" +
                                             ((Error != null) ? $", Error: {Error}" : null) + 
                                             ((Stream != null) ? $", Stream: {Stream.Length}"   : null);
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    public struct m3u8_file_t__v2
    {
        public IReadOnlyList< m3u8_part_ts__v2 > Parts { get; private set; }
        public Uri BaseAddress { get; private set; }
        public string RawText { get; private set; }

        public static m3u8_file_t__v2 Parse( string content, Uri baseAddress ) => Parse( m3u8_file_t.Parse( content, baseAddress ) );
        public static m3u8_file_t__v2 Parse( in m3u8_file_t mf )
        {
            var parts = new List< m3u8_part_ts__v2 >( mf.Parts.Count );
                parts.AddRange( mf.Parts.Select( p => new m3u8_part_ts__v2( p.RelativeUrlName, p.OrderNumber ) ) );
            var o = new m3u8_file_t__v2()
            {
                Parts       = parts.AsReadOnly(),
                BaseAddress = mf.BaseAddress,
                RawText     = mf.RawText,
            };
            return (o);
        }
#if DEBUG
        public override string ToString() => $"Parts: {Parts?.Count.ToString() ?? "-"}";
#endif
    }
    //----------------------------------------------//

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

        [M(O.AggressiveInlining)] internal static Uri GetPartUrl( this in m3u8_part_ts part, Uri baseAddress ) => baseAddress.GetPartUrl( part.RelativeUrlName );
        [M(O.AggressiveInlining)] internal static Uri GetPartUrl( this in m3u8_part_ts__v2 part, Uri baseAddress ) => baseAddress.GetPartUrl( part.RelativeUrlName );
        [M(O.AggressiveInlining)] private static Uri GetPartUrl( this Uri baseAddress, string relativeUrlName )
        {
            var url = new Uri( baseAddress, relativeUrlName );
            if ( url.Query.IsNullOrEmpty() )
            {
                var baseQuery = baseAddress.Query;
                if ( !baseQuery.IsNullOrEmpty() && (1 < baseQuery.Length) )
                {
                    url = new Uri( url, baseQuery );
                }
            }
            return (url);
        }
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
    //----------------------------------------------//

    /// <summary>
    /// 
    /// </summary>
    public sealed class m3u8_client : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        public struct init_params
        {
            public int?  AttemptRequestCount { get; set; }
            public bool? ConnectionClose     { get; set; }
            public HttpCompletionOption? HttpCompletionOption { get; set; }
        }

        #region [.field's.]
        private HttpClient  _HttpClient;
        private IDisposable _DisposableObj;
        private bool?       _ConnectionClose;
        private int         _AttemptRequestCount;
        private HttpCompletionOption _HttpCompletionOption;
        #endregion

        #region [.ctor().]
        public m3u8_client( HttpClient httpClient, in init_params ip )
        {
            _HttpClient = httpClient ?? throw (new ArgumentNullException( nameof(httpClient) ));
            InitParams  = ip;
            _ConnectionClose      = ip.ConnectionClose;
            _AttemptRequestCount  = ip.AttemptRequestCount.GetValueOrDefault( 1 );
            _HttpCompletionOption = ip.HttpCompletionOption.GetValueOrDefault( HttpCompletionOption.ResponseHeadersRead );

        }
        internal m3u8_client( in (HttpClient httpClient, IDisposable disposableObj) t, in init_params ip ) : this( t.httpClient, in ip )
        {
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

        public init_params InitParams { get; }
#if M3U8_CLIENT_TESTS
        public HttpClient HttpClient => _HttpClient;
#endif
        private static async Task< m3u8_Exception > create_m3u8_Exception( HttpResponseMessage resp, CancellationToken ct )
        {
            var responseText = default(string);
            try
            {
#if NETCOREAPP
                responseText = await resp.Content.ReadAsStringAsync( ct ).CAX();
#else
                responseText = await resp.Content.ReadAsStringAsync( /*ct*/ ).CAX();
#endif                
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );
                resp.EnsureSuccessStatusCode();
            }
            return (new m3u8_Exception( resp.CreateExceptionMessage( responseText ) ));
        }
        private HttpRequestMessage CreateRequstGet( Uri url )
        {
            var req = new HttpRequestMessage( HttpMethod.Get, url );
            req.Headers.ConnectionClose = _ConnectionClose;
            return (req);
        }

        public async Task< m3u8_file_t > DownloadFile( Uri url, CancellationToken ct = default )
        {
            if ( url == null ) throw (new m3u8_ArgumentException( nameof(url) ));
            //------------------------------------------------------------------//

            for ( var attemptRequestCount = _AttemptRequestCount; 0 < attemptRequestCount; attemptRequestCount-- )
            {
                try
                {
                    using ( var requestMsg  = CreateRequstGet( url ) )
                    using ( var responseMsg = await _HttpClient.SendAsync( requestMsg, _HttpCompletionOption, ct ).CAX() )
                    using ( var content     = responseMsg.Content )
                    {
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

                        throw (await create_m3u8_Exception( responseMsg, ct ).CAX());
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
                    using ( var requestMsg  = CreateRequstGet( url ) )
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

                        throw (await create_m3u8_Exception( responseMsg, ct ).CAX());
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
                    using ( var req  = CreateRequstGet( url ) )
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

                        throw (await create_m3u8_Exception( resp, ct ).CAX());
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
