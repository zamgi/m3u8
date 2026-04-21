using System;
using System.Collections.Generic;
using System.IO;

//using _m3u8_processor_ = m3u8.m3u8_processor_adv;
using _m3u8_processor_       = m3u8.m3u8_processor_adv__v2;
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
        private TimeSpan               _PausedOrWaitElapsed;
        private long                   _DownloadBytesLength_BeforeRunning;
        private _RowPropertiesChanged_ _RowPropertiesChanged;

        internal DownloadRow( DownloadRow_Definer_1 t //in (string Url, IDictionary< string, string > RequestHeaders, string OutputFileName, string OutputDirectory) t
            , DownloadListModel model, _RowPropertiesChanged_ rowPropertiesChanged ) : base( model )
        {
            _RowPropertiesChanged = rowPropertiesChanged ?? throw (new ArgumentNullException( nameof(rowPropertiesChanged) ));

            Status                   = DownloadStatus.Created;
            CreatedOrStartedDateTime = DateTime.Now;
            Url                      = t.Url;
            RequestHeaders           = t.RequestHeaders;
            WebProxyInfo             = t.WebProxyInfo;
            OutputFileName           = t.OutputFileName;
            OutputDirectory          = t.OutputDirectory;

            Log = new LogListModel();
        }
        internal DownloadRow( DownloadRow_Definer_2 t //in (string Url, IDictionary< string, string > RequestHeaders, string OutputFileName, string OutputDirectory, bool IsLiveStream, long LiveStreamMaxFileSizeInBytes) t
            , DownloadListModel model, _RowPropertiesChanged_ rowPropertiesChanged ) : this( (DownloadRow_Definer_1) t, model, rowPropertiesChanged )
        {
            IsLiveStream                 = t.IsLiveStream;
            LiveStreamMaxFileSizeInBytes = t.LiveStreamMaxFileSizeInBytes;
        }
        internal DownloadRow( DownloadRow_Definer_3 t //in (DateTime CreatedOrStartedDateTime, string Url, IDictionary< string, string > RequestHeaders, string OutputFileName, string OutputDirectory, DownloadStatus Status, bool IsLiveStream, long LiveStreamMaxFileSizeInBytes) t
            , DownloadListModel model, _RowPropertiesChanged_ rowPropertiesChanged ) : this( (DownloadRow_Definer_1) t, model, rowPropertiesChanged )
        {
            //---Status                       = DownloadStatus.Created; //t.Status;
            CreatedOrStartedDateTime     = t.CreatedOrStartedDateTime;// DateTime.Now;
            IsLiveStream                 = t.IsLiveStream;
            LiveStreamMaxFileSizeInBytes = t.LiveStreamMaxFileSizeInBytes;
        }
        private DownloadRow( DownloadRow r, IEnumerable< LogRow > rows = null ) : base( r.Model )
        {
            _RowPropertiesChanged = r._RowPropertiesChanged ?? throw (new ArgumentNullException( nameof(r._RowPropertiesChanged) ));

            Status                             = r.Status;
            CreatedOrStartedDateTime           = r.CreatedOrStartedDateTime;
            Url                                = r.Url;
            OutputFileName                     = r.OutputFileName;
            OutputDirectory                    = r.OutputDirectory;
            IsLiveStream                       = r.IsLiveStream;
            LiveStreamMaxFileSizeInBytes       = r.LiveStreamMaxFileSizeInBytes;
            TotalParts                         = r.TotalParts;
            FailedDownloadParts                = r.FailedDownloadParts;
            SuccessDownloadParts               = r.SuccessDownloadParts;
            DownloadBytesLength                = r.DownloadBytesLength;
            VeryFirstOutputFullFileName        = r.VeryFirstOutputFullFileName;
            _InstantSpeedInMbps                = r._InstantSpeedInMbps;
            _FinitaElapsed                     = r._FinitaElapsed;
            _PausedOrWaitElapsed               = r._PausedOrWaitElapsed;
            _DownloadBytesLength_BeforeRunning = r._DownloadBytesLength_BeforeRunning;
            RequestHeaders                     = r.RequestHeaders;
            WebProxyInfo                       = r.WebProxyInfo;

            //Log = rows.AnyEx() ? new LogListModel( rows ) : new LogListModel( r.Log );
            Log = rows.AnyEx() ? new LogListModel( rows ) : new LogListModel();
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
        private double?       _InstantSpeedInMbps;

        public bool           IsLiveStream                 { [M(O.AggressiveInlining)] get; private set; }
        public long           LiveStreamMaxFileSizeInBytes { [M(O.AggressiveInlining)] get; private set; }
        private DateTime? _CreatedOrStartedDateTime_4_LastPartOfLiveStream;

        public IDictionary< string, string > RequestHeaders { [M(O.AggressiveInlining)] get; private set; }
        public web_proxy_info WebProxyInfo { [M(O.AggressiveInlining)] get; private set; }

        public LogListModel Log { [M(O.AggressiveInlining)] get; }

        public string   GetOutputFullFileName() => Path.Combine( OutputDirectory, OutputFileName );
        public string[] GetOutputFullFileNames()
        {
            var outputFullFileName = GetOutputFullFileName();
            if ( !VeryFirstOutputFullFileName.IsNullOrEmpty() && (outputFullFileName != VeryFirstOutputFullFileName) )
            {
                return ([ VeryFirstOutputFullFileName, outputFullFileName ]);
            }
            return ([ outputFullFileName ]);
        }
        public void     SaveVeryFirstOutputFullFileName( string outputFullFileName ) => VeryFirstOutputFullFileName = outputFullFileName;
        public string   SaveVeryFirstOutputFullFileName() => VeryFirstOutputFullFileName = GetOutputFullFileName();

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
        public void SetLiveStreamMaxFileSizeInBytes( long liveStreamMaxFileSizeInBytes )
        {
            if ( LiveStreamMaxFileSizeInBytes != liveStreamMaxFileSizeInBytes )
            {
                LiveStreamMaxFileSizeInBytes = liveStreamMaxFileSizeInBytes;
                _RowPropertiesChanged?.Invoke( this, nameof(LiveStreamMaxFileSizeInBytes) );
            }
        }
        //---public void SetWebProxyInfo( in web_proxy_info webProxyInfo ) => WebProxyInfo = webProxyInfo;

        public DownloadRow CreateCopy() => new DownloadRow( this );
        public DownloadRow Add2ModelFinishedCopy( DateTime createDateTime, IReadOnlyList< LogRow > logRows, DownloadRow rowSaveState )
        {
            var row = new DownloadRow( this, logRows );
            row.CreatedOrStartedDateTime = createDateTime;
            row.SetStatus( DownloadStatus.Finished );

            row.TotalParts           -= rowSaveState.TotalParts;
            row.SuccessDownloadParts -= rowSaveState.SuccessDownloadParts;
            row.FailedDownloadParts  -= rowSaveState.FailedDownloadParts;
            //row.DownloadBytesLength  -= rowSaveState.DownloadBytesLength;
            row.VeryFirstOutputFullFileName = null;

            ((DownloadListModel) this.Model).AddRow( row );

            return (row);
        }
        public void StartNewPartOfLiveStream( IReadOnlyList< LogRow > logRows4Removing )
        {
            //---Log.Clear();
            Log.RemoveRows( logRows4Removing );
            _CreatedOrStartedDateTime_4_LastPartOfLiveStream = DateTime.Now;
        }

        //public bool Update( string m3u8FileUrl, IDictionary< string, string > requestHeaders, string outputFileName, string outputDirectory, bool isLiveStream, long liveStreamMaxFileSizeInBytes )
        public bool Update( DownloadRow_Definer_2 t )
        {
            var allowed = !Status.IsRunningOrPaused();
            if ( allowed )
            {
                Url             = t.Url;
                RequestHeaders  = t.RequestHeaders;
                WebProxyInfo    = t.WebProxyInfo;
                OutputFileName  = t.OutputFileName;
                OutputDirectory = t.OutputDirectory;
                IsLiveStream    = t.IsLiveStream;
                LiveStreamMaxFileSizeInBytes = t.LiveStreamMaxFileSizeInBytes;
            }
            return (allowed);
        }

        [M(O.AggressiveInlining)] internal void SetDownloadResponseStepParams( in _m3u8_processor_.ResponseStepActionParams p )
        {
            var call__RowPropertiesChanged = false;
            lock ( this )
            {
                //if ( 0 < p.BytesLength )
                //{
                    var sdp = Math.Min( TotalParts, p.SuccessReceivedPartCount );
                    var fdp = Math.Min( TotalParts, p.FailedReceivedPartCount  );
                    if ( (SuccessDownloadParts != sdp) || (FailedDownloadParts != fdp) )
                    {
                        SuccessDownloadParts = sdp;
                        FailedDownloadParts  = fdp;
                        DownloadBytesLength += p.BytesLength;

                        call__RowPropertiesChanged = true;
                    }
                //}

                if ( _InstantSpeedInMbps != p.InstantSpeedInMbps )
                {
                    _InstantSpeedInMbps = p.InstantSpeedInMbps;
                    call__RowPropertiesChanged = true;
                }
            }
            if ( call__RowPropertiesChanged )
            {
                _RowPropertiesChanged?.Invoke( this, "DownloadParts-&-DownloadBytesLength" );
            }
        }
        [M(O.AggressiveInlining)] internal void SetDownloadResponseStepParams( in m3u8_processor_next.ResponseStepActionParams p )
        {
            var call__RowPropertiesChanged = false;
            lock ( this )
            {
                //if ( 0 < p.BytesLength )
                //{
                    var sdp = Math.Min( TotalParts, p.SuccessReceivedPartCount );
                    var fdp = Math.Min( TotalParts, p.FailedReceivedPartCount  );
                    if ( (SuccessDownloadParts != sdp) || (FailedDownloadParts != fdp) )
                    {
                        SuccessDownloadParts = sdp;
                        FailedDownloadParts  = fdp;
                        //--DownloadBytesLength += p.BytesLength;

                        call__RowPropertiesChanged = true;
                    }
                //}

                //if ( _InstantSpeedInMbps != p.InstantSpeedInMbps )
                //{
                //    _InstantSpeedInMbps = p.InstantSpeedInMbps;
                //    call__RowPropertiesChanged = true;
                //}
            }
            if ( call__RowPropertiesChanged )
            {
                _RowPropertiesChanged?.Invoke( this, "DownloadParts-&-DownloadBytesLength" );
            }
        }
        [M(O.AggressiveInlining)] internal void SetDownloadPartStepParams( in m3u8_client_next.DownloadPartStepActionParams p, bool raiseRowPropertiesChangedEvent )
        {
            if ( raiseRowPropertiesChangedEvent )
            {
                var call__RowPropertiesChanged = false;
                lock ( this )
                {
                    if ( 0 < p.BytesReaded )
                    {
                        DownloadBytesLength += p.BytesReaded;
                        call__RowPropertiesChanged = true;
                    }

                    if ( _InstantSpeedInMbps != p.InstantSpeedInMbps )
                    {
                        _InstantSpeedInMbps = p.InstantSpeedInMbps;
                        call__RowPropertiesChanged = true;
                    }
                }
                if ( call__RowPropertiesChanged )
                {
                    _RowPropertiesChanged?.Invoke( this, "DownloadParts-&-DownloadBytesLength" );
                }
            }
            else
            {
                lock ( this )
                {
                    DownloadBytesLength += p.BytesReaded;
                    _InstantSpeedInMbps = p.InstantSpeedInMbps;
                }
            }
        }
        [M(O.AggressiveInlining)] internal void SetDownloadResponseStepParams_Error()
        {
            lock ( this )
            {
                TotalParts++;
                FailedDownloadParts++;
            }
            _RowPropertiesChanged?.Invoke( this, "DownloadParts-&-DownloadBytesLength" );
        }
        [M(O.AggressiveInlining)] internal void SetDownloadResponseStepParams( long part_size_in_bytes, long total_in_bytes, double? instantSpeedInMbps )
        {
            lock ( this )
            {
                TotalParts++;
                SuccessDownloadParts++;
                DownloadBytesLength = total_in_bytes;

                if ( _InstantSpeedInMbps != instantSpeedInMbps ) _InstantSpeedInMbps = instantSpeedInMbps;
            }
            _RowPropertiesChanged?.Invoke( this, "DownloadParts-&-DownloadBytesLength" );
        }
        [M(O.AggressiveInlining)] public void SetStatus( DownloadStatus newStatus )
        {
            var call__RowPropertiesChanged = false;
            lock ( this )
            {
                if ( Status != newStatus )
                {
                    switch ( newStatus )
                    {
                        case DownloadStatus.Started:
                            _DownloadBytesLength_BeforeRunning = this.DownloadBytesLength = 0;
                            CreatedOrStartedDateTime           = DateTime.Now;
                            _InstantSpeedInMbps                = null;
                            TotalParts                         = 0;
                            SuccessDownloadParts               = 0;
                            break;

                        case DownloadStatus.Running:
                            _DownloadBytesLength_BeforeRunning = this.DownloadBytesLength;
                            CreatedOrStartedDateTime           = DateTime.Now;
                            _InstantSpeedInMbps                = null;
                            break;

                        case DownloadStatus.Canceled:
                        case DownloadStatus.Error:
                        case DownloadStatus.Finished:
                            _FinitaElapsed = (DateTime.Now - CreatedOrStartedDateTime);
                            break;

                        case DownloadStatus.Paused:
                        case DownloadStatus.Wait:
                            _PausedOrWaitElapsed = (DateTime.Now - CreatedOrStartedDateTime);
                            break;
                    }

                    Status = newStatus;
                    call__RowPropertiesChanged = true;
                }
            }
            if ( call__RowPropertiesChanged )
            {
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
        [M(O.AggressiveInlining)] public TimeSpan GetElapsed4SpeedMeasurement()
        {
            switch ( Status )
            {
                case DownloadStatus.Canceled:
                case DownloadStatus.Error:
                case DownloadStatus.Finished:
                    return (_FinitaElapsed);

                case DownloadStatus.Paused:
                case DownloadStatus.Wait:
                    return (_PausedOrWaitElapsed);

                default:
                    return (DateTime.Now - (IsLiveStream ? _CreatedOrStartedDateTime_4_LastPartOfLiveStream.GetValueOrDefault( CreatedOrStartedDateTime ) : CreatedOrStartedDateTime));
            }
        }
        [M(O.AggressiveInlining)] public long GetDownloadBytesLengthAfterLastRun()
        {
            lock ( this )
            {
                return (this.DownloadBytesLength - _DownloadBytesLength_BeforeRunning);
            }
        }
        [M(O.AggressiveInlining)] public double? GetInstantSpeedInMbps()
        {
            lock ( this )
            {
                return (_InstantSpeedInMbps);
            }
        }
//#if DEBUG
        public override string ToString() => $"{Status}, {(IsLiveStream ? "(LiveStream), " : null)}'{GetOutputFullFileName()}'";
//#endif
    }

    /// <summary>
    /// 
    /// </summary>
    internal class DownloadRow_Definer_1
    {
        required public string Url { get; init; }
        required public IDictionary< string, string > RequestHeaders { get; init; }
        required public web_proxy_info WebProxyInfo { get; init; }
        required public string OutputFileName  { get; init; }
        required public string OutputDirectory { get; init; }


        public static implicit operator DownloadRow_Definer_1( 
            in (string Url, IDictionary< string, string > RequestHeaders, web_proxy_info WebProxyInfo, string OutputFileName, string OutputDirectory) t )
        => new DownloadRow_Definer_1()
        {
            Url             = t.Url,
            RequestHeaders  = t.RequestHeaders,
            OutputFileName  = t.OutputFileName,
            OutputDirectory = t.OutputDirectory,
            WebProxyInfo    = t.WebProxyInfo
        };
        public static implicit operator DownloadRow_Definer_1( 
            in (string Url, IDictionary< string, string > RequestHeaders, string OutputFileName, string OutputDirectory) t )
        => new DownloadRow_Definer_1()
        {
            Url             = t.Url,
            RequestHeaders  = t.RequestHeaders,
            OutputFileName  = t.OutputFileName,
            OutputDirectory = t.OutputDirectory,
            WebProxyInfo    = default
        };
    }
    /// <summary>
    /// 
    /// </summary>
    internal class DownloadRow_Definer_2 : DownloadRow_Definer_1
    {
        required public bool IsLiveStream                 { get; init; }
        required public long LiveStreamMaxFileSizeInBytes { get; init; }

        public static implicit operator DownloadRow_Definer_2(
            in (string Url, IDictionary< string, string > RequestHeaders, web_proxy_info WebProxyInfo, string OutputFileName, string OutputDirectory, bool IsLiveStream, long LiveStreamMaxFileSizeInBytes) t )
                                                                          //(f.GetOutputFileName(), f.GetOutputDirectory(), f.IsLiveStream, f.LiveStreamMaxFileSizeInBytes, f.AutoStartDownload)
        => new DownloadRow_Definer_2()
        {
            Url             = t.Url,
            RequestHeaders  = t.RequestHeaders,
            OutputFileName  = t.OutputFileName,
            OutputDirectory = t.OutputDirectory,
            WebProxyInfo    = t.WebProxyInfo,
            IsLiveStream    = t.IsLiveStream,
            LiveStreamMaxFileSizeInBytes = t.LiveStreamMaxFileSizeInBytes
        };
    }
    /// <summary>
    /// 
    /// </summary>
    internal class DownloadRow_Definer_3 : DownloadRow_Definer_2
    {
        required public DateTime       CreatedOrStartedDateTime { get; init; }
        required public DownloadStatus Status                   { get; init; }

        //public static implicit operator DownloadRow_Definer_3(
        //    in (DateTime CreatedOrStartedDateTime, string Url, IDictionary< string, string > RequestHeaders, web_proxy_info WebProxyInfo, string OutputFileName, string OutputDirectory, DownloadStatus Status, bool IsLiveStream, long LiveStreamMaxFileSizeInBytes) t )
        //=> new DownloadRow_Definer_3()
        //{
        //    CreatedOrStartedDateTime = t.CreatedOrStartedDateTime,
        //    Url             = t.Url,
        //    RequestHeaders  = t.RequestHeaders,
        //    OutputFileName  = t.OutputFileName,
        //    OutputDirectory = t.OutputDirectory,
        //    Status          = t.Status,
        //    WebProxyInfo    = default,
        //    IsLiveStream    = t.IsLiveStream,
        //    LiveStreamMaxFileSizeInBytes = t.LiveStreamMaxFileSizeInBytes
        //};
    }
}
