using System;
using System.Threading;
using System.Threading.Tasks;

namespace m3u8
{
    /// <summary>
    /// 
    /// </summary>
    public interface i_throttler_by_speed_t : IDisposable
    {
        decimal? GetMaxSpeedThreshold();
        void ChangeMaxSpeedThreshold( decimal? max_speed_threshold_in_Mbps );
        void Start();
        void Restart();
        public void End();
        double? Throttle( CancellationToken ct );        
        void TakeIntoAccountDownloadedBytes( int downloadedBytes );
    }
    //-----------------------------------------------------------//

    /// <summary>
    /// 
    /// </summary>
    public interface I_ThrottlerBySpeed_InDownloadProcessUser : IDisposable
    {
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
        private i_throttler_by_speed_t _ThrottlerBySpeed;
        private ThrottlerBySpeed_InDownloadProcessUser( i_throttler_by_speed_t throttlerBySpeed )
        {
            _ThrottlerBySpeed = throttlerBySpeed ?? throw (new ArgumentNullException( nameof(throttlerBySpeed) ));
            _ThrottlerBySpeed.Start();
        }
        public void Dispose()
        {
            if ( _ThrottlerBySpeed != null )
            {
                _ThrottlerBySpeed.End();
                _ThrottlerBySpeed = null;
            }
        }

        public double? Throttle( CancellationToken ct ) => _ThrottlerBySpeed.Throttle( ct );
        public void TakeIntoAccountDownloadedBytes( int downloadedBytes ) => _ThrottlerBySpeed.TakeIntoAccountDownloadedBytes( downloadedBytes );
        public void Restart() => _ThrottlerBySpeed.Restart();
        public void End() => Dispose();

        public static I_ThrottlerBySpeed_InDownloadProcessUser Start( i_throttler_by_speed_t throttlerBySpeed )
            => (throttlerBySpeed != null) ? new ThrottlerBySpeed_InDownloadProcessUser( throttlerBySpeed ) : No_ThrottlerBySpeed_InDownloadProcessUser.Inst;
    }
}
