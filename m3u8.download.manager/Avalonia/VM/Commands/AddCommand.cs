using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

using Avalonia.Threading;

using m3u8.download.manager.infrastructure;
using m3u8.download.manager.ipc;
using m3u8.download.manager.ui;

using X = (string m3u8FileUrl, string requestHeaders, bool autoStartDownload);

namespace m3u8.download.manager
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class AddCommand : ICommand
    {
        private MainVM _VM;
        private OutputFileNamePatternProcessor _OutputFileNamePatternProcessor;
        public AddCommand( MainVM vm )
        {
            _VM = vm;
            _OutputFileNamePatternProcessor = new OutputFileNamePatternProcessor();

            PipeIPC.NamedPipeServer__Input.ReceivedInputParamsArray += NamedPipeServer__Input_ReceivedInputParamsArray;
        }
        private async void NamedPipeServer__Input_ReceivedInputParamsArray( X[] array ) => await Dispatcher.UIThread.InvokeAsync( () => AddNewDownloads( array ) );     

        #region [.ICommand.]
#pragma warning disable CS0067
        public event EventHandler CanExecuteChanged;
#pragma warning restore
        public bool CanExecute( object parameter ) => true;
        public void Execute( object parameter ) => AddNewDownload( (null, null, false) );
        #endregion

        public void AddNewDownloads( X[] array )
        {
            var p = (m3u8FileUrls     : (from t in array select (t.m3u8FileUrl, t.requestHeaders)).ToArray(), 
                     autoStartDownload: array.FirstOrDefault().autoStartDownload);
            AddNewDownloads( p );
        }
        public async void AddNewDownloads( (IReadOnlyCollection< (string url, string requestHeaders) > m3u8FileUrls, bool autoStartDownload) p ) //, bool forceShowEmptyDialog )
        {
            if ( p.m3u8FileUrls.AnyEx() )
            {
                if ( p.m3u8FileUrls.Count == 1 )
                {
                    var frt = p.m3u8FileUrls.First();
                    AddNewDownload( (frt.url, frt.requestHeaders, p.autoStartDownload) );
                }
                else
                {
                    var action = new Action< X, (int n, int total) >( (X tp, (int n, int total) seriesInfo ) => AddNewDownload( tp, seriesInfo ) );

                    var n     = p.m3u8FileUrls.Count;
                    var count = n;
                    foreach ( var t in p.m3u8FileUrls.Reverse() )
                    {
                        var seriesInfo = (n--, count);
                        await Dispatcher.UIThread.InvokeAsync( () => action( (t.url, t.requestHeaders, p.autoStartDownload), seriesInfo ) );
                        //---this.BeginInvoke( action, (t.url, t.requestHeaders, p.autoStartDownload), seriesInfo );
                    }
                }
            }
            else //if ( forceShowEmptyDialog )
            {
                AddNewDownload( (null, null, false) );
            }
        }
        public async void AddNewDownload( X p, (int n, int total)? seriesInfo = null )
        {
            var suc = BrowserIPC.ExtensionRequestHeader.Try2Dict( p.requestHeaders, out var requestHeaders );
            Debug.Assert( suc || p.requestHeaders.IsNullOrEmpty() );

            if ( p.autoStartDownload && !p.m3u8FileUrl.IsNullOrWhiteSpace() &&
                 FileNameCleaner4UI.TryGetOutputFileNameByUrl( p.m3u8FileUrl, out var outputFileName ) 
               )
            {
                if ( _VM.SettingsController.UniqueUrlsOnly && !_VM.DownloadListModel.ContainsUrl( p.m3u8FileUrl ) )
                {
                    var row = _VM.DownloadListModel.AddRow( (p.m3u8FileUrl, requestHeaders, outputFileName, _VM.SettingsController.OutputFileDirectory) );
                    _VM.DownloadController.Start( row );
                }
                return;
            }

            var f = AddNewDownloadForm.Show( _VM, p.m3u8FileUrl, requestHeaders, _OutputFileNamePatternProcessor, seriesInfo );
            {
                await f.ShowDialogEx();
                if ( f.Success )
                {
                    var row = _VM.DownloadListModel.AddRow( (f.M3u8FileUrl, f.GetRequestHeaders(), f.GetOutputFileName(), f.GetOutputDirectory(), f.IsLiveStream, f.LiveStreamMaxFileSizeInBytes) );
                    if ( f.AutoStartDownload )
                    {
                        _VM.DownloadController.Start( row );
                    }
                }
            }

            //if ( AddNewDownloadForm.TryGetOpenedForm( out var openedForm ) )
            //{
            //    openedForm.ActivateAfterCloseOther();
            //}
        }
    }
}
