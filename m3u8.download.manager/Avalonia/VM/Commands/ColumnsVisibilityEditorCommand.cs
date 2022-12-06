using System;
using System.Windows.Input;

using m3u8.download.manager.Properties;
using m3u8.download.manager.ui;

namespace m3u8.download.manager
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class ColumnsVisibilityEditorCommand : ICommand
    {
        private Settings   _Settings;
        private MainWindow _MainWindow;
        public ColumnsVisibilityEditorCommand( MainVM vm, MainWindow mainWindow ) => (_Settings, _MainWindow) = (vm.SettingsController.Settings, mainWindow);

#pragma warning disable CS0067
        public event EventHandler CanExecuteChanged;
#pragma warning restore
        public bool CanExecute( object parameter ) => true;

        public async void Execute( object parameter )
        {
            var f = new ColumnsVisibilityEditor( _MainWindow.DownloadListDGV );
            {
                await f.ShowDialogEx();
                if ( f.Success )
                {
                    _Settings.DownloadListDGVColumnsVisibilityJson = ColumnsVisibilitySerializer.ToJSON( _MainWindow.DownloadListDGV.Columns );
                    _Settings.SaveNoThrow();
                }
            }            
        }
    }
}
