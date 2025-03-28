﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using m3u8.download.manager.infrastructure;
using m3u8.download.manager.models;
using m3u8.download.manager.Properties;
using m3u8.ext;
using m3u8.infrastructure;

using _m3u8_processor_ = m3u8.m3u8_processor_adv__v2;
using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;

namespace m3u8.download.manager.controllers
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class DownloadController : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        public delegate void IsDownloadingChangedEventHandler( bool isDownloading );
        /// <summary>
        /// 
        /// </summary>
        private struct Tuple
        {
            [M(O.AggressiveInlining)]
            public static Tuple Create( m3u8_client mc, CancellationTokenSource cts
                                      , ManualResetEventSlim waitIfPausedEvent
                                      , IDownloadThreadsSemaphoreEx downloadThreadsSemaphore
                                      , int startOrderNumber ) 
                => new Tuple() { mc = mc, cts = cts
                               , waitIfPausedEvent        = waitIfPausedEvent
                               , downloadThreadsSemaphore = downloadThreadsSemaphore
                               , startOrderNumber         = startOrderNumber };
            [M(O.AggressiveInlining)]
            public static Tuple Create( CancellationTokenSource cts
                                      , ManualResetEventSlim waitIfPausedEvent
                                      , IDownloadThreadsSemaphoreEx downloadThreadsSemaphore
                                      , int startOrderNumber ) 
                => new Tuple() { cts = cts
                               , waitIfPausedEvent        = waitIfPausedEvent
                               , downloadThreadsSemaphore = downloadThreadsSemaphore
                               , startOrderNumber         = startOrderNumber };
            [M(O.AggressiveInlining)]
            public static Tuple Create( m3u8_client_next mc_next, CancellationTokenSource cts
                                      , ManualResetEventSlim waitIfPausedEvent
                                      , IDownloadThreadsSemaphoreEx downloadThreadsSemaphore
                                      , IDownloadThreadsSemaphoreEx downloadThreadsSemaphore_2
                                      , int startOrderNumber ) 
                => new Tuple() { mc_next = mc_next, cts = cts
                               , waitIfPausedEvent          = waitIfPausedEvent
                               , downloadThreadsSemaphore   = downloadThreadsSemaphore
                               , downloadThreadsSemaphore_2 = downloadThreadsSemaphore_2
                               , startOrderNumber           = startOrderNumber };

            public m3u8_client                 mc                         { [M(O.AggressiveInlining)] get; private set; }
            public m3u8_client_next            mc_next                    { [M(O.AggressiveInlining)] get; private set; }
            public CancellationTokenSource     cts                        { [M(O.AggressiveInlining)] get; private set; }
            public ManualResetEventSlim        waitIfPausedEvent          { [M(O.AggressiveInlining)] get; private set; }
            public IDownloadThreadsSemaphoreEx downloadThreadsSemaphore   { [M(O.AggressiveInlining)] get; private set; }
            public IDownloadThreadsSemaphoreEx downloadThreadsSemaphore_2 { [M( O.AggressiveInlining )] get; private set; }
            public int                         startOrderNumber           { [M(O.AggressiveInlining)] get; private set; }
        }

        #region [.fields.]
        private const int MILLISECONDSDELAY_M3U8FILE_OUTPUT_PAUSE = 500; //3_000;
        private const int STREAM_IN_POOL_CAPACITY                 = 1_024 * 1_024 * 5;
        private const int RESP_BUF_IN_POOL_CAPACITY               = 1_024 * 100;
        private DownloadListModel                          _Model;
        private SettingsPropertyChangeController           _SettingsController;
        private ConcurrentDictionary< DownloadRow, Tuple > _Dict;
        private cross_download_instance_restriction        _CrossDownloadInstanceRestriction;
        private interlocked_lock                           _ProcessCrossDownloadInstanceRestrictionLock;
        private int                                        _RealRunningCount;
        private download_threads_semaphore_factory         _DownloadThreadsSemaphoreFactory;
        private download_threads_semaphore_factory         _DownloadThreadsSemaphoreFactory_2;
        private DefaultConnectionLimitSaver                _DefaultConnectionLimitSaver;
        private I_throttler_by_speed__v2_t                 _ThrottlerBySpeed;
        private ObjectPoolDisposable< Stream >             _StreamPool;
        private ObjectPool< byte[] >                       _RespBufPool;
        #endregion

        private static ObjectPoolDisposable< Stream > CreateStreamPool( int maxDegreeOfParallelism, int streamInPoolCapacity = STREAM_IN_POOL_CAPACITY ) 
            => new ObjectPoolDisposable< Stream >( maxDegreeOfParallelism, () => new MemoryStream( streamInPoolCapacity ) );
        private static ObjectPool< byte[] > CreateRespBufPool( int maxDegreeOfParallelism, int bufInPoolCapacity = RESP_BUF_IN_POOL_CAPACITY ) 
            => new ObjectPool< byte[] >( maxDegreeOfParallelism, () => new byte[ bufInPoolCapacity ] );

        #region [.ctor().]
        public DownloadController( DownloadListModel model, SettingsPropertyChangeController sc )
        {
            _Model = model ?? throw (new ArgumentNullException( nameof(model) ));

            _SettingsController = sc ?? throw (new ArgumentNullException( nameof(sc) ));
            _SettingsController.SettingsPropertyChanged += SettingsController_PropertyChanged;

            _Dict = new ConcurrentDictionary< DownloadRow, Tuple >();

            _CrossDownloadInstanceRestriction = new cross_download_instance_restriction( _SettingsController.MaxCrossDownloadInstance );
            _DownloadThreadsSemaphoreFactory  = new download_threads_semaphore_factory( _SettingsController.UseCrossDownloadInstanceParallelism,
                                                                                        _SettingsController.MaxDegreeOfParallelism );
            _DownloadThreadsSemaphoreFactory_2 = new download_threads_semaphore_factory( _SettingsController.UseCrossDownloadInstanceParallelism,
                                                                                         _SettingsController.MaxDegreeOfParallelism );

            _DefaultConnectionLimitSaver = DefaultConnectionLimitSaver.Create( _SettingsController.MaxDegreeOfParallelism );
#if THROTTLER__V1
            _ThrottlerBySpeed = new throttler_by_speed_impl__v1( _SettingsController.MaxSpeedThresholdInMbps );
#endif
#if THROTTLER__V2
            _ThrottlerBySpeed = new throttler_by_speed_impl__v2( _SettingsController.MaxSpeedThresholdInMbps );
#endif
            _StreamPool  = CreateStreamPool ( _SettingsController.MaxDegreeOfParallelism );
            _RespBufPool = CreateRespBufPool( _SettingsController.MaxDegreeOfParallelism );
        }

        public void Dispose()
        {
            m3u8_client_factory.ForceClearAndDisposeAll();

            _DefaultConnectionLimitSaver.Dispose();
            _ThrottlerBySpeed.Dispose();
            _StreamPool.Dispose();

            if ( _DownloadThreadsSemaphoreFactory != null )
            {
                _DownloadThreadsSemaphoreFactory.Dispose();
                _DownloadThreadsSemaphoreFactory = null;
            }
            if ( _DownloadThreadsSemaphoreFactory_2 != null )
            {
                _DownloadThreadsSemaphoreFactory_2.Dispose();
                _DownloadThreadsSemaphoreFactory_2 = null;
            }
            if ( _SettingsController != null )
            {
                _SettingsController.SettingsPropertyChanged -= SettingsController_PropertyChanged;
                _SettingsController = null;
            }
        }
        #endregion

        #region [.static 'GetFileTextContent'.]
        public static Task< (m3u8_file_t m3u8File, Exception error) > GetFileTextContent( string m3u8FileUrlText, IDictionary< string, string > requestHeaders, TimeSpan requestTimeoutByPart, CancellationTokenSource cts = null )
        {
            #region [.url.]
            if ( !UrlHelper.TryGetM3u8FileUrl( m3u8FileUrlText?.Trim(), out var t ) )
            {
                return Task.FromResult( (default(m3u8_file_t), t.error) );
            }
            #endregion

            return (GetFileTextContent( t.m3u8FileUrl, requestHeaders, requestTimeoutByPart, cts ));
        }
        public static async Task< (m3u8_file_t m3u8File, Exception error) > GetFileTextContent( Uri m3u8FileUrl, IDictionary< string, string > requestHeaders, TimeSpan requestTimeoutByPart, CancellationTokenSource cts = null )
        {
            using ( var mc = m3u8_client_next_factory.Create( requestTimeoutByPart, attemptRequestCountByPart: 1 ) )
            {
                try
                {
                    var m3u8File = await mc.DownloadFile( m3u8FileUrl, cts?.Token ?? default, requestHeaders );

                    return (m3u8File, null);
                }
                catch ( Exception ex )
                {
                    return (default, ex);
                }
            }
        }
        #endregion

        #region [.private methods.]
        private async void SettingsController_PropertyChanged( Settings settings, string propertyName )
        {
            switch ( propertyName )
            {
                case nameof(Settings.MaxDegreeOfParallelism):
                {
                    await ProcessChangedMaxDegreeOfParallelism( _SettingsController.MaxDegreeOfParallelism );                    
                }
                break;

                case nameof(Settings.UseCrossDownloadInstanceParallelism):
                {
                    _DownloadThreadsSemaphoreFactory.UseCrossDownloadInstanceParallelism = settings.UseCrossDownloadInstanceParallelism;
                    _DownloadThreadsSemaphoreFactory_2.UseCrossDownloadInstanceParallelism = settings.UseCrossDownloadInstanceParallelism;
                }
                break;

                case nameof(Settings.MaxCrossDownloadInstance):
                {
                    _CrossDownloadInstanceRestriction.SetMaxCrossDownloadInstance( settings.MaxCrossDownloadInstance );
                    await ProcessCrossDownloadInstanceRestriction( settings.MaxCrossDownloadInstance );
                }
                break;

                case nameof(Settings.MaxSpeedThresholdInMbps):
                {
                    _ThrottlerBySpeed.ChangeMaxSpeedThreshold( settings.MaxSpeedThresholdInMbps );
                }
                break;
            }
        }

        private static void DisposeExistsTupleWhenAdd2Dict( Tuple t )
        {
#if DEBUG
            if ( Debugger.IsAttached )
            {
                Debugger.Break();
            }
            else
            {
                Debug.Assert( false, nameof(DisposeExistsTupleWhenAdd2Dict) );
            }
#endif
            t.cts.Cancel_NoThrow();
            t.cts.Dispose_NoThrow();
            t.mc?.Dispose_NoThrow();
            t.mc_next?.Dispose_NoThrow();
            t.waitIfPausedEvent.Set_NoThrow();
            t.waitIfPausedEvent.Dispose_NoThrow();        
            t.downloadThreadsSemaphore.Dispose_NoThrow();
            t.downloadThreadsSemaphore_2?.Dispose_NoThrow();
        }
        [M(O.AggressiveInlining)] private void Fire_IsDownloadingChanged() => IsDownloadingChanged?.Invoke( (_Dict.Count != 0) );

        #region [.Process of a Changed 'MaxDegreeOfParallelism' props.]
        private async Task ProcessChangedMaxDegreeOfParallelism( int maxDegreeOfParallelism )
        {
            _DefaultConnectionLimitSaver.Reset( maxDegreeOfParallelism );

            var ph = new PausedHelper( _Dict, default );
            var tuples = await ph.PausedAll_Started_Running_and_GetThem().CAX();

            await _DownloadThreadsSemaphoreFactory.ResetMaxDegreeOfParallelism( maxDegreeOfParallelism ).CAX();
            await _DownloadThreadsSemaphoreFactory_2.ResetMaxDegreeOfParallelism( maxDegreeOfParallelism ).CAX();
            _StreamPool.ChangeCapacity( maxDegreeOfParallelism );
            _RespBufPool.ChangeCapacity( maxDegreeOfParallelism );

            ph.ResetMaxDegreeOfParallelism_For_NonUseCrossDownloadInstanceParallelism_DownloadThreadsSemaphore( maxDegreeOfParallelism );
            ph.ContinueAll_Paused( tuples );
        }
        #endregion

        #region [.Process 'CrossDownloadInstanceRestriction'.]
        [M(O.AggressiveInlining)] private bool CanStartOrSetWaitCrossDownloadInstanceRestriction( DownloadRow row )
        {
            var maxCrossDownloadInstance = _CrossDownloadInstanceRestriction.GetMaxCrossDownloadInstance();
            if ( maxCrossDownloadInstance.HasValue ) //if used cross download instance restriction
            {
                var runningCount = Volatile.Read( ref _RealRunningCount ); //---_Model.GetRows().Count( _row => _row.IsRunning() );
                if ( maxCrossDownloadInstance.Value <= runningCount )
                {
                    row.StatusWait();
                    return (false);
                }
            }
            return (true);
        }
        private async Task ProcessCrossDownloadInstanceRestriction( int? maxCrossDownloadInstance )
        {
            if ( !_ProcessCrossDownloadInstanceRestrictionLock.TryEnter() )
            {
                return;
            }

            try
            {
                void StartAdditionalDownloadsIfAllowed( int allowedStartCount )
                {
                    if ( 0 < allowedStartCount )
                    {
                        StartAdditionalDownloads( allowedStartCount );
                    }
                };
                void StartAdditionalDownloads( int allowedStartCount )
                {
#if DEBUG
                    Debug.Assert( 0 < allowedStartCount );
#endif
                    foreach ( var row in _Model.GetRows().ToArrayEx() )
                    {
                        if ( row.IsWait() /*&& _Dict.ContainsKey( row )*/ )
                        {
                            Start( row );
                            allowedStartCount--;
                            if ( allowedStartCount <= 0 )
                            {
                                break;
                            }
                        }
                    }
                };
                async Task PausedExtraDownloads( int d )
                {
                    if ( d < 0 )
                    {
                        foreach ( var row in _Model.GetRows().Reverse().ToArray() )
                        {
                            if ( row.IsRunning() )
                            {
                                await PauseWithWait( row, CancellationToken.None ).CAX();
                                row.StatusWait();
                                d++;
                                if ( 0 <= d )
                                {
                                    break;
                                }
                            }
                        }
                    }
                };
                void StartAllWaitDownloads()
                {
                    foreach ( var row in _Model.GetRows().ToArrayEx() )
                    {
                        if ( row.IsWait() )
                        {
                            Start( row );
                        }
                    }
                };

                if ( maxCrossDownloadInstance.HasValue ) //if used cross download instance restriction
                {
                    var runningCount       = Volatile.Read( ref _RealRunningCount );
                    var actualRunningCount = _Model.GetRows().Count( row => row.IsRunning() );
                    var d = maxCrossDownloadInstance.Value - runningCount;

                    if ( d == 0 ) //nothing todo => start downloads, whos was started and after be wait (because was changed 'MaxCrossDownloadInstance' value setting)
                    {                        
                        d = maxCrossDownloadInstance.Value - actualRunningCount;
                        StartAdditionalDownloadsIfAllowed( d );
                        return;
                    }

                    if ( actualRunningCount < runningCount )
                    {
                        d = maxCrossDownloadInstance.Value - actualRunningCount;
                    }

                    if ( 0 < d ) //can start additional downloads
                    {
                        StartAdditionalDownloads( d );
                    }
                    else //need paused extra downloads
                    {
                        await PausedExtraDownloads( d ).CAX();
                    }
                }
                else //not-used cross download instance restriction
                {
                    StartAllWaitDownloads();
                }
            }
            finally
            {
                _ProcessCrossDownloadInstanceRestrictionLock.Exit();
            }
        }

        private async Task PauseWithWait( DownloadRow row, CancellationToken ct, int millisecondsDelay = 10, int? totalMillisecondsTimeout = null )
        {
            if ( (row != null) && _Dict.TryGetValue( row, out var t ) )
            {
                t.waitIfPausedEvent.Reset_NoThrow();
                if ( row.IsPaused() )
                {
                    return;
                }

                await Task.Run( /*async*/ () =>
                {
                    var remainedDelayCount = (totalMillisecondsTimeout.HasValue ? (totalMillisecondsTimeout.Value / Math.Max( 1, millisecondsDelay )) : int.MaxValue);

                    for ( ; _Dict.TryGetValue( row, out var x ) && (0 <= remainedDelayCount) && !ct.IsCancellationRequested; )
                    {
                        t.waitIfPausedEvent.Reset_NoThrow();
                        if ( row.IsPaused() )
                        {
                            return;
                        }

                        remainedDelayCount--;
                        Task.Delay( millisecondsDelay, ct ).Wait( ct );
                        //await Task.Delay( millisecondsDelay, ct ).CAX();
                    }
                }, ct ).ContinueWith( task => {
                    if ( !task.IsCanceled && task.IsFaulted ) // suppress cancel exception
                    {
                        throw (task.Exception);
                    }
                }, TaskContinuationOptions.ExecuteSynchronously ).CAX();
            }
        }
        #endregion
        #endregion

        #region [.public props.]
        public event IsDownloadingChangedEventHandler IsDownloadingChanged;

        public DownloadListModel Model { get => _Model; }
        public bool IsDownloading { [M(O.AggressiveInlining)] get => (_Dict.Count != 0); }
        #endregion

        #region [.public methods.]
        public async void Start( DownloadRow row )
        {
            #region [.check exists & paused.]
            if ( row == null )
            {
                return;
            }
            if ( _Dict.TryGetValue( row, out var x ) )
            {
                if ( row.IsPaused() || row.IsWait() )
                {
                    x.waitIfPausedEvent.Set();
                }
                return;
            }
            #endregion

            #region [.starting.]
            row.StatusStarted();
            #endregion

            #region [.url.]
            if ( !UrlHelper.TryGetM3u8FileUrl( row.Url, out var t ) )
            {
                row.StatusError( t.error );
                return;
            }
            #endregion

            #region [.cross download instance restriction.]
            if ( !CanStartOrSetWaitCrossDownloadInstanceRestriction( row ) )
            {
                return;
            }
            #endregion

            Interlocked.Increment( ref _RealRunningCount );

            #region [.routine.]
            if ( row.IsLiveStream )
            {
                await StartLiveStreamRoutine( row, t.m3u8FileUrl );
            }
            else
            {
                await StartRoutine( row, t.m3u8FileUrl );
            }
            #endregion

            Interlocked.Decrement( ref _RealRunningCount );

            #region [.cross download instance restriction.]
            await ProcessCrossDownloadInstanceRestriction( _CrossDownloadInstanceRestriction.GetMaxCrossDownloadInstance() );
            #endregion
        }
        private async Task StartRoutine( DownloadRow row, Uri m3u8FileUrl )
        {
            using ( var mc                         = m3u8_client_next_factory.Create( _SettingsController.GetCreateM3u8ClientParams() ) )
            using ( var cts                        = new CancellationTokenSource() )
            using ( var waitIfPausedEvent          = new ManualResetEventSlim( true, 0 ) )
            using ( var downloadThreadsSemaphore   = _DownloadThreadsSemaphoreFactory.Get() )
            using ( var downloadThreadsSemaphore_2 = _DownloadThreadsSemaphoreFactory_2.Get() )
            {
                var tup = Tuple.Create( mc, cts, waitIfPausedEvent, downloadThreadsSemaphore, downloadThreadsSemaphore_2, _Dict.Count );
                _Dict.Add( row, tup, DisposeExistsTupleWhenAdd2Dict ); Fire_IsDownloadingChanged();

                try
                {
                    var sw = Stopwatch.StartNew();

                    //-1-//
                    row.AddBeginRequest2Log();
                    var m3u8File = await mc.DownloadFile( m3u8FileUrl, cts.Token, row.RequestHeaders );

                    row.SetTotalParts( m3u8File.Parts.Count );
                    row.Log.Output( m3u8File );

                    //-2-//
                    await Task.Delay( MILLISECONDSDELAY_M3U8FILE_OUTPUT_PAUSE, cts.Token );
                    row.AddBeginRequest2Log();

                    //-3-//
                    var anyErrorHappend = false;
                    var dpsr = await Task.Run(
#if NETCOREAPP
                    async
#endif
                    () =>
                    {
                        var start_ts  = Stopwatch.GetTimestamp();
                        var rows_Dict = new Dictionary< int, LogRow >( m3u8File.Parts.Count );

                        var requestStepAction  = new m3u8_processor_next.RequestStepActionDelegate( (in m3u8_processor_next.RequestStepActionParams p) =>
                        {
                            row.SetStatus( DownloadStatus.Running );

                            var requestText = $"#{p.PartOrderNumber} of {p.TotalPartCount}). '{p.Part.RelativeUrlName}'...";
                            if ( p.Success )
                            {
                                var logRow = row.Log.AddRequestRow( requestText, responseText: "/starting/..." );
                                rows_Dict.Add( p.Part.OrderNumber, logRow );
                            }
                            else
                            {
                                anyErrorHappend = true;
                                row.Log.AddResponseErrorRow( requestText, p.Error.ToString() );
                            }
                        });
                        var responseStepAction = new m3u8_processor_next.ResponseStepActionDelegate( (in m3u8_processor_next.ResponseStepActionParams p ) =>
                        {
                            row.SetDownloadResponseStepParams( p );

                            if ( rows_Dict.TryGetValue( p.Part.OrderNumber, out var logRow ) )
                            {
                                rows_Dict.Remove( p.Part.OrderNumber );
                                if ( p.Part.Error != null )
                                {
                                    anyErrorHappend = true;
                                    logRow.SetResponseError( p.Part.Error.ToString() );
                                }
                                else
                                {
                                    logRow.SetResponseSuccess( $"received, ({Extensions.GetSizeFormatted( p.Part.Stream.Length )})" );
                                }
                            }
                        });
                        var downloadPartStepAction  = new m3u8_client_next.DownloadPartStepActionDelegate( (in m3u8_client_next.DownloadPartStepActionParams p) =>
                        {
                            var ts = Stopwatch.GetTimestamp();
                            var raiseRowPropertiesChangedEvent = InterlockedExtension.ExchangeIfNewValueBigger( ref start_ts, ts, ts - (100 * InterlockedExtension.TicksPerMillisecond) );
                            row.SetDownloadPartStepParams( p, raiseRowPropertiesChangedEvent );

                            if ( rows_Dict.TryGetValue( p.Part.OrderNumber, out var logRow ) )
                            {
                                if ( p.Part.Error != null )
                                {
                                    rows_Dict.Remove( p.Part.OrderNumber );
                                    anyErrorHappend = true;
                                    logRow.SetResponseError( p.Part.Error.ToString(), p.AttemptRequestNumber );
                                }
                                else if ( raiseRowPropertiesChangedEvent )
                                {
                                    var msg = p.TotalContentLength.HasValue ? $"{Extensions.GetSizeFormatted( p.TotalBytesReaded )} of {Extensions.GetSizeFormatted( p.TotalContentLength.Value )}"
                                                                               : Extensions.GetSizeFormatted( p.TotalBytesReaded );
                                    logRow.SetResponse( msg, p.AttemptRequestNumber );
                                }
                                else
                                {
                                    logRow.SetAttemptRequestNumber( p.AttemptRequestNumber );
                                }
                            }
                            #region comm. prev.
                            /*
                            if ( raiseRowPropertiesChangedEvent && rows_Dict.TryGetValue( p.Part.OrderNumber, out var logRow ) )
                            {
                                if ( p.Part.Error != null )
                                {
                                    rows_Dict.Remove( p.Part.OrderNumber );
                                    anyErrorHappend = true;
                                    logRow.SetResponseError( p.Part.Error.ToString(), p.AttemptRequestNumber );
                                }
                                else
                                {
                                    var msg = p.TotalContentLength.HasValue ? $"{Extensions.GetSizeFormatted( p.TotalBytesReaded )} of {Extensions.GetSizeFormatted( p.TotalContentLength.Value )}" 
                                                                               : Extensions.GetSizeFormatted( p.TotalBytesReaded );
                                    logRow.SetResponse( msg, p.AttemptRequestNumber );
                                }
                            }
                            //*/
                            #endregion
                        });
                        var waitingIfPausedAction   = new Action( () => row.SetStatus( DownloadStatus.Paused ) );
                        var waitingIfPausedBefore_2 = new Action< m3u8_part_ts__v2 >( part =>
                        {
                            waitingIfPausedAction();
                            if ( rows_Dict.TryGetValue( part.OrderNumber, out var logRow ) )
                            {
                                logRow.SetResponse( logRow.ResponseText + ", /paused/" );
                            }
                        });
                        var waitingIfPausedAfter_2  = new Action< m3u8_part_ts__v2 >( part =>
                        {
                            row.SetStatus( DownloadStatus.Running );
                            if ( rows_Dict.TryGetValue( part.OrderNumber, out var logRow ) )
                            {
                                logRow.SetResponse( "/continue/..." );
                            }
                        });

                        var veryFirstOutputFullFileName = row.SaveVeryFirstOutputFullFileName();

                        var ip = new m3u8_processor_next.DownloadPartsAndSaveInputParams()
                        {
                            mc                         = mc,
                            m3u8File                   = m3u8_file_t__v2.Parse( m3u8File ),
                            OutputFileName             = veryFirstOutputFullFileName,
                            CancellationToken          = cts.Token,
                            RequestStepAction          = requestStepAction,
                            ResponseStepAction         = responseStepAction,
                            MaxDegreeOfParallelism     = _SettingsController.MaxDegreeOfParallelism,                            
                            DownloadThreadsSemaphore   = downloadThreadsSemaphore,
                            DownloadThreadsSemaphore_2 = downloadThreadsSemaphore_2,
                            WaitIfPausedEvent          = waitIfPausedEvent,
                            WaitingIfPaused            = waitingIfPausedAction,
                            WaitingIfPausedBefore_2    = waitingIfPausedBefore_2,
                            WaitingIfPausedAfter_2     = waitingIfPausedAfter_2,
                            ThrottlerBySpeed           = _ThrottlerBySpeed,
                            StreamPool                 = _StreamPool,
                            RespBufPool                = _RespBufPool,
                            DownloadPartStepAction     = downloadPartStepAction,
                        };
#if NETCOREAPP
                        var result = await m3u8_processor_next.DownloadPartsAndSave_Async( ip, row.RequestHeaders ).CAX();
#else
                        var result = m3u8_processor_next.DownloadPartsAndSave( ip, row.RequestHeaders );
#endif
                        return (result);
                    });

                    //-4-//
                    _Dict.Remove( row ); Fire_IsDownloadingChanged();

                    #region [.remane output file if changed.]
                    var renameOutputFileException = default(Exception);

                    var desiredOutputFullFileName = row.GetOutputFullFileName();
                    if ( dpsr.OutputFileName != desiredOutputFullFileName )
                    {
                        if ( row.VeryFirstOutputFullFileName.IsNullOrEmpty() /*dpsr.OutputFileName.EqualIgnoreCase( row.VeryFirstOutputFullFileName )*/ 
                              || 
                             FileHelper.TryMoveFile_NoThrow( dpsr.OutputFileName, desiredOutputFullFileName, out renameOutputFileException ) )
                        {
                            dpsr.ResetOutputFileName( desiredOutputFullFileName );
                        }
                    }
                    #endregion

                    row.StatusFinished( dpsr, sw.StopAndElapsed() );

                    #region [.error rename output-file happen.]
                    if ( renameOutputFileException != null )
                    {
                        row.StatusErrorIfRenameOutputFile( renameOutputFileException );
                    }
                    #endregion

                    #region [.any error happen.]
                    if ( anyErrorHappend )
                    {
                        row.StatusErrorWithNoLog();
                    }
                    #endregion
                }
                catch ( Exception ex )
                {
                    _Dict.Remove( row ); Fire_IsDownloadingChanged();

                    if ( cts.IsCancellationRequested )
                    {
                        FileHelper.DeleteFiles_NoThrow( row.GetOutputFullFileNames() );
                        row.StatusCanceled();
                    }
                    else if ( ex is m3u8_Exception mex )
                    {
                        FileHelper.DeleteFiles_NoThrow( row.GetOutputFullFileNames() );
                        row.StatusError( ex.Message );
                    }
                    else
                    {
                        row.StatusError( ex );
                    }
                }
            }
        }
        private async Task StartLiveStreamRoutine( DownloadRow row, Uri m3u8FileUrl )
        {
            var (timeout, _) = _SettingsController.GetCreateM3u8ClientParams();
            var (hc, d) = HttpClientFactory_WithRefCount.Get( timeout );
            using ( d )
            //using ( var mc                       = m3u8_client_factory.Create( _SettingsController.GetCreateM3u8ClientParams() ) )
            using ( var cts                      = new CancellationTokenSource() )
            using ( var waitIfPausedEvent        = new ManualResetEventSlim( true, 0 ) )
            using ( var downloadThreadsSemaphore = _DownloadThreadsSemaphoreFactory.Get() )
            {
                var tup = Tuple.Create( /*mc,*/ cts, waitIfPausedEvent, downloadThreadsSemaphore, _Dict.Count );
                _Dict.Add( row, tup, DisposeExistsTupleWhenAdd2Dict ); Fire_IsDownloadingChanged();

                try
                {
                    var sw = Stopwatch.StartNew();
                    row.AddBeginRequest2Log();

                    //-1-//
                    var anyErrorHappend = false;
                    await Task.Run( async () =>
                    {
                        #region [.veryFirstOutputFullFileName.]
                        var veryFirstOutputFullFileName = row.VeryFirstOutputFullFileName.IsNullOrEmpty() ? row.GetOutputFullFileName() 
                                                                                                          : row.VeryFirstOutputFullFileName;
                        row.SaveVeryFirstOutputFullFileName( veryFirstOutputFullFileName );
                        #endregion

                        var rows_Dict = new Dictionary< string, LogRow >();
                        var localLock = new object();

                        var st = (LastPartLogRows       : new List< LogRow >(),
                                  CreateDateTime        : default(DateTime), 
                                  RowSaveState          : default(DownloadRow), 
                                  CreatedOutpuFileLogRow: default(LogRow));
                        var queued_cnt      = 0;
                        var output_file_cnt = 0;

                        var downloadContentAction = new m3u8_live_stream_downloader.DownloadContentDelegate( part_url =>
                        {
                            lock ( localLock )
                            {
                                row.SetStatus( DownloadStatus.Running );
                                rows_Dict[ part_url ] = row.Log.AddRequestRow( $"{++queued_cnt}). [queued]: {part_url}" );
                            }
                        });
                        var downloadContentErrorAction = new m3u8_live_stream_downloader.DownloadContentErrorDelegate( (part_url, ex) =>
                        {
                            lock ( localLock )
                            {
                                anyErrorHappend = true;
                                rows_Dict[ part_url ] = row.Log.AddResponseErrorRow( $"{++queued_cnt}).[queued]: {part_url}", ex.ToString() );
                            }
                        });
                        var downloadPartAction = new m3u8_live_stream_downloader.DownloadPartDelegate( (part_url, partBytes, totalBytes, instantSpeedInMbps) =>
                        {
                            lock ( localLock )
                            {
                                row.SetDownloadResponseStepParams( partBytes, totalBytes, instantSpeedInMbps );
                                if ( rows_Dict.RemoveEx( part_url, out var logRow ) )
                                {
                                    logRow.SetResponseSuccess( "received" );
                                    st.LastPartLogRows.Add( logRow );
                                }
                                //$"[DOWNLOAD]: {part_url} => ok. (part-size: {(1.0 * partBytes / 1024):N2} KB, total-size: {(1.0 * totalBytes / (1024 * 1024)):N2} MB)";
                            }
                        });
                        var downloadPartErrorAction = new m3u8_live_stream_downloader.DownloadPartErrorDelegate( (part_url, ex) =>
                        {
                            lock ( localLock )
                            {
                                anyErrorHappend = true;
                                row.SetDownloadResponseStepParams_Error();
                                if ( rows_Dict.RemoveEx( part_url, out var logRow ) )
                                {
                                    logRow.SetResponseError( ex.ToString() );
                                    st.LastPartLogRows.Add( logRow );
                                }
                            }
                        });
                        var downloadCreateOutputFileAction = new m3u8_live_stream_downloader.DownloadCreateOutputFileDelegate( fn =>
                        {
                            lock ( localLock )
                            {
                                #region [.create copy of current row with finished state.]
                                if ( 0 < row.DownloadBytesLength /*lastCreatedOutpuFileLogRow != null*/ )
                                {
                                    var part_row = row.Add2ModelFinishedCopy( st.CreateDateTime, st.LastPartLogRows, st.RowSaveState );

                                    st.CreatedOutpuFileLogRow.Append2RequestText( $"size: {Extensions.GetSizeInMbFormatted( part_row.DownloadBytesLength )} mb, elapsed: {part_row.GetElapsed().GetElapsedFormatted()}" );
                                    part_row.Log.AddRow( st.CreatedOutpuFileLogRow );
                                }
                                #endregion

                                row.SetOutputFileName( Path.GetFileName( fn ) );

                                st.CreateDateTime = DateTime.Now;
                                st.RowSaveState   = row.CreateCopy();
                                row.StartNewPartOfLiveStream( st.LastPartLogRows );
                                st.LastPartLogRows.Clear();
                                st.CreatedOutpuFileLogRow = row.Log.AddRequestRow( $"[#{++output_file_cnt}]. Created output file: '{fn}',..." );
                            }
                        });

                        var p = new m3u8_live_stream_downloader.InitParams()
                        { 
                            HttpClient = hc,

                            M3u8Url        = m3u8FileUrl.ToString(),
                            OutputFileName = veryFirstOutputFullFileName,

                            WaitIfPausedEvent = waitIfPausedEvent,
                            WaitingIfPaused   = () => row.SetStatus( DownloadStatus.Paused ),
                            ThrottlerBySpeed  = _ThrottlerBySpeed,

                            DownloadContent          = downloadContentAction,
                            DownloadContentError     = downloadContentErrorAction,
                            DownloadPart             = downloadPartAction,
                            DownloadPartError        = downloadPartErrorAction,
                            DownloadCreateOutputFile = downloadCreateOutputFileAction
                        };

                        await m3u8_live_stream_downloader._Download_( p, cts.Token, () => row.LiveStreamMaxFileSizeInBytes, row.RequestHeaders );
                    });

                    //-4-//
                    _Dict.Remove( row ); Fire_IsDownloadingChanged();

                    #region comm. [.remane output file if changed.]
                    /*
                    var renameOutputFileException = default(Exception);

                    var desiredOutputFullFileName = row.GetOutputFullFileName();
                    if ( dpsr.OutputFileName != desiredOutputFullFileName )
                    {
                        try
                        {
                            if ( !dpsr.OutputFileName.EqualIgnoreCase( desiredOutputFullFileName ) )
                            {
                                Extensions.DeleteFile_NoThrow( desiredOutputFullFileName );
                            }
                            File.Move( dpsr.OutputFileName, desiredOutputFullFileName );
                            dpsr.ResetOutputFileName( desiredOutputFullFileName );
                        }
                        catch ( Exception ex )
                        {
                            renameOutputFileException = ex;
                        }
                    }
                    //*/
                    #endregion

                    row.StatusFinished( /*dpsr,*/ sw.StopAndElapsed() );

                    #region comm. [.error rename output-file happen.]
                    /*
                    if ( renameOutputFileException != null )
                    {
                        row.StatusErrorIfRenameOutputFile( renameOutputFileException );
                    }
                    //*/
                    #endregion

                    #region [.any error happen.]
                    if ( anyErrorHappend )
                    {
                        row.StatusErrorWithNoLog();
                    }
                    #endregion
                }
                catch ( Exception ex )
                {
                    _Dict.Remove( row ); Fire_IsDownloadingChanged();

                    if ( cts.IsCancellationRequested )
                    {
                        FileHelper.DeleteFiles_NoThrow( row.GetOutputFullFileNames() );
                        row.StatusCanceled();
                    }
                    else if ( ex is m3u8_Exception mex )
                    {
                        FileHelper.DeleteFiles_NoThrow( row.GetOutputFullFileNames() );
                        row.StatusError( ex.Message );
                    }
                    else
                    {
                        row.StatusError( ex );
                    }
                }
            }
        }

        public void Pause( DownloadRow row )
        {
            if ( row != null )
            {
                if ( _Dict.TryGetValue( row, out var t ) )
                {
                    t.waitIfPausedEvent.Reset_NoThrow();
                }
                row.SetStatus( DownloadStatus.Paused );
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public interface IDownloadRowStatusTransaction : IDisposable
        {
            void CommitStatus();
            void RollbackStatus();
        }
        /// <summary>
        /// 
        /// </summary>
        private struct CancelIfInProgress_Transaction : IDownloadRowStatusTransaction
        {
            private DownloadRow _Row;
            private DownloadStatus _RollbackStatus;
            public CancelIfInProgress_Transaction( DownloadRow row, DownloadStatus rollbackStatus ) => (_Row, _RollbackStatus) = (row, rollbackStatus);
            public void Dispose() => RollbackStatus();
            public void CommitStatus() => _Row = null;
            public void RollbackStatus() => _Row?.SetStatus( _RollbackStatus );

            public static CancelIfInProgress_Transaction Empty => new CancelIfInProgress_Transaction();
        }
        public IDownloadRowStatusTransaction CancelIfInProgress_WithTransaction( DownloadRow row )
        {
            if ( row != null )
            {
                var status = row.Status;
                switch ( status )
                {
                    case DownloadStatus.Canceled:
                    case DownloadStatus.Finished:
                    case DownloadStatus.Created:
                        break;

                    case DownloadStatus.Error:
                        Cancel( row );
                        return (new CancelIfInProgress_Transaction( row, status ));                        

                    default:
                        Cancel( row );
                        break;
                }
            }
            return (CancelIfInProgress_Transaction.Empty);
        }

        public void Cancel( DownloadRow row )
        {
            if ( row != null )
            {
                if ( _Dict.TryGetValue( row, out var t ) )
                {
                    t.cts.Cancel_NoThrow();
                }
                else
                {
                    row.StatusCanceled();
                }
            }
        }
        public void CancelAll()
        {
            foreach ( var t in _Dict.Values )
            {
                t.cts.Cancel_NoThrow();
            }

            foreach ( var row in _Model.GetRows() )
            {
                if ( row.IsWait() && !_Dict.ContainsKey( row ) )
                {
                    row.StatusCanceled();
                }
            }
        }
        #endregion

        #region [.PausedHelper.]
        private struct PausedHelper
        {
            /// <summary>
            /// 
            /// </summary>
            public struct Tuple
            {
                internal DownloadRow row;
                internal int         order;
            }
            /// <summary>
            /// 
            /// </summary>
            private struct Result
            {
                public IReadOnlyList< Tuple >    tuples;
                public HashSet< DownloadStatus > statuses;
            }

            #region [.field's.]
            private ConcurrentDictionary< DownloadRow, DownloadController.Tuple > _Dict;
            private CancellationToken _Ct;
            #endregion

            #region [.ctor().]
            public PausedHelper( ConcurrentDictionary< DownloadRow, DownloadController.Tuple > dict, CancellationToken ct ) => (_Dict, _Ct) = (dict, ct);
            #endregion

            #region [.method's.]
            private static Result PausedAll_ByStatus_GetThem( ConcurrentDictionary< DownloadRow, DownloadController.Tuple > dict, params DownloadStatus[] statuses )
            {
                var tuples = new List< Tuple >( dict.Count );

                var hs = statuses.ToHashSet();
                foreach ( var p in dict )
                {
                    var row = p.Key;
                    if ( hs.Contains( row.Status ) )
                    {
                        var t = p.Value;
                        if ( t.waitIfPausedEvent.Reset_NoThrow() )
                        {
                            tuples.Add( new Tuple() { row = row, order = t.startOrderNumber } );
                        }
                    }
                }

                return (new Result() { tuples = tuples, statuses = hs });
            }
            private static async Task WaitAll_For_NotHas_Status( Result pr, CancellationToken ct, int millisecondsDelay, int? totalMillisecondsTimeout )
            {
                await Task.Run( /*async*/ () =>
                {
                    var remainedDelayCount = (totalMillisecondsTimeout.HasValue ? (totalMillisecondsTimeout.Value / Math.Max( 1, millisecondsDelay )) : int.MaxValue);
                    for ( int i = pr.tuples.Count - 1; (0 <= i) && (0 <= remainedDelayCount) && !ct.IsCancellationRequested; )
                    {
                        if ( pr.statuses.Contains( pr.tuples[ i ].row.Status ) )
                        {
                            remainedDelayCount--;
                            Task.Delay( millisecondsDelay, ct ).Wait( ct );
                            //await Task.Delay( millisecondsDelay, ct );
                        }
                        else
                        {
                            i--;
                        }
                    }
                }, ct )
                .ContinueWith( task => 
                {
                    if ( !task.IsCanceled && task.IsFaulted ) // suppress cancel exception
                    {
                        throw (task.Exception);
                    }
                }, TaskContinuationOptions.ExecuteSynchronously );
            }

            public async Task< IReadOnlyList< Tuple > > PausedAll_Started_Running_and_GetThem( int millisecondsDelay = 10, int? totalMillisecondsTimeout = null )
            {
                //-1-//
                var pr = PausedAll_ByStatus_GetThem( _Dict, DownloadStatus.Started, DownloadStatus.Running );

                //-2-//
                await WaitAll_For_NotHas_Status( pr, _Ct, millisecondsDelay, totalMillisecondsTimeout );

                return (pr.tuples);
            }

            public void ContinueAll_Paused( IReadOnlyList< Tuple > tuples )
            {
                foreach ( var row in (from t in tuples orderby t.order select t.row) )
                {
                    if ( row.IsPaused() && _Dict.TryGetValue( row, out var t ) )
                    {                        
                        t.waitIfPausedEvent.Set_NoThrow();
                    }
                }
            }

            public void ResetMaxDegreeOfParallelism_For_NonUseCrossDownloadInstanceParallelism_DownloadThreadsSemaphore( int maxDegreeOfParallelism )
            {
                foreach ( var t in _Dict.Values )
                {
                    if ( !t.downloadThreadsSemaphore.UseCrossDownloadInstanceParallelism )
                    {
                        t.downloadThreadsSemaphore.ResetSemaphore( maxDegreeOfParallelism );
                    }
                    if ( (t.downloadThreadsSemaphore_2 != null) && !t.downloadThreadsSemaphore_2.UseCrossDownloadInstanceParallelism )
                    {
                        t.downloadThreadsSemaphore_2.ResetSemaphore( maxDegreeOfParallelism );
                    }
                }
            }
            #endregion
        }
        #endregion


        #region [.Delete rows with output-files.]
        public Task DeleteRowsWithOutputFiles_Parallel_UseSynchronizationContext( IReadOnlyList< DownloadRow > rows, CancellationToken ct, 
            Action< DownloadRow, CancellationToken > deleteFilesAction, 
            Action< DownloadRow > afterSuccesDeleteAction )
        {
            var syncCtx = SynchronizationContext.Current;
            var delete_task = Task.Run(() =>
            {
                Parallel.ForEach( rows, new ParallelOptions() { CancellationToken = ct, MaxDegreeOfParallelism = rows.Count }, row =>
                {
                    using ( var statusTran = this.CancelIfInProgress_WithTransaction( row ) )
                    {
                        deleteFilesAction( row, ct );
                        statusTran.CommitStatus();

                        syncCtx.Invoke(() => afterSuccesDeleteAction( row ));
                    }
                });
            });
            return (delete_task);
        }
        public async Task DeleteRowsWithOutputFiles_Consecutively( DownloadRow[] rows, CancellationToken ct,
            Func< DownloadRow, CancellationToken, Task > deleteFilesAction, 
            Action< DownloadRow > afterSuccesDeleteAction )
        {
            foreach ( var row in rows )
            {
                using ( var statusTran = this.CancelIfInProgress_WithTransaction( row ) )
                {
                    await deleteFilesAction( row, ct );
                    statusTran.CommitStatus();

                    afterSuccesDeleteAction?.Invoke( row );
                }
            }
        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    internal static class DownloadControllerExtensions
    {
        public static void AddBeginRequest2Log( this DownloadRow row, bool clearLog = true ) => row.Log.AddBeginRequest2Log( row.Url, row.RequestHeaders, clearLog );
        public static void AddBeginRequest2Log( this LogListModel log, string url, IDictionary< string, string > requestHeaders, bool clearLog = true )
        {
            if ( clearLog ) log.Clear();
            log.AddRequestRow( "url:" );
            log.AddRequestRow( url );
            log.OutputRequestHeaders( requestHeaders );
        }

        public static void Output( this LogListModel log, in m3u8_file_t m3u8File )
        {
            var lines = m3u8File.RawText?.Split( new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries )
                                .Where( line => !line.IsNullOrWhiteSpace() )
                                ;//.ToList();
            if ( lines.AnyEx() )
            {
                log.BeginUpdate();
                {
                    foreach ( var line in lines )
                    {
                        log.AddRequestRow( line );
                    }
                    log.AddEmptyRow();
                    log.AddRequestRow( $" patrs count: {m3u8File.Parts.Count}" );
                }
                log.EndUpdate();
            }
        }
        public static void OutputRequestHeaders( this LogListModel log, IDictionary< string, string > requestHeaders, bool addEmptyRowAtEnd = true )
        {
            if ( requestHeaders.AnyEx() )
            {
                log.BeginUpdate();
                {
                    log.OutputRequestHeaders_Internal( requestHeaders, addEmptyRowAtEnd );
                }
                log.EndUpdate();
            }
            else if ( addEmptyRowAtEnd )
            {
                log.AddEmptyRow();
            }
        }
        private static void OutputRequestHeaders_Internal( this LogListModel log, IDictionary< string, string > requestHeaders, bool addEmptyRowAtEnd )
        {
            log.AddRequestHeaderRow( $"/request headers: {requestHeaders.Count}/" );
            var n = 0;
            var max_key_len = requestHeaders.Max( p => p.Key.Length );
            var max_n_len   = requestHeaders.Count.ToString().Length;
            foreach ( var p in requestHeaders/*.OrderBy( p => p.Key )*/ )
            {
                //log.AddRequestHeaderRow( $"  ({++n}) {p.Key} = {p.Value}" );

                var n_txt    = (++n).ToString();
                var n_indent = new string( ' ', max_n_len   - n_txt.Length );
                var indent   = new string( ' ', max_key_len - p.Key.Length );
                log.AddRequestHeaderRow( $" {n_indent}({n_txt}) {p.Key}: {indent}{p.Value}" );
            }
            if ( addEmptyRowAtEnd ) log.AddEmptyRow();
        }
        public static void StatusStarted( this DownloadRow row )
        {
            row.SetStatus( DownloadStatus.Started );
            row.Log.Clear();
        }
        public static void StatusWait( this DownloadRow row ) => row.SetStatus( DownloadStatus.Wait );
        public static void StatusFinished( this DownloadRow row, in _m3u8_processor_.DownloadPartsAndSaveResult dpsr, TimeSpan elapsed )
        {
            row.SetStatus( DownloadStatus.Finished );
            var log = row.Log;
            log.AddEmptyRow();
            log.AddRequestRow( $" downloaded & writed parts {dpsr.PartsSuccessCount} of {dpsr.TotalParts}" );
            log.AddEmptyRow();
            log.AddRequestRow( $" elapsed: {elapsed}" );
            log.AddRequestRow( $"         file: '{dpsr.OutputFileName}'" );
            log.AddRequestRow( $"       size: {Extensions.GetSizeInMbFormatted( dpsr.TotalBytes )} mb" );
        }
        public static void StatusFinished( this DownloadRow row, in m3u8_processor_next.DownloadPartsAndSaveResult dpsr, TimeSpan elapsed )
        {
            row.SetStatus( DownloadStatus.Finished );
            var log = row.Log;
            log.AddEmptyRow();
            log.AddRequestRow( $" downloaded & writed parts {dpsr.PartsSuccessCount} of {dpsr.TotalParts}" );
            log.AddEmptyRow();
            log.AddRequestRow( $" elapsed: {elapsed}" );
            log.AddRequestRow( $"         file: '{dpsr.OutputFileName}'" );
            log.AddRequestRow( $"       size: {Extensions.GetSizeInMbFormatted( dpsr.TotalBytes )} mb" );
        }
        public static void StatusFinished( this DownloadRow row, TimeSpan elapsed )
        {
            row.SetStatus( DownloadStatus.Finished );
            var log = row.Log;
            log.AddEmptyRow();
            log.AddRequestRow( $" downloaded & writed parts {row.SuccessDownloadParts} of {row.TotalParts}" );
            log.AddEmptyRow();
            log.AddRequestRow( $" elapsed: {elapsed}" );
            log.AddRequestRow( $"         file: '{row.OutputFileName}'" );
            log.AddRequestRow( $"       size: {Extensions.GetSizeInMbFormatted( row.DownloadBytesLength )} mb" );
        }
        public static void StatusError( this DownloadRow row, Exception ex ) => row.StatusError( ex.ToString() );
        public static void StatusError( this DownloadRow row, string errorText, bool addEmptyRow = false )
        {
            if ( addEmptyRow )
            {
                row.Log.AddEmptyRow();
            }
            row.Log.AddRequestErrorRow( errorText );

            row.SetStatus( DownloadStatus.Error );
        }
        public static void StatusErrorWithNoLog( this DownloadRow row ) => row.SetStatus( DownloadStatus.Error );
        public static void StatusCanceled( this DownloadRow row )
        {
            row.SetStatus( DownloadStatus.Canceled );
            row.Log.AddEmptyRow();
            row.Log.AddRequestErrorRow( ".....Canceled by User....." );
        }

        public static void StatusErrorIfRenameOutputFile( this DownloadRow row, Exception renameOutputFileException )
        {
            if ( renameOutputFileException != null )
            {
                row.StatusError( $"Rename output file error => '{renameOutputFileException}'", true );
            }
        }

        [M(O.AggressiveInlining)] public static TimeSpan StopAndElapsed( this Stopwatch sw )
        {
            sw.Stop();
            return (sw.Elapsed);
        }

        [M(O.AggressiveInlining)] public static void Add< V >( this ConcurrentDictionary< DownloadRow, V > dict, DownloadRow row, V x, Action< V > processExistsValueAction ) 
            => dict.AddOrUpdate( row, x, (_row, _x) =>
            {
                processExistsValueAction?.Invoke( _x );
                return (x);
            });
        [M(O.AggressiveInlining)] public static void Remove< DownloadRow, V >( this ConcurrentDictionary< DownloadRow, V > dict, DownloadRow row ) => dict.TryRemove( row, out var _ );

        [M(O.AggressiveInlining)] public static void WaitAsyncAndDispose( this SemaphoreSlim semaphore, int millisecondsTimeout = 7_000 )
        {
            if ( semaphore != null )
            {
                semaphore.WaitAsync( millisecondsTimeout ).ContinueWith( t => semaphore.Dispose() );
            }
        }

        [M(O.AggressiveInlining)] public static bool RemoveEx< K, T >( this IDictionary< K, T > d, K k, out T t )
        {
#if NETCOREAPP
            return (d.Remove( k, out t ));
#else
            var suc = d.TryGetValue( k, out t );
            if ( suc )
            {
                d.Remove( k );
            }
            return (suc);
#endif
        }
    }
}
