using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;

namespace System.Threading
{
    /// <summary>
    /// SemaphoreSlim based
    /// </summary>
    public struct AsyncCriticalSection : IDisposable
    {
        private SemaphoreSlim _Smpr;
        [M( O.AggressiveInlining )] public static AsyncCriticalSection Create() => new AsyncCriticalSection() { _Smpr = new SemaphoreSlim( 1, 1 ) };
        [M( O.AggressiveInlining )] public static AsyncCriticalSection CreateAndEnter()
        {
            var o = Create();
            o.Enter();
            return (o);
        }
        [M( O.AggressiveInlining )] public void Dispose()
        {
            if ( _Smpr != null )
            {
                _Smpr.Dispose();
                _Smpr = null;
            }
        }

        public bool IsEmpty { [M( O.AggressiveInlining )] get => (_Smpr == null); }

        [M( O.AggressiveInlining )] public void Enter() => _Smpr.Wait();
        [M( O.AggressiveInlining )] public bool TryEnter() => _Smpr.Wait( 0 );

        [M( O.AggressiveInlining )] public Task EnterAsync() => _Smpr.WaitAsync();
        [M( O.AggressiveInlining )] public Task EnterAsync( CancellationToken ct ) => _Smpr.WaitAsync( ct );
        [M( O.AggressiveInlining )] public Task< bool > TryEnterAsync() => _Smpr.WaitAsync( 0 );
        [M( O.AggressiveInlining )] public Task< bool > TryEnterAsync( int millisecondsTimeout ) => _Smpr.WaitAsync( millisecondsTimeout );
        [M( O.AggressiveInlining )] public Task< bool > TryEnterAsync( TimeSpan timeout ) => _Smpr.WaitAsync( timeout );
        [M( O.AggressiveInlining )] public Task< bool > TryEnterAsync( TimeSpan timeout, CancellationToken ct ) => _Smpr.WaitAsync( timeout, ct );
        [M( O.AggressiveInlining )] public Task< bool > TryEnterAsync( int millisecondsTimeout, CancellationToken ct ) => _Smpr.WaitAsync( millisecondsTimeout, ct );

        [M( O.AggressiveInlining )] public void Exit() => _Smpr.Release();
#if DEBUG
        public override string ToString() => (IsEmpty ? "NULL" : _Smpr.CurrentCount.ToString());
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    public static class AsyncCriticalSectionHelper
    {
        /// <summary>
        /// 
        /// </summary>
        private struct ExitHolder : IDisposable
        {
            private AsyncCriticalSection _Acs;
            [M( O.AggressiveInlining )] public ExitHolder( AsyncCriticalSection acs ) => _Acs = acs;
            [M( O.AggressiveInlining )] public void Dispose()
            {
                if ( !_Acs.IsEmpty )
                {
                    _Acs.Exit();
                    _Acs = default;
                }
            }
        }

        [M( O.AggressiveInlining )] public static IDisposable UseEnter( this AsyncCriticalSection acs )
        {
            acs.Enter();
            return (new ExitHolder( acs ));
        }
        [M( O.AggressiveInlining )] public static async Task<IDisposable> UseEnterAsync( this AsyncCriticalSection acs )
        {
            await acs.EnterAsync().ConfigureAwait( false );
            return (new ExitHolder( acs ));
        }
    }
    //----------------------------------------------------//

    /// <summary>
    /// SemaphoreSlim based
    /// </summary>
    public struct AsyncWaitEvent : IDisposable
    {
        private SemaphoreSlim _Smpr;
        [M( O.AggressiveInlining )] public static AsyncWaitEvent Create() => new AsyncWaitEvent( true );
        [M( O.AggressiveInlining )] public AsyncWaitEvent( bool initialState ) => _Smpr = new SemaphoreSlim( (initialState ? 1 : 0), 1 );
        [M( O.AggressiveInlining )] public void Dispose()
        {
            if ( _Smpr != null )
            {
                _Smpr.Dispose();
                _Smpr = null;
            }
        }

        public bool IsEmpty { [M( O.AggressiveInlining )] get => (_Smpr == null); }

        [M( O.AggressiveInlining )] public void Wait() => _Smpr.Wait();
        [M( O.AggressiveInlining )] public void Wait( CancellationToken ct ) => _Smpr.Wait( ct );
        [M( O.AggressiveInlining )] public bool Wait( TimeSpan timeout ) => _Smpr.Wait( timeout );
        [M( O.AggressiveInlining )] public bool Wait( TimeSpan timeout, CancellationToken ct ) => _Smpr.Wait( timeout, ct );
        [M( O.AggressiveInlining )] public bool Wait( int millisecondsTimeout ) => _Smpr.Wait( millisecondsTimeout );
        [M( O.AggressiveInlining )] public bool Wait( int millisecondsTimeout, CancellationToken ct ) => _Smpr.Wait( millisecondsTimeout, ct );

        [M( O.AggressiveInlining )] public Task WaitAsync() => _Smpr.WaitAsync();
        [M( O.AggressiveInlining )] public Task WaitAsync( CancellationToken ct ) => _Smpr.WaitAsync( ct );
        [M( O.AggressiveInlining )] public Task< bool > WaitAsync( int millisecondsTimeout ) => _Smpr.WaitAsync( millisecondsTimeout );
        [M( O.AggressiveInlining )] public Task< bool > WaitAsync( TimeSpan timeout ) => _Smpr.WaitAsync( timeout );
        [M( O.AggressiveInlining )] public Task< bool > WaitAsync( TimeSpan timeout, CancellationToken ct ) => _Smpr.WaitAsync( timeout, ct );
        [M( O.AggressiveInlining )] public Task< bool > WaitAsync( int millisecondsTimeout, CancellationToken ct ) => _Smpr.WaitAsync( millisecondsTimeout, ct );

        [M( O.AggressiveInlining )] public void Set() => _Smpr.Release();
        [M( O.AggressiveInlining )] public void Release() => _Smpr.Release();
#if DEBUG
        public override string ToString() => (IsEmpty ? "NULL" : _Smpr.CurrentCount.ToString());
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    public static class AsyncWaitEventHelper
    {
        /// <summary>
        /// 
        /// </summary>
        private struct ReleaseHolder : IDisposable
        {
            private AsyncWaitEvent _Awe;
            [M( O.AggressiveInlining )] public ReleaseHolder( AsyncWaitEvent awe ) => _Awe = awe;
            [M( O.AggressiveInlining )] public void Dispose()
            {
                if ( !_Awe.IsEmpty )
                {
                    _Awe.Release();
                    _Awe = default;
                }
            }
        }

        [M( O.AggressiveInlining )] public static async Task<IDisposable> UseWaitAsync( this AsyncWaitEvent awe )
        {
            await awe.WaitAsync().ConfigureAwait( false );
            return (new ReleaseHolder( awe ));
        }
    }
    //----------------------------------------------------//

    /// <summary>
    /// 
    /// </summary>
    public static class InterlockedExtension
    {
        public const long TicksPerMillisecond = 10_000;
        public const long TicksPerSecond      = TicksPerMillisecond * 1_000;

        [M(O.AggressiveInlining)] public static bool ExchangeIfNewValueSmaller( ref int target, int newValue )
        {
            int snapshot;
            bool stillLess;
            do
            {
                snapshot = target;
                stillLess = newValue < snapshot;
            } while ( stillLess && Interlocked.CompareExchange( ref target, newValue, snapshot ) != snapshot );

            return stillLess;
        }
        [M(O.AggressiveInlining)] public static bool ExchangeIfNewValueBigger( ref int target, int newValue )
        {
            int snapshot;
            bool stillMore;
            do
            {
                snapshot = target;
                stillMore = newValue > snapshot;
            } while ( stillMore && Interlocked.CompareExchange( ref target, newValue, snapshot ) != snapshot );

            return stillMore;
        }

        [M(O.AggressiveInlining)] public static bool ExchangeIfNewValueSmaller( ref long target, long newValue )
        {
            long snapshot;
            bool new_less;
            do
            {
                snapshot = Interlocked.Read( ref target );
                new_less = (newValue < snapshot);
            }
            while ( new_less && (Interlocked.CompareExchange( ref target, newValue, snapshot ) != snapshot) );

            return (new_less);
        }
        [M(O.AggressiveInlining)] public static bool ExchangeIfNewValueBigger( ref long target, long newValue )
        {
            long snapshot;
            bool new_more;
            do
            {
                snapshot = Interlocked.Read( ref target );
                new_more = (newValue > snapshot);
            } 
            while ( new_more && (Interlocked.CompareExchange( ref target, newValue, snapshot ) != snapshot) );

            return (new_more);
        }
        [M(O.AggressiveInlining)] public static bool ExchangeIfNewValueBigger( ref long target, long newValue4Replace, long newValue4Compare )
        {
            long snapshot;
            bool new_more;
            do
            {
                snapshot = Interlocked.Read( ref target );
                new_more = (snapshot < newValue4Compare);
            }
            while ( new_more && (Interlocked.CompareExchange( ref target, newValue4Replace, snapshot ) != snapshot) );

            return (new_more);
        }
    }
}

namespace System.Threading.Tasks
{
    /// <summary>
    ///
    /// </summary>
    public static class ParallelTaskExtensions
    {
        public static async Task ForEachAsync< T >( this IReadOnlyCollection< T > seq
            , int maxDegreeOfParallelism
            , CancellationToken ct
            , Func< T, CancellationToken, Task > seqItemFunc
            ) 
        {
            if ( (seq == null) || !seq.Any() ) return;
            if ( maxDegreeOfParallelism <= 0 ) throw (new ArgumentException( nameof(maxDegreeOfParallelism) ));
            if ( seqItemFunc == null )         throw (new ArgumentException( nameof(seqItemFunc) ));
            //-----------------------------------------------------------//

            using ( var semaphore   = new SemaphoreSlim( maxDegreeOfParallelism, maxDegreeOfParallelism ) )
            using ( var finitaEvent = new AsyncWaitEvent( false ) )
            {
                var totalSeqCount     = seq.Count;
                var processedSeqCount = 0;

                foreach ( var t in seq )
                {
                    await semaphore.WaitAsync( ct ).ConfigureAwait( false );
#pragma warning disable CS4014
                    //Because this call is not awaited, execution of the current method continues before the call is completed. 
                    //Consider applying the 'await' operator to the result of the call.
                    
                    var local_t = t;
                    Task.Run( async () =>
                    {
                        try
                        {
                            await seqItemFunc( local_t, ct ).ConfigureAwait( false );
                        }
                        finally
                        {
                            semaphore.Release();

                            if ( Interlocked.Increment( ref processedSeqCount ) == totalSeqCount )
                            {
                                finitaEvent.Set();
                            }
                        }
                    });
#pragma warning restore CS4014

                    ct.ThrowIfCancellationRequested();
                }

                await finitaEvent.WaitAsync( ct ).ConfigureAwait( false );
            }
        }        

        public static async Task ForEachAsync< T >( this ICollection< T > seq
            , int maxDegreeOfParallelism
            , CancellationToken ct
            , Func< T, CancellationToken, Task > seqItemFunc
            ) 
        {
            if ( (seq == null) || !seq.Any() ) return;
            if ( maxDegreeOfParallelism <= 0 ) throw (new ArgumentException( nameof(maxDegreeOfParallelism) ));
            if ( seqItemFunc == null )         throw (new ArgumentException( nameof(seqItemFunc) ));
            //-----------------------------------------------------------//

            using ( var semaphore   = new SemaphoreSlim( maxDegreeOfParallelism, maxDegreeOfParallelism ) )
            using ( var finitaEvent = new AsyncWaitEvent( false ) )
            {
                var totalSeqCount     = seq.Count;
                var processedSeqCount = 0;

                foreach ( var t in seq )
                {
                    await semaphore.WaitAsync( ct ).ConfigureAwait( false );
#pragma warning disable CS4014
                    //Because this call is not awaited, execution of the current method continues before the call is completed. 
                    //Consider applying the 'await' operator to the result of the call.

                    var local_t = t;
                    Task.Run( async () =>
                    {
                        try
                        {
                            await seqItemFunc( local_t, ct ).ConfigureAwait( false );
                        }
                        finally
                        {
                            semaphore.Release();

                            if ( Interlocked.Increment( ref processedSeqCount ) == totalSeqCount )
                            {
                                finitaEvent.Set();
                            }
                        }
                    });
#pragma warning restore CS4014

                    ct.ThrowIfCancellationRequested();
                }

                await finitaEvent.WaitAsync( ct ).ConfigureAwait( false );
            }
        }        

        public static async Task ForEachAsync< T >( this List< T > seq
            , int maxDegreeOfParallelism
            , CancellationToken ct
            , Func< T, CancellationToken, Task > seqItemFunc
            ) 
        {
            if ( (seq == null) || !seq.Any() ) return;
            if ( maxDegreeOfParallelism <= 0 ) throw (new ArgumentException( nameof(maxDegreeOfParallelism) ));
            if ( seqItemFunc == null )         throw (new ArgumentException( nameof(seqItemFunc) ));
            //-----------------------------------------------------------//
            
            using ( var semaphore   = new SemaphoreSlim( maxDegreeOfParallelism, maxDegreeOfParallelism ) )
            using ( var finitaEvent = new AsyncWaitEvent( false ) )
            {
                var totalSeqCount     = seq.Count;
                var processedSeqCount = 0;

                foreach ( var t in seq )
                {
                    await semaphore.WaitAsync( ct ).ConfigureAwait( false );
#pragma warning disable CS4014
                    //Because this call is not awaited, execution of the current method continues before the call is completed. 
                    //Consider applying the 'await' operator to the result of the call.

                    var local_t = t;
                    Task.Run( async () =>
                    {
                        try
                        {
                            await seqItemFunc( local_t, ct ).ConfigureAwait( false );
                        }
                        finally
                        {
                            semaphore.Release();
                            //try
                            //{
                            //    semaphore.Release();
                            //}
                            //catch ( Exception ex )
                            //{
                            //    if ( Debugger.IsAttached ) Debugger.Break();
                            //    Debug.WriteLine( ex );
                            //}

                            if ( Interlocked.Increment( ref processedSeqCount ) == totalSeqCount )
                            {
                                finitaEvent.Set();
                            }
                        }
                    });
#pragma warning restore CS4014

                    ct.ThrowIfCancellationRequested();
                }

                await finitaEvent.WaitAsync( ct ).ConfigureAwait( false );
            }
        }        

        public static async Task ForEachAsync< T >( this T[] seq
            , int maxDegreeOfParallelism
            , CancellationToken ct
            , Func< T, CancellationToken, Task > seqItemFunc
            ) 
        {
            if ( (seq == null) || !seq.Any() ) return;
            if ( maxDegreeOfParallelism <= 0 ) throw (new ArgumentException( nameof(maxDegreeOfParallelism) ));
            if ( seqItemFunc == null )         throw (new ArgumentException( nameof(seqItemFunc) ));
            //-----------------------------------------------------------//
            
            using ( var semaphore   = new SemaphoreSlim( maxDegreeOfParallelism, maxDegreeOfParallelism ) )
            using ( var finitaEvent = new AsyncWaitEvent( false ) )
            {
                var totalSeqCount     = seq.Length;
                var processedSeqCount = 0;

                foreach ( var t in seq )
                {
                    await semaphore.WaitAsync( ct ).ConfigureAwait( false );
#pragma warning disable CS4014
                    //Because this call is not awaited, execution of the current method continues before the call is completed. 
                    //Consider applying the 'await' operator to the result of the call.

                    var local_t = t;
                    Task.Run( async () =>
                    {
                        try
                        {
                            await seqItemFunc( local_t, ct ).ConfigureAwait( false );
                        }
                        finally
                        {
                            semaphore.Release();

                            if ( Interlocked.Increment( ref processedSeqCount ) == totalSeqCount )
                            {
                                finitaEvent.Set();
                            }
                        }
                    });
#pragma warning restore CS4014

                    ct.ThrowIfCancellationRequested();
                }

                await finitaEvent.WaitAsync( ct ).ConfigureAwait( false );
            }
        }        

        public static async Task ForEachAsync< T >( this IEnumerable< T > seq
            , int maxDegreeOfParallelism
            , CancellationToken ct
            , Func< T, CancellationToken, Task > seqItemFunc
            )
        {
            if ( seq == null ) return;
            if ( maxDegreeOfParallelism <= 0 ) throw (new ArgumentException( nameof(maxDegreeOfParallelism) ));
            if ( seqItemFunc == null )         throw (new ArgumentException( nameof(seqItemFunc) ));
            //-----------------------------------------------------------//

            using ( var e = seq.GetEnumerator() )
            {
                if ( !e.MoveNext() )
                {
                    return;
                }
                
                using ( var semaphore   = new SemaphoreSlim( maxDegreeOfParallelism, maxDegreeOfParallelism ) )
                using ( var finitaEvent = new AsyncWaitEvent( false ) )
                {
                    var seqIsFinished   = false;
                    var enqueueSeqCount = 0;
                    for ( var t = e.Current; ; t = e.Current )
                    {
                        if ( !e.MoveNext() )
                        {
                            seqIsFinished = true;
                        }

                        Interlocked.Increment( ref enqueueSeqCount );
                        await semaphore.WaitAsync( ct ).ConfigureAwait( false );
#pragma warning disable CS4014
                        //Because this call is not awaited, execution of the current method continues before the call is completed. 
                        //Consider applying the 'await' operator to the result of the call.

                        var local_t = t;
                        Task.Run( async () =>
                        {
                            try
                            {
                                await seqItemFunc( local_t, ct ).ConfigureAwait( false );
                            }
                            finally
                            {
                                semaphore.Release();

                                if ( (Interlocked.Decrement( ref enqueueSeqCount ) == 0) && seqIsFinished )
                                {
                                    finitaEvent.Set();
                                }
                            }
                        });
#pragma warning restore CS4014

                        if ( seqIsFinished )
                        {
                            break;
                        }

                        ct.ThrowIfCancellationRequested();
                    }

                    await finitaEvent.WaitAsync( ct ).ConfigureAwait( false );
                }
            }
        }
    }
}
