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

                f.AttemptRequestCountByPart             = settings.AttemptRequestCountByPart;
                f.RequestTimeoutByPart                  = settings.RequestTimeoutByPart;
                f.ShowOnlyRequestRowsWithErrors         = settings.ShowOnlyRequestRowsWithErrors;
                f.UniqueUrlsOnly                        = settings.UniqueUrlsOnly;
                f.ShowDownloadStatisticsInMainFormTitle = settings.ShowDownloadStatisticsInMainFormTitle;
                f.OutputFileExtension                   = settings.OutputFileExtension;

                await f.ShowDialogEx();
                if ( f.Success )
                {
                    settings.AttemptRequestCountByPart             = f.AttemptRequestCountByPart;
                    settings.RequestTimeoutByPart                  = f.RequestTimeoutByPart;
                    settings.ShowOnlyRequestRowsWithErrors         = f.ShowOnlyRequestRowsWithErrors;
                    settings.UniqueUrlsOnly                        = f.UniqueUrlsOnly;
                    settings.ShowDownloadStatisticsInMainFormTitle = f.ShowDownloadStatisticsInMainFormTitle;
                    settings.OutputFileExtension                   = f.OutputFileExtension;
                    settings.SaveNoThrow();
                }
            }            
        }
    }
}
