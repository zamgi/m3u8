using System;
using System.Windows.Input;

using m3u8.download.manager.models;
using m3u8.download.manager.ui;

namespace m3u8.download.manager
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class EditCommand : ICommand
    {
        private MainVM _VM;
        private MainWindow _MainWindow;
        private OutputFileNamePatternProcessor _OutputFileNamePatternProcessor;
        public EditCommand( MainVM vm, MainWindow mainWindow )
        {
            _VM = vm;
            _MainWindow = mainWindow;
            _OutputFileNamePatternProcessor = new OutputFileNamePatternProcessor();
        }

        #region [.ICommand.]
#pragma warning disable CS0067
        public event EventHandler CanExecuteChanged;
#pragma warning restore
        public bool CanExecute( object parameter ) => (parameter is DownloadRow row) && !row.Status.IsRunningOrPaused();
        public void Execute( object parameter ) => EditDownload( (DownloadRow) parameter );
        #endregion

        public async void EditDownload( DownloadRow row, AddNewDownloadForm.TabPageKind? activeTabPageKind = null )
        {
            if ( (row == null) || row.Status.IsRunningOrPaused() ) return;

            var f = AddNewDownloadForm.Edit( _VM, row, _OutputFileNamePatternProcessor, activeTabPageKind );
            {
                await f.ShowDialogEx();
                if ( f.Success && !row.Status.IsRunningOrPaused() )
                {
                    var tp = f.GetParamsTuple();
                    row.Update( tp );
                    await _MainWindow.ChangeOutputDirectory( row, tp.OutputDirectory );
                    await _MainWindow.ChangeOutputFileName ( row, tp.OutputFileName  );
                }
            }
        }
    }
}
