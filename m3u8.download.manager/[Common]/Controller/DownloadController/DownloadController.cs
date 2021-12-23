using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using m3u8.download.manager.models;
using m3u8.download.manager.Properties;
using m3u8.ext;
using _m3u8_processor_ = m3u8.m3u8_processor_v2;
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
        private struct tuple
        {
            [M(O.AggressiveInlining)]
            public static tuple Create( m3u8_client mc, CancellationTokenSource cts
                                      , ManualResetEventSlim waitIfPausedEvent
                                      , IDownloadThreadsSemaphoreEx downloadThreadsSemaphore
                                      , int startOrderNumber ) 
                => new tuple() { mc = mc, cts = cts
                               , waitIfPausedEvent = waitIfPausedEvent
                               , downloadThreadsSemaphore = downloadThreadsSemaphore
                               , startOrderNumber = startOrderNumber };

            public m3u8_client                 mc                       { [M(O.AggressiveInlining)] get; private set; }
            public CancellationTokenSource     cts                      { [M(O.AggressiveInlining)] get; private set; }
            public ManualResetEventSlim        waitIfPausedEvent        { [M(O.AggressiveInlining)] get; private set; }
            public IDownloadThreadsSemaphoreEx downloadThreadsSemaphore { [M(O.AggressiveInlining)] get; private set; }
            public int                         startOrderNumber         { [M(O.AggressiveInlining)] get; private set; }
        }

        #region [.fields.]
        private const int MILLISECONDSDELAY_M3U8FILE_OUTPUT_PAUSE = 500; //3_000;
        private DownloadListModel                          _Model;
        private SettingsPropertyChangeController           _SettingsController;
        private ConcurrentDictionary< DownloadRow, tuple > _Dict;
        private cross_download_instance_restriction        _CrossDownloadInstanceRestriction;
        private interlocked_lock                           _ProcessCrossDownloadInstanceRestrictionLock;
        private int                                        _RealRunningCount;
        private download_threads_semaphore_factory         _DownloadThreadsSemaphoreFactory;        
        private DefaultConnectionLimitSaver                _DefaultConnectionLimitSaver;
        #endregion

        #region [.ctor().]
        public DownloadController( DownloadListModel model, SettingsPropertyChangeController sc )
        {
            _Model = model ?? throw (new ArgumentNullException( nameof(model) ));

            _SettingsController = sc ?? throw (new ArgumentNullException( nameof(sc) ));
            _SettingsController.SettingsPropertyChanged += SettingsController_PropertyChanged;

            _Dict = new ConcurrentDictionary< DownloadRow, tuple >();

            _CrossDownloadInstanceRestriction = new cross_download_instance_restriction( _SettingsController.MaxCrossDownloadInstance );
            _DownloadThreadsSemaphoreFactory  = new download_threads_semaphore_factory( _SettingsController.UseCrossDownloadInstanceParallelism,
                                                                                        _SettingsController.MaxDegreeOfParallelism );

            _DefaultConnectionLimitSaver = DefaultConnectionLimitSaver.Create( _SettingsController.MaxDegreeOfParallelism );            
        }

        public void Dispose()
        {
            m3u8_client_factory.ForceClearAndDisposeAll();

            _DefaultConnectionLimitSaver.Dispose();

            if ( _DownloadThreadsSemaphoreFactory != null )
            {
                _DownloadThreadsSemaphoreFactory.Dispose();
                _DownloadThreadsSemaphoreFactory = null;
            }
            if ( _SettingsController != null )
            {
                _SettingsController.SettingsPropertyChanged -= SettingsController_PropertyChanged;
                _SettingsController = null;
            }
        }
        #endregion

        #region [.static 'GetFileTextContent'.]
        public static Task< (m3u8_file_t m3u8File, Exception error) > GetFileTextContent( string m3u8FileUrlText, CancellationTokenSource cts = null )
        {
            #region [.url.]
            if ( !Extensions.TryGetM3u8FileUrl( m3u8FileUrlText?.Trim(), out var t ) )
            {
                return Task.FromResult( (default(m3u8_file_t), t.error) );
            }
            #endregion

            return (GetFileTextContent( t.m3u8FileUrl, cts ));
        }
        public static async Task< (m3u8_file_t m3u8File, Exception error) > GetFileTextContent( Uri m3u8FileUrl, CancellationTokenSource cts = null )
        {
            using ( var mc = m3u8_client_factory.Create( SettingsPropertyChangeController.SettingsDefault.RequestTimeoutByPart, attemptRequestCountByPart: 1 ) )
            {
                try
                {
                    var m3u8File = await mc.DownloadFile( m3u8FileUrl, cts?.Token );

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
                }
                break;

                case nameof(Settings.MaxCrossDownloadInstance):
                {
                    _CrossDownloadInstanceRestriction.SetMaxCrossDownloadInstance( settings.MaxCrossDownloadInstance );
                    await ProcessCrossDownloadInstanceRestriction( settings.MaxCrossDownloadInstance );
                }
                break;
            }
        }

        private static void DisposeExistsTupleWhenAdd2Dict( tuple t )
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
            try { t.cts.Cancel();                       } catch ( Exception ex ) { Debug.WriteLine( ex ); }
            try { t.cts.Dispose();                      } catch ( Exception ex ) { Debug.WriteLine( ex ); }
            try { t.mc .Dispose();                      } catch ( Exception ex ) { Debug.WriteLine( ex ); }
            try { t.waitIfPausedEvent.Set();            } catch ( Exception ex ) { Debug.WriteLine( ex ); }
            try { t.waitIfPausedEvent.Dispose();        } catch ( Exception ex ) { Debug.WriteLine( ex ); }
            try { t.downloadThreadsSemaphore.Dispose(); } catch ( Exception ex ) { Debug.WriteLine( ex ); }
        }
        [M(O.AggressiveInlining)] private void Fire_IsDownloadingChanged() => IsDownloadingChanged?.Invoke( (_Dict.Count != 0) );

        #region [.Process of a Changed 'MaxDegreeOfParallelism' props.]
        private async Task ProcessChangedMaxDegreeOfParallelism( int maxDegreeOfParallelism )
        {
            _DefaultConnectionLimitSaver.Reset( maxDegreeOfParallelism );

            var ph = new PausedHelper( _Dict, CancellationToken.None );
            var tuples = await ph.PausedAll_Started_Running_and_GetThem();

            await _DownloadThreadsSemaphoreFactory.ResetMaxDegreeOfParallelism( maxDegreeOfParallelism );

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
                    foreach ( var row in _Model.GetRows().ToArray() )
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
                                await PauseWithWait( row, CancellationToken.None );
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
                    foreach ( var row in _Model.GetRows().ToArray() )
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
                        await PausedExtraDownloads( d );
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
                        //await Task.Delay( millisecondsDelay, ct );
                    }
                }, ct ).ContinueWith( task => {
                    if ( !task.IsCanceled && task.IsFaulted ) // suppress cancel exception
                    {
                        throw (task.Exception);
                    }
                }, TaskContinuationOptions.ExecuteSynchronously );
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
            if ( !Extensions.TryGetM3u8FileUrl( row.Url, out var t ) )
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

            using ( var mc                       = m3u8_client_factory.Create( _SettingsController.GetCreateM3u8ClientParams() ) )
            using ( var cts                      = new CancellationTokenSource() )
            using ( var waitIfPausedEvent        = new ManualResetEventSlim( true, 0 ) )
            using ( var downloadThreadsSemaphore = _DownloadThreadsSemaphoreFactory.Get() )
            {
                var tup = tuple.Create( mc, cts, waitIfPausedEvent, downloadThreadsSemaphore, _Dict.Count );
                _Dict.Add( row, tup, DisposeExistsTupleWhenAdd2Dict ); Fire_IsDownloadingChanged();

                try
                {
                    var sw = Stopwatch.StartNew();

                    //-1-//
                    var m3u8File = await mc.DownloadFile( t.m3u8FileUrl, cts.Token );

                    row.SetTotalParts( m3u8File.Parts.Count );
                    row.Log.Output( in m3u8File );

                    //-2-//
                    await Task.Delay( MILLISECONDSDELAY_M3U8FILE_OUTPUT_PAUSE, cts.Token );
                    row.Log.Clear();

                    //-3-//
                    var anyErrorHappend = false;
                    var dpsr = await Task.Run( () =>
                    {
                        var rows_Dict = new Dictionary< int, LogRow >( m3u8File.Parts.Count );

                        var requestStepAction  = new _m3u8_processor_.RequestStepActionDelegate( p =>
                        {
                            row.SetStatus( DownloadStatus.Running );

                            var requestText = $"#{p.PartOrderNumber} of {p.TotalPartCount}). '{p.Part.RelativeUrlName}'...";
                            if ( p.Success )
                            {
                               var logRow = row.Log.AddRequestRow( requestText );
                               rows_Dict.Add( p.Part.OrderNumber, logRow );
                            }
                            else
                            {
                                anyErrorHappend = true;
                               row.Log.AddResponseErrorRow( requestText, p.Error.ToString() );
                            }
                        });
                        var responseStepAction = new _m3u8_processor_.ResponseStepActionDelegate( p =>
                        {
                            row.SetDownloadResponseStepParams( in p );

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
                                    logRow.SetResponseSuccess( "received" );
                                }
                            }
                        });

                        var veryFirstOutputFullFileName = row.GetOutputFullFileName();
                        row.SaveVeryFirstOutputFullFileName( veryFirstOutputFullFileName );

                        var ip = new _m3u8_processor_.DownloadPartsAndSaveInputParams()
                        {
                            mc                       = mc,
                            m3u8File                 = m3u8File,
                            OutputFileName           = veryFirstOutputFullFileName,
                            Cts                      = cts,
                            RequestStepAction        = requestStepAction,
                            ResponseStepAction       = responseStepAction,
                            MaxDegreeOfParallelism   = _SettingsController.MaxDegreeOfParallelism,
                            WaitIfPausedEvent        = waitIfPausedEvent,
                            DownloadThreadsSemaphore = downloadThreadsSemaphore,
                            WaitingIfPaused          = () => row.SetStatus( DownloadStatus.Paused ),
                        };

                        var result = _m3u8_processor_.DownloadPartsAndSave( in ip );
                        return (result);
                    });

                    //-4-//
                    _Dict.Remove( row ); Fire_IsDownloadingChanged();

                    #region [.remane output file if changed.]
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
                    #endregion

                    row.StatusFinished( in dpsr, sw.StopAndElapsed() );

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
                        Extensions.DeleteFiles_NoThrow( row.GetOutputFullFileNames() );
                        row.StatusCanceled();
                    }
                    else if ( ex is m3u8_Exception mex )
                    {
                        Extensions.DeleteFiles_NoThrow( row.GetOutputFullFileNames() );
                        row.StatusError( ex.Message );
                    }
                    else
                    {
                        row.StatusError( ex );
                    }
                }
            }

            Interlocked.Decrement( ref _RealRunningCount );

            #region [.cross download instance restriction.]
            await ProcessCrossDownloadInstanceRestriction( _CrossDownloadInstanceRestriction.GetMaxCrossDownloadInstance() );
            #endregion
        }
        public void Pause( DownloadRow row )
        {
            if ( (row != null) && _Dict.TryGetValue( row, out var t ) )
            {
                t.waitIfPausedEvent.Reset_NoThrow();
            }
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
            private ConcurrentDictionary< DownloadRow, tuple > _Dict;
            private CancellationToken _Ct;
            #endregion

            #region [.ctor().]
            public PausedHelper( ConcurrentDictionary< DownloadRow, tuple > dict, CancellationToken ct ) => (_Dict, _Ct) = (dict, ct);
            #endregion

            #region [.method's.]
            private static Result PausedAll_ByStatus_GetThem( ConcurrentDictionary< DownloadRow, tuple > dict, params DownloadStatus[] statuses )
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
                }
            }
            #endregion
        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    internal static class DownloadControllerExtensions
    {
        public static void Output( this LogListModel log, in m3u8_file_t m3u8File )
        {
            var lines = m3u8File.RawText?.Split( new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries )
                                .Where( line => !line.IsNullOrWhiteSpace() );
            if ( lines.AnyEx() )
            {
                log.BeginUpdate();
                {
                    log.Clear();
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
            log.AddRequestRow( $"       size: {(dpsr.TotalBytes >> 20).ToString( "0,0" )} mb" );
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
    }
}
