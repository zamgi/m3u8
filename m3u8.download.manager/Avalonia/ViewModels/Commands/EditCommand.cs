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
        private OutputFileNamePatternProcessor _OutputFileNamePatternProcessor;
        public EditCommand( MainVM vm )
        {
            _VM = vm;
            _OutputFileNamePatternProcessor = new OutputFileNamePatternProcessor();
        }

        #region [.ICommand.]
#pragma warning disable CS0067
        public event EventHandler CanExecuteChanged;
#pragma warning restore
        public bool CanExecute( object parameter ) => (parameter is DownloadRow row) && !row.Status.IsRunningOrPaused();
        public void Execute( object parameter ) => EditDownload( (DownloadRow) parameter );
        #endregion

        public async void EditDownload( DownloadRow row )
        {
            if ( (row == null) || row.Status.IsRunningOrPaused() ) return;

            var f = AddNewDownloadForm.Edit( _VM, row, _OutputFileNamePatternProcessor );
            {
                await f.ShowDialogEx();
                if ( f.Success && !row.Status.IsRunningOrPaused() )
                {
                    row.Update( f.M3u8FileUrl, f.GetRequestHeaders(), f.GetOutputFileName(), f.GetOutputDirectory(), f.IsLiveStream, f.LiveStreamMaxFileSizeInBytes );
                }
            }
        }
    }
}
