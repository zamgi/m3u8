using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using _RowPropertiesChanged_ = m3u8.download.manager.models.DownloadListModel.RowPropertiesChangedEventHandler;
using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;

namespace m3u8.download.manager.models
{
    /// <summary>
    /// 
    /// </summary>
    internal enum DownloadStatus
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
    internal sealed class DownloadRow : RowBase< DownloadRow >
    {
        private TimeSpan               _FinitaElapsed;
        private long                   _DownloadBytesLength_BeforeRunning;
        private _RowPropertiesChanged_ _RowPropertiesChanged;

        internal DownloadRow( in (string Url, string OutputFileName, string OutputDirectory) t
            , DownloadListModel model, _RowPropertiesChanged_ rowPropertiesChanged ) : base( model )
        {
            _RowPropertiesChanged = rowPropertiesChanged ?? throw (new ArgumentNullException( nameof(rowPropertiesChanged) ));

            Status                   = DownloadStatus.Created;
            CreatedOrStartedDateTime = DateTime.Now;
            Url                      = t.Url;
            OutputFileName           = t.OutputFileName;
            OutputDirectory          = t.OutputDirectory;

            Log = new LogListModel();
        }
        internal DownloadRow( in (DateTime CreatedOrStartedDateTime, string Url, string OutputFileName, string OutputDirectory, DownloadStatus Status) t
            , DownloadListModel model, _RowPropertiesChanged_ rowPropertiesChanged ) : base( model )
        {
            _RowPropertiesChanged = rowPropertiesChanged ?? throw (new ArgumentNullException( nameof(rowPropertiesChanged) ));

            Status                   = DownloadStatus.Created; //t.Status;
            CreatedOrStartedDateTime = t.CreatedOrStartedDateTime;// DateTime.Now;
            Url                      = t.Url;
            OutputFileName           = t.OutputFileName;
            OutputDirectory          = t.OutputDirectory;

            Log = new LogListModel();
        }
        internal void _Remove_RowPropertiesChangedEventHandler() => _RowPropertiesChanged = null;

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
                _RowPropertiesChanged?.Invoke( this, nameof(OutputFileName) );
            }
        }
        public void SetOutputDirectory( string outputDirectory )
        {
            if ( OutputDirectory != outputDirectory )
            {
                OutputDirectory = outputDirectory;
                _RowPropertiesChanged?.Invoke( this, nameof(OutputDirectory) );
            }
        }
        public void SetTotalParts( int totalParts )
        {
            if ( (0 < totalParts) && (TotalParts != totalParts) )
            {
                TotalParts = totalParts;
                _RowPropertiesChanged?.Invoke( this, nameof(TotalParts) );
            }
        }

        [M(O.AggressiveInlining)] public void SetDownloadResponseStepParams( in m3u8_processor_v2.ResponseStepActionParams p )
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

                    _RowPropertiesChanged?.Invoke( this, "DownloadParts-&-DownloadBytesLength" );
                }
            }
        }
        [M(O.AggressiveInlining)] public void SetStatus( DownloadStatus newStatus )
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
                _RowPropertiesChanged?.Invoke( this, nameof(Status) );
            }
        }
        [M(O.AggressiveInlining)] public TimeSpan GetElapsed()
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
        [M(O.AggressiveInlining)] public long GetDownloadBytesLengthAfterLastRun() => this.DownloadBytesLength - _DownloadBytesLength_BeforeRunning;
#if DEBUG
        public override string ToString() => Status.ToString();
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    internal sealed class DownloadListModel : ListModel< DownloadRow >
    {
        private HashSet< string > _Urls;
        public DownloadListModel() => _Urls = new HashSet< string >( StringComparer.InvariantCultureIgnoreCase );

        public DownloadRow AddRow( in (string Url, string OutputFileName, string OutputDirectory) t )
        {
            var row = base.Add( new DownloadRow( in t, this, base._Fire_RowPropertiesChangedEventHandler ) );
            _Urls.Add( row.Url );
            return (row);
        }
        public void AddRows( IEnumerable< (DateTime CreatedOrStartedDateTime, string Url, string OutputFileName, string OutputDirectory, DownloadStatus Status) > rows )
        {
            foreach ( var t in rows )
            {
                var row = base.Add( new DownloadRow( in t, this, base._Fire_RowPropertiesChangedEventHandler ) );
                _Urls.Add( row.Url );
            }
        }

        [M(O.AggressiveInlining)] public bool HasAnyFinished() => GetAllFinished().Any();
        [M(O.AggressiveInlining)] public IEnumerable< DownloadRow > GetAllFinished() => (from row in GetRows() where (row.Status == DownloadStatus.Finished) select row);

        [M(O.AggressiveInlining)] public bool ContainsUrl( string url ) => (!url.IsNullOrEmpty() && _Urls.Contains( url ));
        public bool RemoveRow( DownloadRow row )
        {
            row?._Remove_RowPropertiesChangedEventHandler();
            var success = base.Remove( row );
            _Urls.Remove( (row?.Url ?? string.Empty) );
            return (success);
        }

        protected override void OnAfterClear() => _Urls.Clear();

        public IReadOnlyDictionary< DownloadStatus, int > GetStatisticsByAllStatus()
        {
            var values = (DownloadStatus[]) Enum.GetValues( typeof(DownloadStatus) );
            var dict = new Dictionary< DownloadStatus, int >( 11 /*values.Length*/ );
            foreach ( var v in values )
            {
                dict[ v ] = 0;
            }
            foreach ( var row in base.GetRows() )
            {
                dict[ row.Status ]++;
            }
            return (dict);
        }
        public bool TryGetSingleRunning( out DownloadRow singleRunningRow )
        {
            singleRunningRow = default;
            foreach ( var row in GetRows() )
            {
                if ( row.Status == DownloadStatus.Running )
                {
                    if ( singleRunningRow != null )
                    {
                        return (false); //not single running row
                    }
                    singleRunningRow = row;
                }
            }
            return (singleRunningRow != null);
        }
    }
}
