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
            var f = new SettingsForm( _VM );
            {
                var st = _VM.SettingsController.Settings;

                f.AttemptRequestCountByPart              = st.AttemptRequestCountByPart;
                f.RequestTimeoutByPart                   = st.RequestTimeoutByPart;
                f.ShowOnlyRequestRowsWithErrors          = st.ShowOnlyRequestRowsWithErrors;
                f.ShowDownloadStatisticsInMainFormTitle  = st.ShowDownloadStatisticsInMainFormTitle;
                f.ShowAllDownloadsCompleted_Notification = st.ShowAllDownloadsCompleted_Notification;
                f.OutputFileExtension                    = st.OutputFileExtension;
                f.UniqueUrlsOnly                         = st.UniqueUrlsOnly;

                await f.ShowDialogEx();
                if ( f.Success )
                {
                    st.AttemptRequestCountByPart              = f.AttemptRequestCountByPart;
                    st.RequestTimeoutByPart                   = f.RequestTimeoutByPart;
                    st.ShowOnlyRequestRowsWithErrors          = f.ShowOnlyRequestRowsWithErrors;
                    st.ShowDownloadStatisticsInMainFormTitle  = f.ShowDownloadStatisticsInMainFormTitle;
                    st.ShowAllDownloadsCompleted_Notification = f.ShowAllDownloadsCompleted_Notification;
                    st.OutputFileExtension                    = f.OutputFileExtension;
                    st.UniqueUrlsOnly                         = f.UniqueUrlsOnly;
                    _VM.SettingsController.SaveNoThrow_IfAnyChanged();
                }
            }            
        }
    }
}
