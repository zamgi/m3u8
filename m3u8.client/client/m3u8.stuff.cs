using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;

namespace m3u8
{
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
            var lines = from row in content.Split( ['\r', '\n'], StringSplitOptions.RemoveEmptyEntries )
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
    public static class m3u8_FileHelper
    {
        public static FileStream File_Open4Write( string fileName, FileShare fileShare = /*FileShare.Read*/FileShare.Read | FileShare.Delete )
        {
            var fs = new FileStream( fileName, FileMode.OpenOrCreate, FileAccess.Write, fileShare ); //---var fs = File.OpenWrite( fileName );
            fs.SetLength( 0 );
            return (fs);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class m3u8_Extensions
    {
        [M(O.AggressiveInlining)] public static bool IsNullOrEmpty( this string s ) => string.IsNullOrEmpty( s );
        [M(O.AggressiveInlining)] public static bool IsNullOrWhiteSpace( this string s ) => string.IsNullOrWhiteSpace( s );
        [M(O.AggressiveInlining)] public static string AsPartExceptionMessage( this string responseText ) => (responseText.IsNullOrWhiteSpace() ? string.Empty : ($", '{responseText}'"));
        [M(O.AggressiveInlining)] public static string CreateExceptionMessage( this HttpResponseMessage response, string responseText ) => ($"{(int) response.StatusCode}, {response.ReasonPhrase}{responseText.AsPartExceptionMessage()}");
        [M(O.AggressiveInlining)] public static bool AnyEx< T >( this IEnumerable< T > seq ) => (seq != null) && seq.Any();
        [M(O.AggressiveInlining)] public static bool AnyEx< T >( this IReadOnlyList< T > seq ) => (seq != null) && (0 < seq.Count);

        [M(O.AggressiveInlining)] public static ConfiguredTaskAwaitable< T > CAX< T >( this Task< T > task ) => task.ConfigureAwait( false );
        [M(O.AggressiveInlining)] public static ConfiguredTaskAwaitable CAX( this Task task ) => task.ConfigureAwait( false );

        [M(O.AggressiveInlining)] public static ConfiguredValueTaskAwaitable< T > CAX< T >( this in ValueTask< T > task ) => task.ConfigureAwait( false );
        [M(O.AggressiveInlining)] public static ConfiguredValueTaskAwaitable CAX( this in ValueTask task ) => task.ConfigureAwait( false );

        [M(O.AggressiveInlining)] public static async Task< HttpResponseMessage > SendAsync_Ex( this HttpMessageInvoker httpInvoker, HttpRequestMessage req, TimeSpan timeout, CancellationToken ct )
        {
            using var cts = new CancellationTokenSource( timeout );
            using var union_cts = CancellationTokenSource.CreateLinkedTokenSource( cts.Token, ct );

            try
            {
                var resp = await httpInvoker.SendAsync( req, union_cts.Token ).ConfigureAwait( false );
                return (resp);
            }
            catch ( Exception ex ) when ( ct.IsCancellationRequested )
            {
                throw (new OperationCanceledException( $"Http request was canceled.", ex ));
            }
            catch ( Exception ex ) when ( cts.IsCancellationRequested )
            {
                throw (new TimeoutException( $"Http request timeout exceeded: {timeout}.", ex ));
            }
        }

        [M(O.AggressiveInlining)] public static async Task< byte[] > ReadAsByteArrayAsync_Ex__0( this HttpContent content, CancellationToken ct
            , int capacity = 0x1000, int innerBufferCapacity = 8192 
            )
        {
            //---var bytes = await response.Content.ReadAsByteArrayAsync().CAX();

            // Здесь мы получили только заголовки. Тело ответа еще в сокете. Читаем его потоком (Stream).
            var byteList = new List< byte >( capacity );
            using ( var stream = await content.ReadAsStreamAsync().CAX() )
            {
                var buffer = new byte[ innerBufferCapacity ];
                int bytesRead;
                while ( 0 < (bytesRead = await stream.ReadAsync( buffer, 0, buffer.Length, ct ).CAX()) )
                {
                    if ( bytesRead == buffer.Length )
                    byteList.AddRange( buffer );
                    else
                    byteList.AddRange( buffer.Take( bytesRead ) );
                }
            }
            var bytes = byteList.ToArray();
            return (bytes);
        }
        [M(O.AggressiveInlining)] public static async Task< byte[] > ReadAsByteArrayAsync_Ex( this HttpContent content, CancellationToken ct )
        {
            //---var bytes = await response.Content.ReadAsByteArrayAsync().CAX();

            // Здесь мы получили только заголовки. Тело ответа еще в сокете. Читаем его потоком (Stream).
            using ( var stream = await content.ReadAsStreamAsync().CAX() )
            {
                var bytes = await stream.ReadWithPipelines( ct ).CAX();
                return (bytes);
            }
        }
        [M(O.AggressiveInlining)] public static async Task< byte[] > ReadWithPipelines( this Stream stream, CancellationToken ct )
        {
            // Создаем PipeReader над существующим потоком
            var reader = PipeReader.Create( stream );

            while ( true/*!ct.IsCancellationRequested*/ )
            {
                // Читаем данные из пайпа (он сам управляет ArrayPool внутри)
                ReadResult result = await reader.ReadAsync( ct ).CAX();
                ReadOnlySequence< byte > buffer = result.Buffer;

                // Если поток завершен, собираем всё в один массив
                if ( result.IsCompleted )
                {
                    var finalArray = buffer.ToArray(); // Копируем один раз в конце
                    reader.AdvanceTo( buffer.End ); // Помечаем данные как прочитанные
                    return (finalArray);
                }

                // Если еще не конец, говорим пайпу, что мы пока только "осмотрели" данные
                // но не потребляем их по частям, а ждем конца
                reader.AdvanceTo( buffer.Start, buffer.End );
            }
        }

        public static async Task< m3u8_Exception > create_m3u8_Exception( this HttpResponseMessage resp, CancellationToken ct )
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


        //-----------------------------------------------------------------------------------------------------------//
        public static Uri EmptyUri { get; } = new Uri( string.Empty, UriKind.Relative );
        public static Uri GetAddressUri( this IWebProxy webProxy ) => webProxy?.GetProxy( EmptyUri );
        public static string GetAddress( this IWebProxy webProxy ) => webProxy?.GetProxy( EmptyUri ).ToString();
        //-----------------------------------------------------------------------------------------------------------//

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
    internal struct DefaultConnectionLimitSaver : IDisposable
    {
#if !(NETCOREAPP)
        private readonly int _DefaultConnectionLimit;
#endif
        private DefaultConnectionLimitSaver( int connectionLimit )
        {
#if !(NETCOREAPP)
            if ( ServicePointManager.DefaultConnectionLimit < connectionLimit )
            {
                _DefaultConnectionLimit = ServicePointManager.DefaultConnectionLimit;
                ServicePointManager.DefaultConnectionLimit = connectionLimit;
            }
            else
            {
                _DefaultConnectionLimit = -1;
            }
#endif
        }        
        public void Dispose()
        {
#if !(NETCOREAPP)
            if ( 0 < _DefaultConnectionLimit )
            {
                ServicePointManager.DefaultConnectionLimit = _DefaultConnectionLimit;
            }
#endif
        }

        public void Reset( int connectionLimit )
        {
#if !(NETCOREAPP)
            if ( ServicePointManager.DefaultConnectionLimit < connectionLimit )
            {
                ServicePointManager.DefaultConnectionLimit = connectionLimit;
            }
#endif
        }

        public static DefaultConnectionLimitSaver Create( int connectionLimit ) => new DefaultConnectionLimitSaver( connectionLimit );
    }
}
