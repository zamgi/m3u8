using System;
using System.Windows.Input;

using m3u8.download.manager.models;
using m3u8.download.manager.ui;

namespace m3u8.download.manager
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class ChangeSettingsParams4DownloadRowCommand : ICommand
    {
        private MainVM _VM;
        private MainWindow _MainWindow;
        private OutputFileNamePatternProcessor _OutputFileNamePatternProcessor;
        public ChangeSettingsParams4DownloadRowCommand( MainVM vm, MainWindow mainWindow, OutputFileNamePatternProcessor outputFileNamePatternProcessor )
        {
            _VM = vm;
            _MainWindow = mainWindow;
            _OutputFileNamePatternProcessor = outputFileNamePatternProcessor;
        }

        #region [.ICommand.]
#pragma warning disable CS0067
        public event EventHandler CanExecuteChanged;
#pragma warning restore
        public bool CanExecute( object parameter ) => (parameter is DownloadRow row) && row.Status.IsRunningOrPaused();
        public void Execute( object parameter ) => ChangeSettingsParams4DownloadRow( (DownloadRow) parameter );
        #endregion

        public async void ChangeSettingsParams4DownloadRow( DownloadRow row, ChangeSettingsParams4DownloadRowForm.TabPageKind? activeTabPageKind = null )
        {
            if ( (row == null) || !row.Status.IsRunningOrPaused() ) return;

            var f = ChangeSettingsParams4DownloadRowForm.Edit( _VM, row, _OutputFileNamePatternProcessor, activeTabPageKind );
            {
                await f.ShowDialogEx();
                if ( f.Success && row.Status.IsRunningOrPaused() )
                {
                    var tp = f.GetParamsTuple();

                    var suc = _VM.DownloadController.TryChangeSettings( row, tp.WebProxyInfo, tp.Timeout, tp.AttemptRequestCount );
                    if ( suc ) //must be RunningOrPaused if suc
                    {
                        await _MainWindow.ChangeOutputDirectory( row, tp.OutputDirectory );
                        await _MainWindow.ChangeOutputFileName ( row, tp.OutputFileName  );
                    }
                    else if ( !row.Status.IsRunningOrPaused() )
                    {
                        row.Update( (f.M3u8FileUrl, tp.RequestHeaders, tp.WebProxyInfo, tp.Timeout, tp.AttemptRequestCount, tp.LiveStreamMaxFileSizeInBytes) );
                        await _MainWindow.ChangeOutputDirectory( row, tp.OutputDirectory );
                        await _MainWindow.ChangeOutputFileName ( row, tp.OutputFileName  );
                    }
                    else
                    {
                        await _MainWindow.MessageBox_ShowError( $"Failed change settings for download row: '{row.OutputFileName}'.", $"Change settings, / '{row.OutputFileName}' /" );
                    }
                }
            }
        }
    }
}
