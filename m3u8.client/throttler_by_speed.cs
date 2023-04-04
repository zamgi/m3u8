using System;
using System.Threading;
using System.Threading.Tasks;

namespace m3u8
{
    /// <summary>
    /// 
    /// </summary>
    internal interface I_throttler_by_speed_t : IDisposable
    {
        double? GetMaxSpeedThreshold();
        void ChangeMaxSpeedThreshold( double? max_speed_threshold_in_Mbps );
        void Start( Task task );
        void Restart( Task task );
        public void End( Task task );
        double? Throttle( Task task, CancellationToken ct );        
        void TakeIntoAccountDownloadedBytes( Task task, int downloadedBytes );
    }
    //-----------------------------------------------------------//

    /// <summary>
    /// 
    /// </summary>
    internal interface I_ThrottlerBySpeed_InDownloadProcessUser : IDisposable
    {
        //void Start();
        double? Throttle( CancellationToken ct );
        void TakeIntoAccountDownloadedBytes( int downloadedBytes );
        void Restart();
        void End();
    }

    /// <summary>
    /// 
    /// </summary>
    internal sealed class No_ThrottlerBySpeed_InDownloadProcessUser : I_ThrottlerBySpeed_InDownloadProcessUser
    {
        public static No_ThrottlerBySpeed_InDownloadProcessUser Inst { get; } = new No_ThrottlerBySpeed_InDownloadProcessUser();
        private No_ThrottlerBySpeed_InDownloadProcessUser() { }
        public void Dispose() { }
        //public void Start() { }
        public void Restart() { }
        public void End() { }
        public void TakeIntoAccountDownloadedBytes( int downloadedBytes ) { }
        public double? Throttle( CancellationToken ct ) => null;
    }

    /// <summary>
    /// 
    /// </summary>
    internal sealed class ThrottlerBySpeed_InDownloadProcessUser : I_ThrottlerBySpeed_InDownloadProcessUser
    {
        private I_throttler_by_speed_t _ThrottlerBySpeed;
        private Task _MarkerTask;
        private ThrottlerBySpeed_InDownloadProcessUser( I_throttler_by_speed_t throttlerBySpeed )
        {
            _ThrottlerBySpeed = throttlerBySpeed ?? throw (new ArgumentNullException( nameof(throttlerBySpeed) ));

            _MarkerTask = Task.Run(() => { });
            _ThrottlerBySpeed.Start( _MarkerTask );
        }

        public void Dispose()
        {
            if ( _ThrottlerBySpeed != null )
            {
                _ThrottlerBySpeed.End( _MarkerTask );
                _ThrottlerBySpeed = null;
            }
        }

        public double? Throttle( CancellationToken ct ) => _ThrottlerBySpeed.Throttle( _MarkerTask, ct );
        public void TakeIntoAccountDownloadedBytes( int downloadedBytes ) => _ThrottlerBySpeed.TakeIntoAccountDownloadedBytes( _MarkerTask, downloadedBytes );
        public void Restart() => _ThrottlerBySpeed.Restart( _MarkerTask );
        public void End() => Dispose();

        public static I_ThrottlerBySpeed_InDownloadProcessUser Start( I_throttler_by_speed_t throttlerBySpeed )
            => (throttlerBySpeed != null) ? new ThrottlerBySpeed_InDownloadProcessUser( throttlerBySpeed ) : No_ThrottlerBySpeed_InDownloadProcessUser.Inst;
    }
}
