using System;
using System.Windows.Input;
using m3u8.download.manager.infrastructure;
using m3u8.download.manager.Properties;
using m3u8.download.manager.ui;

namespace m3u8.download.manager
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class FileNameExcludesWordsEditorCommand : ICommand
    {
        private Settings _Settings;
        public FileNameExcludesWordsEditorCommand( MainVM vm ) => _Settings = vm.SettingsController.Settings;

#pragma warning disable CS0067
        public event EventHandler CanExecuteChanged;
#pragma warning restore
        public bool CanExecute( object parameter ) => true;

        public async void Execute( object parameter )
        {
            using var f = new FileNameExcludesWordsEditor( NameCleaner.ExcludesWords );
            {
                await f.ShowDialogEx();
                if ( f.Success )
                {
                    NameCleaner.ResetExcludesWords( f.GetFileNameExcludesWords() );
                    _Settings.ResetNameCleanerExcludesWords( NameCleaner.ExcludesWords );
                    _Settings.SaveNoThrow();
                }
            }            
        }
    }
}
