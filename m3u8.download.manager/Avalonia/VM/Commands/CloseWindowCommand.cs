using System;
using System.Windows.Input;

using Avalonia.Controls;

namespace m3u8.download.manager
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class CloseWindowCommand : ICommand
    {
        private Window _Window;
        public CloseWindowCommand( Window window ) => _Window = window;

#pragma warning disable CS0067
        public event EventHandler CanExecuteChanged;
#pragma warning restore
        public bool CanExecute( object parameter ) => _Window.IsInitialized;
        public void Execute( object parameter ) => _Window.Close();
    }
}
