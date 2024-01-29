#define USE_ConcurrentStack_With_Manual_Count

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

#if (USE_ConcurrentStack_With_Manual_Count && NETCOREAPP)
using System.Diagnostics.CodeAnalysis; 
#endif

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
    }

    /// <summary>
    /// 
    /// </summary>
    internal sealed class ObjectPoolDisposable< T > : ObjectPool< T >, IDisposable
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
}