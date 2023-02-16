using System;
using System.Windows.Input;

using m3u8.download.manager.ui;

namespace m3u8.download.manager
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class ParallelismCommand : ICommand
    {
        private MainVM _VM;
        public ParallelismCommand( MainVM vm ) => _VM = vm;

#pragma warning disable CS0067
        public event EventHandler CanExecuteChanged;
#pragma warning restore
        public bool CanExecute( object parameter ) => true;

        public async void Execute( object parameter )
        {
            var f = new ParallelismForm( _VM.DownloadController );
            {
                var st = _VM.SettingsController.Settings;

                f.UseCrossDownloadInstanceParallelism = st.UseCrossDownloadInstanceParallelism;
                f.MaxDegreeOfParallelism              = st.MaxDegreeOfParallelism;
                f.SetMaxCrossDownloadInstance( st.MaxCrossDownloadInstance, st.MaxCrossDownloadInstanceSaved );
                f.SetMaxSpeedThresholdInMbps ( st.MaxSpeedThresholdInMbps , st.MaxSpeedThresholdInMbpsSaved  );

                await f.ShowDialogEx();
                if ( f.Success )
                {
                    st.UseCrossDownloadInstanceParallelism = f.UseCrossDownloadInstanceParallelism;
                    st.MaxDegreeOfParallelism              = f.MaxDegreeOfParallelism;
                    st.MaxCrossDownloadInstance            = f.MaxCrossDownloadInstance;
                    st.MaxCrossDownloadInstanceSaved       = f.MaxCrossDownloadInstanceSaved;
                    st.MaxSpeedThresholdInMbps             = f.MaxSpeedThresholdInMbps;
                    st.MaxSpeedThresholdInMbpsSaved        = f.MaxSpeedThresholdInMbpsSaved;
                    _VM.SettingsController.SaveNoThrow_IfAnyChanged();
                }
            }            
        }
    }
}
