using System;
using System.Windows.Input;

using m3u8.client__v2;
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
        public EditCommand( MainVM vm, MainWindow mainWindow )
        {
            _VM = vm;
            _MainWindow = mainWindow;
        }

        #region [.ICommand.]
#pragma warning disable CS0067
        public event EventHandler CanExecuteChanged;
#pragma warning restore
        public bool CanExecute( object parameter ) => (parameter is DownloadRow row) && !row.Status.IsRunningOrPaused();
        public void Execute( object parameter ) => Run( (DownloadRow) parameter );
        #endregion

        public async void Run( DownloadRow row, AddNewDownloadForm.TabPageKind? activeTabPageKind = null )
        {
            if ( (row == null) || row.Status.IsRunningOrPaused() ) return;

            var f = AddNewDownloadForm.Edit( _VM, row, _VM.OutputFileNamePatternProcessor, _VM.ReceivedAndWritedPartsProcessor, activeTabPageKind );
            {
                await f.ShowDialogEx();
                if ( f.Success && !row.Status.IsRunningOrPaused() )
                {
                    var tp = f.GetParamsTuple();
                    var suc = row.Update( tp );
                    if ( suc )
                    {
                        await _MainWindow.ChangeOutputDirectory( row, tp.OutputDirectory );
                        await _MainWindow.ChangeOutputFileName ( row, tp.OutputFileName  );
                    }
                }
            }
        }
    }
}
