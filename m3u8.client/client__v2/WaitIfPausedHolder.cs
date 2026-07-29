using System;
using System.Threading;

using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;

namespace m3u8.client__v2
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class CancellationTokenSourceWrapper : IDisposable
    {
        private CancellationTokenSource _Cts;
        public CancellationTokenSourceWrapper() => _Cts = new CancellationTokenSource();
        public void Dispose() => _Cts.Dispose();

        [M(O.AggressiveInlining)] public void Cancel() => _Cts.Cancel();
        [M(O.AggressiveInlining)] public void Reset()
        {
            //var suc = _Cts.TryReset();
            //if ( !suc ) { _Cts.Dispose(); _Cts = new CancellationTokenSource(); }
            _Cts.Dispose(); 
            _Cts = new CancellationTokenSource();
        }
        public CancellationToken Token { [M(O.AggressiveInlining)] get => _Cts.Token; }
        public bool IsCancellationRequested { [M(O.AggressiveInlining)] get => _Cts.IsCancellationRequested; }

        public override string ToString() => _Cts.ToString();
    }

    /// <summary>
    /// 
    /// </summary>
    internal sealed class WaitIfPausedEventWrapper : IDisposable
    {
        private ManualResetEventSlim _Event;
        private CancellationTokenSourceWrapper _TokenSource;
        public WaitIfPausedEventWrapper()
        {
            _Event = new ManualResetEventSlim( true, 0 );
            _TokenSource = new CancellationTokenSourceWrapper();
        }
        public void Dispose()
        {            
            _Event.Dispose();
            _TokenSource.Dispose();
        }

        public CancellationToken Token { [M(O.AggressiveInlining)] get => _TokenSource.Token; }
        public bool IsNeedWait { [M(O.AggressiveInlining)] get => !_Event.IsSet; }
        [M(O.AggressiveInlining)] public void SetNeedWait()
        {
            _Event.Reset();
            _TokenSource.Cancel();
        }
        [M(O.AggressiveInlining)] public void ResetNeedWait()
        {
            _TokenSource.Reset();
            _Event.Set();
        }
        [M(O.AggressiveInlining)] public void Wait( CancellationToken ct ) => _Event.Wait( ct );

        public override string ToString() => $"IsNeedWait = {IsNeedWait}, (Token.IsCancellationRequested = {_TokenSource.IsCancellationRequested})";
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class WaitIfPausedHolder
    {
        private WaitIfPausedEventWrapper _EventWrapper;
        private Action                   _BeforeWaitAction;
        private Action                   _AfterWaitAction;
        private Action< m3u8_part_ts > _BeforeWaitAction_4_Parts;
        private Action< m3u8_part_ts > _AfterWaitAction_4_Parts;

        internal WaitIfPausedHolder( WaitIfPausedEventWrapper eventWrapper ) => _EventWrapper = eventWrapper ?? throw (new ArgumentNullException( nameof(eventWrapper) ));
        internal WaitIfPausedHolder( WaitIfPausedEventWrapper eventWrapper, Action beforeWaitAction, Action afterWaitAction ) : this( eventWrapper )
        {
            _BeforeWaitAction = beforeWaitAction;
            _AfterWaitAction  = afterWaitAction;
        }
        internal WaitIfPausedHolder( WaitIfPausedEventWrapper   eventWrapper
                                   , Action< m3u8_part_ts > beforeWaitAction
                                   , Action< m3u8_part_ts > afterWaitAction ) : this( eventWrapper )
        {
            _BeforeWaitAction_4_Parts = beforeWaitAction;
            _AfterWaitAction_4_Parts  = afterWaitAction;
        }

        public CancellationToken Token { [M(O.AggressiveInlining)] get => _EventWrapper.Token; }
        public bool IsNeedWait { [M(O.AggressiveInlining)] get => _EventWrapper.IsNeedWait; }
        //[M(O.AggressiveInlining)] public void SetNeedWait() => _EventWrapper.SetNeedWait();
        //[M(O.AggressiveInlining)] public void ResetNeedWait() => _EventWrapper.ResetNeedWait();
        [M(O.AggressiveInlining)] public void Wait_WithCallbacks( in m3u8_part_ts part, CancellationToken ct )
        {
            _BeforeWaitAction_4_Parts?.Invoke( part );
            _EventWrapper.Wait( ct );
            _AfterWaitAction_4_Parts?.Invoke( part );
        }
        [M(O.AggressiveInlining)] public void Wait_WithCallbacks( CancellationToken ct )
        {
            _BeforeWaitAction?.Invoke();
            _EventWrapper.Wait( ct );
            _AfterWaitAction?.Invoke();
        }
        [M(O.AggressiveInlining)] public void Wait_NoCallbacks( CancellationToken ct ) => _EventWrapper.Wait( ct );

        public override string ToString() => _EventWrapper.ToString();
    }
}
