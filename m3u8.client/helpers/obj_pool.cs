#define USE_ConcurrentStack_With_Manual_Count

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;

#if (USE_ConcurrentStack_With_Manual_Count && NETCOREAPP)
using System.Diagnostics.CodeAnalysis;
#endif

using ThreadingTimer = System.Threading.Timer;
using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;

namespace System.Collections.Generic
{
    /// <summary>
    /// 
    /// </summary>
    public interface IObjectHolder< out T > : IDisposable
    {
        T Value { get; }
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IObjectPool< T >
    {
        IObjectHolder< T > GetHolder();
        IObjectHolder< T > GetHolder( out T t );
    }

    /// <summary>
    /// 
    /// </summary>
    public class ObjectPool< T > : IObjectPool< T >, IDisposable
        where T : class
    {
#if USE_ConcurrentStack_With_Manual_Count
        /// <summary>
        /// 
        /// </summary>
        private sealed class ConcurrentStack_WithManualCount< X >
        {
            private int _Manual_Count;
            private  ConcurrentStack< X > _Stack;
            public ConcurrentStack_WithManualCount() => _Stack = new ConcurrentStack< X >();

            public int Count => Volatile.Read( ref _Manual_Count );
            public void Push( X x )
            {
                Interlocked.Increment( ref _Manual_Count );
                _Stack.Push( x );
            }
#if NETCOREAPP
            public bool TryPop( [MaybeNullWhen(false)] out X x )
#else
            public bool TryPop( out X x )
#endif
            {
                if ( _Stack.TryPop( out x ) )
                {
                    Interlocked.Decrement( ref _Manual_Count );
                    return (true);
                }
                return (false);
            }
            public void Clear()
            {
                Interlocked.Exchange( ref _Manual_Count, 0 );
                _Stack.Clear();
            }
            public X[] ToArray() => _Stack.ToArray();

            public override string ToString() => $"Stack.Count = {_Stack.Count}, Manual_Count= {_Manual_Count}";
        }

        private ConcurrentStack_WithManualCount< T > _Stack;
#else
        private ConcurrentStack< T > _Stack;
#endif        
        private int       _ObjectInstanceCount;
        private Func< T > _ObjectConstructorFunc;
        public ObjectPool( int objectInstanceCount, Func< T > objectConstructorFunc )
        {
            if ( objectInstanceCount   <= 0    ) throw (new ArgumentException( nameof(objectInstanceCount) ));
            if ( objectConstructorFunc == null ) throw (new ArgumentNullException( nameof(objectConstructorFunc) ));
            //-----------------------------------------------//
#if USE_ConcurrentStack_With_Manual_Count
            _Stack = new ConcurrentStack_WithManualCount< T >();
#else
            _Stack = new ConcurrentStack< T >();
#endif
            for ( var i = 0; i < objectInstanceCount; i++ )
            {
                _Stack.Push( objectConstructorFunc() );
            }
            _ObjectInstanceCount   = objectInstanceCount;
            _ObjectConstructorFunc = objectConstructorFunc;
        }
        public void Dispose()
        {
            lock ( _Stack ) 
            {
                DisposeInternal();
                _Stack.Clear();
            }
        }

        [M(O.AggressiveInlining)] private int Get_ObjectInstanceCount() => Volatile.Read( ref _ObjectInstanceCount )/*_ObjectInstanceCount*/;

        protected virtual void DisposeInternal() { }
        protected virtual void DisposeInternalT( T t ) { }
        protected IReadOnlyCollection< T > GetObjects() => _Stack.ToArray();

        /// <summary>
        /// 
        /// </summary>
        private struct Releaser : IObjectHolder< T >, IDisposable
        {
            private ObjectPool< T > _ObjectPool;
            [M(O.AggressiveInlining)] public Releaser( ObjectPool< T > objectPool, T t ) => (_ObjectPool, Value) = (objectPool, t);
            public void Dispose()
            {
                if ( Value != null )
                {
                    _ObjectPool.Release( Value );
                    Value = null;
                }
            }
            public T Value { get; private set; }
        }

        [M(O.AggressiveInlining)] public T Get()
        {
            if ( !_Stack.TryPop( out var t ) )
            {
                t = _ObjectConstructorFunc();
            }
            return (t);
        }
        [M(O.AggressiveInlining)] public void Release( T t )
        {            
            Debug.Assert( t != null );

            lock ( _Stack )
            {
                if ( _Stack.Count < Get_ObjectInstanceCount() )
                {
                    _Stack.Push( t );
                }
                else
                {
                    DisposeInternalT( t );
                }
            }
        }

        public IObjectHolder< T > GetHolder() => new Releaser( this, Get() );
        public IObjectHolder< T > GetHolder( out T t ) => new Releaser( this, t = Get() );

        public int CurrentCount_Stack       => _Stack.Count;
        public int CurrentManualCount_Stack => _Stack.Count;
        public int ObjectInstanceCount      => Get_ObjectInstanceCount();

        public void ChangeCapacity( int objInstCnt )
        {
            objInstCnt = Math.Max( 1, objInstCnt );
            if ( Get_ObjectInstanceCount() != objInstCnt )
            {
                Interlocked.Exchange( ref _ObjectInstanceCount, objInstCnt );
            }
        }

        public override string ToString() => $"MAX = {_ObjectInstanceCount}, Count = {_Stack.Count}";
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class ObjectPoolDisposable< T > : ObjectPool< T >, IDisposable
        where T : class, IDisposable
    {
        public ObjectPoolDisposable( int objectInstanceCount, Func< T > objectConstructorFunc ) : base( objectInstanceCount, objectConstructorFunc ) { }
        protected override void DisposeInternal()
        {
            foreach ( var t in base.GetObjects() )
            {
                t.Dispose();
            }
        }
        protected override void DisposeInternalT( T t ) => t.Dispose();
    }


    /// <summary>
    /// 
    /// </summary>
    public sealed class CtsTimerPool : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly struct Tuple : IDisposable
        {
            required public CancellationTokenSource Cts { get; init; }
            required public ThreadingTimer Timer { get; init; }

            public void Dispose()
            {
                Cts.Dispose();
                Timer.Dispose();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private /*readonly*/ struct Releaser : IObjectHolder< CancellationTokenSource >, IDisposable
        {
            private readonly CtsTimerPool _Pool;
            private /*readonly*/ Tuple _Tuple;
            [M(O.AggressiveInlining)] public Releaser( CtsTimerPool pool, in Tuple t ) => (_Pool, _Tuple) = (pool, t);
            public void Dispose()
            {
                if ( _Tuple.Cts != null )
                {
                    _Pool.Release( _Tuple );
                    _Tuple = default;
                }
            }
            public CancellationTokenSource Value { [M(O.AggressiveInlining)] get => _Tuple.Cts; }
        }

        private readonly ConcurrentQueue< Tuple > _Queue;
        private int _MaxSize;
        private bool _IsDisposed;

        public CtsTimerPool( int initialSize ) //, int maxSize = /*256*/int.MaxValue )
        {
            _Queue = new ConcurrentQueue< Tuple >();

            _MaxSize = initialSize;
            for ( var i = 0; i < initialSize; i++ )
            {
                var cts = new CancellationTokenSource();
                // Создаём таймер с пустым callback — его мы подменим при выдаче
                var timer = new ThreadingTimer( TimerCallback, cts, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan );
                _Queue.Enqueue( new Tuple() { Cts = cts, Timer = timer } );
            }
        }
        public void Dispose()
        {
            _IsDisposed = true;
            while ( _Queue.TryDequeue( out var t ) )
            {
                t.Cts.Dispose();
                t.Timer.Dispose();
            }
        }

        public void ChangeCapacity( int maxSize )
        {
            maxSize = Math.Max( 1, maxSize );
            if ( GetMaxSize() != maxSize )
            {
                Interlocked.Exchange( ref _MaxSize, maxSize );
            }
        }
        public int MaxSize => GetMaxSize();
        [M(O.AggressiveInlining)] private int GetMaxSize() => Volatile.Read( ref _MaxSize )/*_MaxSize*/;

        public IObjectHolder< CancellationTokenSource > Acquire( TimeSpan timeout, out CancellationTokenSource cts )
        {
            if ( _IsDisposed ) throw (new ObjectDisposedException( nameof(CtsTimerPool) ));

            Releaser r;
            while ( _Queue.TryDequeue( out var t ) )
            {
                if ( !t.Cts.IsCancellationRequested )
                {
                    cts = t.Cts;
                    r   = new Releaser( this, t );
                    t.Timer.Change( timeout, Timeout.InfiniteTimeSpan );
                    // Нашли живой экземпляр — возвращаем
                    return (r);
                }

                // Мёртвый экземпляр — утилизируем и пробуем дальше
                t.Dispose();
            }

            // Пул пуст или все экземпляры мёртвые — создаём свежие            
            var freshCts   = new CancellationTokenSource();
            var freshTimer = new ThreadingTimer( TimerCallback, freshCts, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan );
            cts = freshCts;
            r   = new Releaser( this, new Tuple() { Cts = freshCts, Timer = freshTimer } );
            freshTimer.Change( timeout, Timeout.InfiniteTimeSpan );
            return (r);
        }

        /// <summary>
        /// Возвращает пару в пул. Если CTS отменён (таймаут сработал) — утилизирует.
        /// </summary>
        private void Release( in Tuple t )
        {
            if ( _IsDisposed || t.Cts == null ) return;

            // Если таймаут уже сработал — CTS испорчен, не возвращаем в пул
            if ( t.Cts.IsCancellationRequested )
            {
                t.Dispose();
                return;
            }

            // Иначе возвращаем, если есть место
            if ( _Queue.Count < GetMaxSize() )
            {
                t.Timer.Change( Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan );
                _Queue.Enqueue( t );
            }
            else
            {
                // Пул полон — лишний экземпляр утилизируем
                t.Dispose();
            }
        }

        private static void TimerCallback( object state )
        {
            Debug.Assert( (state is CancellationTokenSource _cts) && !_cts.IsCancellationRequested );

            var cts = (CancellationTokenSource) state;
            cts.Cancel();
        }
    }

}