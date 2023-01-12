using System;
using System.Windows.Input;

using m3u8.download.manager.ui;

namespace m3u8.download.manager
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class SettingsCommand : ICommand
    {
        private MainVM _VM;
        public SettingsCommand( MainVM vm ) => _VM = vm;

#pragma warning disable CS0067
        public event EventHandler CanExecuteChanged;
#pragma warning restore
        public bool CanExecute( object parameter ) => true;

        public async void Execute( object parameter )
        {
            var f = new SettingsForm( _VM.DownloadController );
            {
                var settings = _VM.SettingsController.Settings;

                f.AttemptRequestCountByPart              = settings.AttemptRequestCountByPart;
                f.RequestTimeoutByPart                   = settings.RequestTimeoutByPart;
                f.ShowOnlyRequestRowsWithErrors          = settings.ShowOnlyRequestRowsWithErrors;
                f.ShowDownloadStatisticsInMainFormTitle  = settings.ShowDownloadStatisticsInMainFormTitle;
                f.ShowAllDownloadsCompleted_Notification = settings.ShowAllDownloadsCompleted_Notification;
                f.OutputFileExtension                    = settings.OutputFileExtension;
                f.UniqueUrlsOnly                         = settings.UniqueUrlsOnly;

                await f.ShowDialogEx();
                if ( f.Success )
                {
                    settings.AttemptRequestCountByPart              = f.AttemptRequestCountByPart;
                    settings.RequestTimeoutByPart                   = f.RequestTimeoutByPart;
                    settings.ShowOnlyRequestRowsWithErrors          = f.ShowOnlyRequestRowsWithErrors;
                    settings.ShowDownloadStatisticsInMainFormTitle  = f.ShowDownloadStatisticsInMainFormTitle;
                    settings.ShowAllDownloadsCompleted_Notification = f.ShowAllDownloadsCompleted_Notification;
                    settings.OutputFileExtension                    = f.OutputFileExtension;
                    settings.UniqueUrlsOnly                         = f.UniqueUrlsOnly;
                    settings.SaveNoThrow();
                }
            }            
        }
    }
}
