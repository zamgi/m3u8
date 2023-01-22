using System;
using System.Diagnostics;
using System.Windows.Input;

using m3u8.download.manager.ui;

namespace m3u8.download.manager
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class ColumnsVisibilityEditorCommand : ICommand
    {
        private MainWindow _MainWindow;
        public ColumnsVisibilityEditorCommand( MainWindow mainWindow ) => _MainWindow = mainWindow;

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
                    Debug.WriteLine( "apply columns visibility" );
                    _MainWindow.SaveDownloadListColumnsInfo();
                }
            }
        }
    }
}
