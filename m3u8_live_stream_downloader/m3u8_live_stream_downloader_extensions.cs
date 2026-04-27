using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;

namespace m3u8
{
    /// <summary>
    /// 
    /// </summary>
    internal static partial class m3u8_live_stream_downloader_extensions
    {
        [M(O.AggressiveInlining)] public static bool IsNullOrEmpty( this string s ) => string.IsNullOrEmpty( s );
        [M(O.AggressiveInlining)] public static bool IsNullOrWhiteSpace( this string s ) => string.IsNullOrWhiteSpace( s );

        [M(O.AggressiveInlining)] public static ConfiguredTaskAwaitable< T > CAX< T >( this Task< T > task ) => task.ConfigureAwait( false );
        [M(O.AggressiveInlining)] public static ConfiguredTaskAwaitable CAX( this Task task ) => task.ConfigureAwait( false );
        [M(O.AggressiveInlining)] public static ConfiguredValueTaskAwaitable< T > CAX< T >( this ValueTask< T > task ) => task.ConfigureAwait( false );
        [M(O.AggressiveInlining)] public static ConfiguredValueTaskAwaitable CAX( this ValueTask task ) => task.ConfigureAwait( false );


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
    }
}
