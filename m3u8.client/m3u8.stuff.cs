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
    public static class m3u8_Consts
    {
        public const bool      DEFAULT_CONNECTIONCLOSE    = true;
        public const int       DEFAULT_TIMEOUT_IN_SECONDS = 100;
        public static TimeSpan DEFAULT_TIMEOUT            => TimeSpan.FromSeconds( DEFAULT_TIMEOUT_IN_SECONDS );
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
}

namespace m3u8.infrastructure
{
    /// <summary>
    /// 
    /// </summary>
    internal static partial class Extensions
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
            using var timeout_cts = new CancellationTokenSource( timeout );
            using var union_cts   = CancellationTokenSource.CreateLinkedTokenSource( timeout_cts.Token, ct );

            try
            {
                var resp = await httpInvoker.SendAsync( req, union_cts.Token ).ConfigureAwait( false );
                return (resp);
            }
            catch ( Exception ex ) when (ct.IsCancellationRequested)
            {
                throw (new OperationCanceledException( $"Http request was canceled.", ex ));
            }
            catch ( Exception ex ) when (timeout_cts.IsCancellationRequested)
            {
                throw (new TimeoutException( $"Http request timeout exceeded: {timeout}.", ex ));
            }
        }
//        [M(O.AggressiveInlining)] public static async Task< HttpResponseMessage > SendAsync_Ex( this HttpMessageInvoker httpInvoker, HttpRequestMessage req
//            , IObjectPool< CancellationTokenSource > timeoutCtsPool, TimeSpan timeout, CancellationToken ct )
//        {
//#if NETCOREAPP
//            using var h = timeoutCtsPool.GetHolder( out var timeout_cts );
//            var suc = timeout_cts.TryReset(); Debug.Assert( suc );

//            CancellationTokenSource timeout_cts_4_dispose;
//            if ( suc )
//            {
//                timeout_cts.CancelAfter( timeout );
//                timeout_cts_4_dispose = null;
//            }
//            else
//            {
//                timeout_cts_4_dispose = timeout_cts = new CancellationTokenSource( timeout );
//            }
//#else
//            using var timeout_cts = new CancellationTokenSource( timeout ); 
//#endif
//            using var union_cts = CancellationTokenSource.CreateLinkedTokenSource( timeout_cts.Token, ct );

//            try
//            {
//                var resp = await httpInvoker.SendAsync( req, union_cts.Token ).ConfigureAwait( false );
//                return (resp);
//            }
//            catch ( Exception ex ) when (ct.IsCancellationRequested)
//            {
//                throw (new OperationCanceledException( $"Http request was canceled.", ex ));
//            }
//            catch ( Exception ex ) when (timeout_cts.IsCancellationRequested)
//            {
//                throw (new TimeoutException( $"Http request timeout exceeded: {timeout}.", ex ));
//            }
//#if NETCOREAPP
//            finally
//            {
//                timeout_cts_4_dispose?.Dispose();
//            }
//#endif
//        }
        [M(O.AggressiveInlining)] public static async Task< HttpResponseMessage > SendAsync_Ex( this HttpMessageInvoker httpInvoker, HttpRequestMessage req
            , CtsTimerPool timeoutCtsPool, TimeSpan timeout, CancellationToken ct )
        {
            using var h = timeoutCtsPool.Acquire( timeout, out var timeout_cts ); Debug.Assert( !timeout_cts.IsCancellationRequested );
            using var union_cts = CancellationTokenSource.CreateLinkedTokenSource( timeout_cts.Token, ct );
            try
            {
                var resp = await httpInvoker.SendAsync( req, union_cts.Token ).ConfigureAwait( false );
                return (resp);
            }
            catch ( Exception ex ) when (ct.IsCancellationRequested)
            {
                throw (new OperationCanceledException( $"Http request was canceled.", ex ));
            }
            catch ( Exception ex ) when (timeout_cts.IsCancellationRequested)
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

        internal static string TrimFromBegin( this string s, int maxLength ) => ((maxLength < s.Length) ? s.Substring( s.Length - maxLength ) : s);

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

        [M(O.AggressiveInlining)] public static Task WriteAsync( this FileStream fs, byte[] buffer, CancellationToken ct ) => fs.WriteAsync( buffer, 0, buffer.Length, ct );
        //[M(O.AggressiveInlining)] public static Task WriteAsync( this FileStream fs, byte[] buffer, int count, CancellationToken ct ) => fs.WriteAsync( buffer, 0, count, ct );
        [M(O.AggressiveInlining)] public static Task WriteAsync( this FileStream fs, in (byte[] buffer, int count) t, CancellationToken ct ) => fs.WriteAsync( t.buffer, 0, t.count, ct );
        [M(O.AggressiveInlining)] public static Task WriteAsync( this FileStream fs, string s, CancellationToken ct )
        {
            var bytes = Encoding.UTF8.GetBytes( s );
            return (fs.WriteAsync( bytes, 0, bytes.Length, ct ));
        }
    }
}