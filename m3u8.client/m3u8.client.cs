using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace System.Collections.Generic
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class ICollectionDebugView< T >
    {
        private ICollection< T > _Collection;

        public ICollectionDebugView( ICollection< T > collection ) => _Collection = collection ?? throw new ArgumentNullException( nameof(collection) );

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items
        {
            get
            {
                var items = new T[ _Collection.Count ];
                _Collection.CopyTo( items, 0 );
                return (items);
            }
        }
    }

    /// <summary>
    /// LRU (least recently used) cache
    /// </summary>
    internal interface ILRUCache< T > : ICollection< T >, IReadOnlyCollection< T >
    {
        int Limit { get; set; }
        int Count_ { get; }
        bool TryGetValue( T equalValue, out T actualValue );
    }

    /// <summary>
    /// 
    /// </summary>
    [DebuggerTypeProxy( typeof(ICollectionDebugView<>) )]
    [DebuggerDisplay( "Count = {Count}" )]
    internal sealed class LRUCache< T > : ILRUCache< T >, ICollection< T >, IReadOnlyCollection< T >
    {
        /// <summary>
        /// 
        /// </summary>
        private sealed class LinkedListNode_EqualityComparer : IEqualityComparer< LinkedListNode< T > >
        {
            private IEqualityComparer< T > _Comparer;
            public LinkedListNode_EqualityComparer( IEqualityComparer< T > comparer ) => _Comparer = comparer ?? EqualityComparer< T >.Default;

            public bool Equals( LinkedListNode< T > x, LinkedListNode< T > y ) => _Comparer.Equals( x.Value, y.Value );
            public int GetHashCode( LinkedListNode< T > obj ) => _Comparer.GetHashCode( obj.Value );
        }

        #region [.fields.]
        private HashSet< LinkedListNode< T > > _HashSet;
        private LinkedList< T >                _LinkedList;
        private int                            _Limit;
        #endregion

        #region [.ctor().]
        public LRUCache( int limit, IEqualityComparer< T > comparer = null )
        {
            this.Limit  = limit;
            _HashSet    = new HashSet< LinkedListNode< T > >( limit, new LinkedListNode_EqualityComparer( comparer ) );
            _LinkedList = new LinkedList< T >();
        }

        private LRUCache() { }
        public static LRUCache< T > CreateWithLimitMaxValue( int capacity, IEqualityComparer< T > comparer = null )
        {
            var o = new LRUCache< T >();
            o.Limit       = int.MaxValue;
            o._HashSet    = new HashSet< LinkedListNode< T > >( capacity, new LinkedListNode_EqualityComparer( comparer ) );
            o._LinkedList = new LinkedList< T >();
            return (o);
        }
        #endregion

        public int Limit
        {
            get => _Limit;
            set
            {
                if ( value <= 0 ) throw (new ArgumentException( nameof(Limit) ));

                _Limit = value;
            }
        }
        public bool TryGetValue( T equalValue, out T actualValue )
        {
            if ( _HashSet.TryGetValue( ToNode( equalValue ), out var node ) )
            {
                MoveToFirst( node );

                actualValue = node.Value;
                return (true);
            }
            actualValue = default;
            return (false);
        }

        [M(O.AggressiveInlining)] private static LinkedListNode< T > ToNode( T item ) => new LinkedListNode< T >( item );
        [M(O.AggressiveInlining)] private void AddFirst( LinkedListNode< T > node )
        {
            _LinkedList.AddFirst( node );
            _HashSet   .Add( node );
        }
        [M(O.AggressiveInlining)] private void Remove( LinkedListNode< T > node )
        {
            _LinkedList.Remove( node );
            _HashSet   .Remove( node );
        }
        [M(O.AggressiveInlining)] private void MoveToFirst( LinkedListNode< T > node )
        {
            _LinkedList.Remove  ( node );
            _LinkedList.AddFirst( node );
        }

        public int Count => _LinkedList.Count;
        public int Count_ => _LinkedList.Count;
        public bool IsReadOnly => false;

        public void Add( T item )
        {
            var temp = ToNode( item );
            
            if ( _HashSet.TryGetValue( temp, out var existsNode ) )
            {
                Remove( existsNode );
                AddFirst( temp );
            }
            else
            {
                AddFirst( temp );

                if ( _Limit < _HashSet.Count )
                {
                    Remove( _LinkedList.Last );
                }
            }
        }
        public bool Remove( T item )
        {
            var temp = ToNode( item );
            if ( _HashSet.TryGetValue( temp, out var existsNode ) )
            {
                Remove( existsNode );
                return (true);
            }
            return (false);
        }
        public bool Contains( T item ) => _HashSet.Contains( ToNode( item ) );
        public void Clear()
        {
            _HashSet   .Clear();
            _LinkedList.Clear();
        }

        public IEnumerator< T > GetEnumerator()
        {
            foreach ( var t in _LinkedList )
            {
                yield return (t);
            }
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void CopyTo( T[] array, int arrayIndex )
        {
            foreach ( var t in _LinkedList )
            {
                array[ arrayIndex++ ] = t;
            }
        }
#if DEBUG
        public override string ToString() => $"Count: {Count}";
#endif
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
    public sealed class m3u8_client : IDisposable
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
        private HttpClient  _HttpClient;
        private IDisposable _DisposableObj;
        #endregion

        #region [.ctor().]
        public m3u8_client( HttpClient httpClient, in init_params ip )
        {
            _HttpClient = httpClient ?? throw (new ArgumentNullException( nameof(httpClient) ));
            InitParams  = ip;            
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

        public async Task< m3u8_file_t > DownloadFile( Uri url, CancellationToken? cancellationToken = null )
        {
            if ( url == null ) throw (new m3u8_ArgumentException( nameof(url) ));
            //------------------------------------------------------------------//

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
        public static m3u8_client Create() => Create( HttpClientFactory_WithRefCount.Get() );
        public static m3u8_client Create( in (TimeSpan timeout, int attemptRequestCountByPart) t ) => Create( t.timeout, t.attemptRequestCountByPart );
        public static m3u8_client Create( TimeSpan timeout, int attemptRequestCountByPart = 10 ) => Create( HttpClientFactory_WithRefCount.Get( timeout ), attemptRequestCountByPart );
        public static m3u8_client Create( in m3u8_client.init_params ip ) => Create( HttpClientFactory_WithRefCount.Get(), in ip );

        public static void ForceClearAndDisposeAll() => HttpClientFactory_WithRefCount.ForceClearAndDisposeAll();

        private static m3u8_client Create( in (HttpClient httpClient, IDisposable) t, int attemptRequestCountByPart = 10 )
            => Create( in t, new m3u8_client.init_params()
                             {
                                AttemptRequestCount = Math.Max( attemptRequestCountByPart, 1 )
                             }
                     );
        private static m3u8_client Create( in (HttpClient httpClient, IDisposable) t, in m3u8_client.init_params ip ) => new m3u8_client( in t, in ip );
    }

    /// <summary>
    /// 
    /// </summary>
    internal static class HttpClientFactory_WithRefCount
    {
        /// <summary>
        /// 
        /// </summary>
        public struct init_params
        {
            public TimeSpan? Timeout { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        private sealed class tuple
        {
            /// <summary>
            /// 
            /// </summary>
            public struct EqualityComparer : IEqualityComparer< tuple >
            {
                public bool Equals( tuple x, tuple y ) => (x.Timeout == y.Timeout);
                public int GetHashCode( tuple obj ) => obj.Timeout.GetHashCode();
            }

            private tuple( TimeSpan timeout ) => Timeout = timeout;
            public tuple( TimeSpan timeout, HttpClient httpClient ) => (Timeout, HttpClient, RefCount) = (timeout, httpClient, 0);

            public TimeSpan   Timeout    { get; }
            public HttpClient HttpClient { get; }
            public int        RefCount   { get; private set; }

            public int IncrementRefCount() => ++RefCount;
            public int DecrementRefCount() => --RefCount;

            public static tuple key( TimeSpan? timeout ) => new tuple( timeout.GetValueOrDefault( TimeSpan.Zero ) );
#if DEBUG
            public override string ToString() => $"{Timeout}, (ref: {RefCount})";
#endif
        }

        private static ILRUCache< tuple > _LRUCache;

        static HttpClientFactory_WithRefCount() => _LRUCache = LRUCache< tuple >.CreateWithLimitMaxValue( 0x10, new tuple.EqualityComparer() );

        /// <summary>
        /// 
        /// </summary>
        private struct free_tuple : IDisposable
        {
            private tuple _tuple;
            public free_tuple( tuple t ) => _tuple = t ?? throw (new ArgumentNullException( nameof(t) ));

            public void Dispose()
            {
                if ( _tuple != null )
                {
                    HttpClientFactory_WithRefCount.Release( _tuple );
                    _tuple = null;
                }
            }
        }

        public static (HttpClient, IDisposable) Get( in init_params ip ) => Get( ip.Timeout );
        public static (HttpClient, IDisposable) Get( TimeSpan? timeout = null )
        {
            var key = tuple.key( timeout );
            lock ( _LRUCache )
            {
                if ( !_LRUCache.TryGetValue( key, out var t ) )
                {
                    t = new tuple( key.Timeout, new HttpClient() );
                    if ( timeout.HasValue )
                    {
                        t.HttpClient.Timeout = timeout.Value;
                    }
                    _LRUCache.Add( t );
                }
                t.IncrementRefCount();
                //-----------------------------------------------//
                var removed = (from et in _LRUCache.Skip( 1 )
                               where (et.RefCount <= 0)
                               select et
                              ).ToArray();
                foreach ( var et in removed )
                {
                    _LRUCache.Remove( et );
                    et.HttpClient.Dispose();
                }
                //-----------------------------------------------//
                return (t.HttpClient, new free_tuple( t ));
            }
        }

        private static void Release( tuple t )
        {
            lock ( _LRUCache )
            {
                var refCount = t.DecrementRefCount();
                if ( (refCount <= 0) && (1 < _LRUCache.Count_) )
                {
                    var first_t = _LRUCache.First();
                    if ( t != first_t )
                    {
                        _LRUCache.Remove( t );
                        t.HttpClient.Dispose();
                    }
                }
            }
        }

        public static void ForceClearAndDisposeAll()
        {
            lock ( _LRUCache )
            {
                foreach ( var t in _LRUCache )
                {
                    t.HttpClient.Dispose();
                }
                _LRUCache.Clear();
            }
        }
    }
}
