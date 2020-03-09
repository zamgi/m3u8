using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

using Avalonia.Threading;
using m3u8.download.manager.infrastructure;
using m3u8.download.manager.ipc;
using m3u8.download.manager.ui;

namespace m3u8.download.manager
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class AddCommand : ICommand
    {
        private MainVM _VM;
        public AddCommand( MainVM vm )
        {
            _VM = vm;

            PipeIPC.NamedPipeServer__in.ReceivedInputParamsArray += NamedPipeServer__in_ReceivedInputParamsArray;
        }
        private async void NamedPipeServer__in_ReceivedInputParamsArray( (string m3u8FileUrl, bool autoStartDownload)[] array )
            => await Dispatcher.UIThread.InvokeAsync( () => AddNewDownloads( array ) );

        #region [.ICommand.]
#pragma warning disable CS0067
        public event EventHandler CanExecuteChanged;
#pragma warning restore
        public bool CanExecute( object parameter ) => true;
        public void Execute( object parameter ) => AddNewDownload( (null, false) );
        #endregion

        public void AddNewDownloads( (string m3u8FileUrl, bool autoStartDownload)[] array )
        {
            var p = (m3u8FileUrls     : (from t in array select t.m3u8FileUrl).ToArray(), 
                     autoStartDownload: array.FirstOrDefault().autoStartDownload);
            AddNewDownloads( p );
        }
        public async void AddNewDownloads( (IReadOnlyCollection< string > m3u8FileUrls, bool autoStartDownload) p ) //, bool forceShowEmptyDialog )
        {
            if ( p.m3u8FileUrls.AnyEx() )
            {
                if ( p.m3u8FileUrls.Count == 1 )
                {
                    AddNewDownload( (p.m3u8FileUrls.First(), p.autoStartDownload) );
                }
                else
                {
                    var action = new Action< (string m3u8FileUrl, bool autoStartDownload), (int n, int total) >( 
                        ((string m3u8FileUrl, bool autoStartDownload) tp, (int n, int total) seriesInfo ) => AddNewDownload( tp, seriesInfo ) );

                    var n     = p.m3u8FileUrls.Count;
                    var count = n;
                    foreach ( var m3u8FileUrl in p.m3u8FileUrls.Reverse() )
                    {
                        var seriesInfo = (n--, count);
                        await Dispatcher.UIThread.InvokeAsync( () => action( (m3u8FileUrl, p.autoStartDownload), seriesInfo ) );
                        //---this.BeginInvoke( action, (m3u8FileUrl, p.autoStartDownload), seriesInfo );
                    }
                }
            }
            else //if ( forceShowEmptyDialog )
            {
                AddNewDownload( ((string) null, false) );
            }
        }
        public async void AddNewDownload( (string m3u8FileUrl, bool autoStartDownload) p, (int n, int total)? seriesInfo = null )
        {
            if ( p.autoStartDownload && !p.m3u8FileUrl.IsNullOrWhiteSpace() &&
                 FileNameCleaner.TryGetOutputFileNameByUrl( p.m3u8FileUrl, out var outputFileName ) 
               )
            {
                if ( _VM.SettingsController.UniqueUrlsOnly && !_VM.DownloadListModel.ContainsUrl( p.m3u8FileUrl ) )
                {
                    var row = _VM.DownloadListModel.AddRow( (p.m3u8FileUrl, outputFileName, _VM.SettingsController.OutputFileDirectory) );
                    //downloadListUC.SelectDownloadRow( row );
                    _VM.DownloadController.Start( row );
                }
                return;
            }

            var f = new AddNewDownloadForm( _VM, p.m3u8FileUrl, seriesInfo );
            {
                await f.ShowDialog( Extensions.GetMainWindow() );
                if ( f.Success )
                {
                    var row = _VM.DownloadListModel.AddRow( (f.M3u8FileUrl, f.GetOutputFileName(), f.GetOutputDirectory()) );
                    //downloadListUC.SelectDownloadRow( row );
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
