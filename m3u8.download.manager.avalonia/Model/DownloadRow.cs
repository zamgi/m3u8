using System;
using System.ComponentModel;
using System.IO;
using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;

namespace m3u8.download.manager.models
{
    /// <summary>
    /// 
    /// </summary>
    public enum DownloadStatus
    {
        Created,
        Started,
        Running,
        Wait,
        Paused,
        Canceled,
        Finished,
        Error,
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class DownloadRow : RowBase< DownloadRow >, INotifyPropertyChanged
    {
        private TimeSpan _FinitaElapsed;
        private long     _DownloadBytesLength_BeforeRunning;

        public event PropertyChangedEventHandler PropertyChanged;

        [M(O.AggressiveInlining)] private void Fire_PropertyChanged_Events( string propertyName )
            => PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );

        internal DownloadRow( in (string Url, string OutputFileName, string OutputDirectory) t, DownloadListModel model ) : base( model )
        {
            Status                   = DownloadStatus.Created;
            CreatedOrStartedDateTime = DateTime.Now;
            Url                      = t.Url;
            OutputFileName           = t.OutputFileName;
            OutputDirectory          = t.OutputDirectory;

            Log = new LogListModel();
        }

        public DateTime       CreatedOrStartedDateTime    { [M(O.AggressiveInlining)] get; private set; }
        public string         Url                         { [M(O.AggressiveInlining)] get; private set; }
        public string         OutputFileName              { [M(O.AggressiveInlining)] get; private set; }
        public string         OutputDirectory             { [M(O.AggressiveInlining)] get; private set; }
        public string         VeryFirstOutputFullFileName { [M(O.AggressiveInlining)] get; private set; }

        public int            TotalParts                  { [M(O.AggressiveInlining)] get; private set; }
        public int            SuccessDownloadParts        { [M(O.AggressiveInlining)] get; private set; }
        public int            FailedDownloadParts         { [M(O.AggressiveInlining)] get; private set; }
        public long           DownloadBytesLength         { [M(O.AggressiveInlining)] get; private set; }
        public DownloadStatus Status                      { [M(O.AggressiveInlining)] get; private set; }

        //--- USING FOR BINDING & UPDATE BINDING---//
        public DownloadRow MySelf { [M(O.AggressiveInlining)] get => this; }

        public LogListModel Log { [M(O.AggressiveInlining)] get; }

        public string   GetOutputFullFileName() => Path.Combine( OutputDirectory, OutputFileName );
        public string[] GetOutputFullFileNames()
        {
            var outputFullFileName = GetOutputFullFileName();
            if ( !VeryFirstOutputFullFileName.IsNullOrEmpty() && (outputFullFileName != VeryFirstOutputFullFileName) )
            {
                return (new[] { VeryFirstOutputFullFileName, outputFullFileName });
            }
            return (new[] { outputFullFileName });
        }
        public void     SaveVeryFirstOutputFullFileName( string outputFullFileName ) => VeryFirstOutputFullFileName = outputFullFileName;

        public void SetOutputFileName ( string outputFileName )
        {
            if ( OutputFileName != outputFileName )
            {
                OutputFileName = outputFileName;
                Fire_PropertyChanged_Events( nameof(OutputFileName) );
            }
        }
        public void SetOutputDirectory( string outputDirectory )
        {
            if ( OutputDirectory != outputDirectory )
            {
                OutputDirectory = outputDirectory;
                Fire_PropertyChanged_Events( nameof(OutputDirectory) );
            }
        }
        public void SetTotalParts( int totalParts )
        {
            if ( (0 < totalParts) && (TotalParts != totalParts) )
            {
                TotalParts = totalParts;
                Fire_PropertyChanged_Events( nameof(MySelf) );
                //---Fire_PropertyChanged_Events( nameof(TotalParts) );
            }
        }

        [M(O.AggressiveInlining)]
        internal void SetDownloadResponseStepParams( in m3u8_processor_v2.ResponseStepActionParams p )
        {
            if ( 0 < p.BytesLength )
            {
                var sdp = Math.Min( TotalParts, p.SuccessReceivedPartCount );
                var fdp = Math.Min( TotalParts, p.FailedReceivedPartCount  );
                if ( (SuccessDownloadParts != sdp) || (FailedDownloadParts != fdp) )
                {
                    SuccessDownloadParts = sdp;
                    FailedDownloadParts  = fdp;
                    DownloadBytesLength += p.BytesLength;

                    Fire_PropertyChanged_Events( nameof(MySelf) );
                    //---Fire_PropertyChanged_Events( "DownloadParts-&-DownloadBytesLength" );                    
                }
            }
        }

        [M(O.AggressiveInlining)]
        public void SetStatus( DownloadStatus newStatus )
        {
            if ( Status != newStatus )
            {
                switch ( newStatus )
                {
                    case DownloadStatus.Started:
                        _DownloadBytesLength_BeforeRunning = this.DownloadBytesLength = 0;
                        CreatedOrStartedDateTime = DateTime.Now;
                    break;

                    case DownloadStatus.Running:
                        _DownloadBytesLength_BeforeRunning = this.DownloadBytesLength;
                        CreatedOrStartedDateTime           = DateTime.Now;
                    break;

                    case DownloadStatus.Canceled:
                    case DownloadStatus.Error:
                    case DownloadStatus.Finished:
                        _FinitaElapsed = (DateTime.Now - CreatedOrStartedDateTime);
                    break;
                }

                Status = newStatus;
                Fire_PropertyChanged_Events( nameof(Status) );
                Fire_PropertyChanged_Events( nameof(MySelf) );
            }
        }

        [M(O.AggressiveInlining)]
        public TimeSpan GetElapsed()
        {
            switch ( Status )
            {
                case DownloadStatus.Canceled:
                case DownloadStatus.Error:
                case DownloadStatus.Finished:
                    return (_FinitaElapsed);

                default:
                    return (DateTime.Now - CreatedOrStartedDateTime);
            }
        }

        [M(O.AggressiveInlining)] 
        public long GetDownloadBytesLengthAfterLastRun() => this.DownloadBytesLength - _DownloadBytesLength_BeforeRunning;
#if DEBUG
        public override string ToString() => Status.ToString();
#endif
    }
}
