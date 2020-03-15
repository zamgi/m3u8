using System;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;
using m3u8.download.manager.infrastructure;
using m3u8.download.manager.ui;

namespace m3u8.download.manager
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class FileNameExcludesWordsEditorCommand : ICommand
    {
        private MainVM _VM;
        public FileNameExcludesWordsEditorCommand( MainVM vm ) => _VM = vm;

#pragma warning disable CS0067
        public event EventHandler CanExecuteChanged;
#pragma warning restore
        public bool CanExecute( object parameter ) => true;

        public async void Execute( object parameter )
        {
            var f = new FileNameExcludesWordsEditor( NameCleaner.ExcludesWords );
            {
                await f.ShowDialogEx();
                if ( f.Success )
                {
                    NameCleaner.ResetExcludesWords( f.GetFileNameExcludesWords() );

                    var settings = _VM.SettingsController.Settings;
                    if ( settings.NameCleanerExcludesWords == null )
                    {
                        settings.NameCleanerExcludesWords = new StringCollection();
                    }
                    else
                    {
                        settings.NameCleanerExcludesWords.Clear();
                    }
                    settings.NameCleanerExcludesWords.AddRange( NameCleaner.ExcludesWords.ToArray() );
                    settings.SaveNoThrow();
                }
            }            
        }
    }
}
