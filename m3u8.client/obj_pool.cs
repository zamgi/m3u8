using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;

namespace m3u8
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
    internal class ObjectPool< T > : IDisposable
        where T : class
    {
        private SemaphoreSlim        _Semaphore;
        private ConcurrentStack< T > _Stack;
        private int                  _ObjectInstanceCount;
        private Func< T >            _ObjectConstructorFunc;
        public ObjectPool( int objectInstanceCount, Func< T > objectConstructorFunc )
        {
            if ( objectInstanceCount   <= 0    ) throw (new ArgumentException( nameof(objectInstanceCount) ));
            if ( objectConstructorFunc == null ) throw (new ArgumentNullException( nameof(objectConstructorFunc) ));
            //-----------------------------------------------//

            _Semaphore = new SemaphoreSlim( objectInstanceCount, objectInstanceCount );
            _Stack     = new ConcurrentStack< T >();
            for ( var i = 0; i < objectInstanceCount; i++ )
            {
                _Stack.Push( objectConstructorFunc() );
            }
            _ObjectInstanceCount   = objectInstanceCount;
            _ObjectConstructorFunc = objectConstructorFunc;
        }
        ~ObjectPool() => Dispose();
        public void Dispose()
        {
            GC.SuppressFinalize( this );
            if ( _Semaphore != null )
            {
                _Semaphore.Dispose();
                _Semaphore = null;
            }

            DisposeInternal();

            _Stack.Clear();
        }

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

        [M(O.AggressiveInlining)] public T Get( CancellationToken ct = default )
        {
            _Semaphore.Wait( ct );

            for (; ; )
            {
                if ( _Stack.TryPop( out var t ) )
                {
                    return (t);
                }
            }
        }
        [M(O.AggressiveInlining)] public async Task< T > GetAsync( CancellationToken ct = default )
        {
            await _Semaphore.WaitAsync( ct ).ConfigureAwait( false );

            for (; ; )
            {
                if ( _Stack.TryPop( out var t ) )
                {
                    return (t);
                }
            }
        }
        [M(O.AggressiveInlining)] public void Release( T t )
        {
            Debug.Assert( t != null );
            //if ( t != null )
            //{
                _Stack.Push( t );
                _Semaphore.Release();
            //}            
        }

        public IObjectHolder< T > GetHolder( CancellationToken ct = default )
        {
            _Semaphore.Wait( ct );

            for (; ; )
            {
                if ( _Stack.TryPop( out var t ) )
                {
                    return (new Releaser( this, t ));
                }
            }
        }
        public async Task< IObjectHolder< T > > GetHolderAsync( CancellationToken ct = default )
        {
            await _Semaphore.WaitAsync( ct ).ConfigureAwait( false );

            for (; ; )
            {
                if ( _Stack.TryPop( out var t ) )
                {
                    return (new Releaser( this, t ));
                }
            }
        }

        public int CurrentCount_Semaphore => _Semaphore.CurrentCount;
        public int CurrentCount_Stack     => _Stack.Count;
        public int ObjectInstanceCount    => _ObjectInstanceCount;

        public void ResetDengerous( int objInstCnt, bool collectGarbage = true )
        {
            if ( _ObjectInstanceCount == objInstCnt ) return;
            
            if ( _Stack.Count < objInstCnt )
            {
                for (; _Stack.Count < objInstCnt; )
                {
                    _Stack.Push( _ObjectConstructorFunc() );
                }
            }
            else if ( objInstCnt < _Stack.Count )
            {
                for ( ; objInstCnt < _Stack.Count; )
                {
                    if ( _Stack.TryPop( out var t ) )
                    {
                        DisposeInternalT( t );
                    }
                }

                if ( collectGarbage )
                {
                    GC.Collect( GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true );
                    GC.WaitForPendingFinalizers();
                    GC.Collect( GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true );
                }
            }

            using var semaphore_disp = _Semaphore;
            Interlocked.Exchange( ref _Semaphore, new SemaphoreSlim( objInstCnt, objInstCnt ) );
            Interlocked.Exchange( ref _ObjectInstanceCount, objInstCnt );
        }
        /*public async Task< bool > ResetDengerous_WithWait4FullFreeSemaphore( int objInstCnt, int millisecondsDelay = 50, CancellationToken ct = default )
        {
            if ( _ObjectInstanceCount == objInstCnt ) return (true);

            //wait 4 full free semaphore
            for ( var i = 7_000 / Math.Max( 1, millisecondsDelay ); (0 < i) && (_Semaphore.CurrentCount != _ObjectInstanceCount); i-- )
            {
                await Task.Delay( millisecondsDelay, ct ).ConfigureAwait( false );
            }
            var suc = (_Semaphore.CurrentCount == _ObjectInstanceCount);
            if ( suc )
            {
                ResetDengerous( objInstCnt );
            }
            return (suc);
        }
        //*/
    }

    /// <summary>
    /// 
    /// </summary>
    internal sealed class ObjectPoolDisposable< T > : ObjectPool< T >, IDisposable
        where T : class, IDisposable
    {
        public ObjectPoolDisposable( int objectInstanceCount, Func< T > objectConstructorFunc ) : base( objectInstanceCount, objectConstructorFunc ) { }
        ~ObjectPoolDisposable() => Dispose();
        protected override void DisposeInternal()
        {
            foreach ( var t in base.GetObjects() )
            {
                t.Dispose();
            }
        }
        protected override void DisposeInternalT( T t ) => t.Dispose();
    }
}