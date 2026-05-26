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
        public async void Execute( object parameter ) => await SettingsCommand.Run( _VM, SettingsForm.SettingsTabEnum.Parallelism );
    }
}
