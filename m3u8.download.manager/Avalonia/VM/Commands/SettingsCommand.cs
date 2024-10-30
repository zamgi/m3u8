using System;
using System.Threading.Tasks;
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
        public async void Execute( object parameter ) => await Show( _VM, SettingsForm.SettingsTabEnum.Other );

        public static async Task Show( MainVM vm, SettingsForm.SettingsTabEnum settingsTab )
        {
            var f = new SettingsForm( vm, settingsTab );
            {
                var st = vm.SettingsController.Settings;

                #region [.parallelism.]
                f.UseCrossDownloadInstanceParallelism = st.UseCrossDownloadInstanceParallelism;
                f.MaxDegreeOfParallelism              = st.MaxDegreeOfParallelism;
                f.SetMaxCrossDownloadInstance( st.MaxCrossDownloadInstance, st.MaxCrossDownloadInstanceSaved );
                f.SetMaxSpeedThresholdInMbps ( st.MaxSpeedThresholdInMbps , st.MaxSpeedThresholdInMbpsSaved  );
                #endregion

                #region [.other.]
                f.AttemptRequestCountByPart              = st.AttemptRequestCountByPart;
                f.RequestTimeoutByPart                   = st.RequestTimeoutByPart;
                f.ShowOnlyRequestRowsWithErrors          = st.ShowOnlyRequestRowsWithErrors;
                f.ShowDownloadStatisticsInMainFormTitle  = st.ShowDownloadStatisticsInMainFormTitle;
                f.ShowAllDownloadsCompleted_Notification = st.ShowAllDownloadsCompleted_Notification;
                f.OutputFileExtension                    = st.OutputFileExtension;
                f.UniqueUrlsOnly                         = st.UniqueUrlsOnly;
                #endregion

                await f.ShowDialogEx();
                if ( f.Success )
                {
                    #region [.parallelism.]
                    st.UseCrossDownloadInstanceParallelism = f.UseCrossDownloadInstanceParallelism;
                    st.MaxDegreeOfParallelism              = f.MaxDegreeOfParallelism;
                    st.MaxCrossDownloadInstance            = f.MaxCrossDownloadInstance;
                    st.MaxCrossDownloadInstanceSaved       = f.MaxCrossDownloadInstanceSaved;
                    st.MaxSpeedThresholdInMbps             = f.MaxSpeedThresholdInMbps;
                    st.MaxSpeedThresholdInMbpsSaved        = f.MaxSpeedThresholdInMbpsSaved;
                    #endregion

                    #region [.other.]
                    st.AttemptRequestCountByPart              = f.AttemptRequestCountByPart;
                    st.RequestTimeoutByPart                   = f.RequestTimeoutByPart;
                    st.ShowOnlyRequestRowsWithErrors          = f.ShowOnlyRequestRowsWithErrors;
                    st.ShowDownloadStatisticsInMainFormTitle  = f.ShowDownloadStatisticsInMainFormTitle;
                    st.ShowAllDownloadsCompleted_Notification = f.ShowAllDownloadsCompleted_Notification;
                    st.OutputFileExtension                    = f.OutputFileExtension;
                    st.UniqueUrlsOnly                         = f.UniqueUrlsOnly;
                    #endregion

                    vm.SettingsController.SaveNoThrow_IfAnyChanged();
                }
            }
        }
    }
}
