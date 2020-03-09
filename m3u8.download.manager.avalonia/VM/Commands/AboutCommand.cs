using System;
using System.Windows.Input;

namespace m3u8.download.manager
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class AboutCommand : ICommand
    {
#pragma warning disable CS0067
        public event EventHandler CanExecuteChanged;
#pragma warning restore
        public bool CanExecute( object parameter ) => true;

        public async void Execute( object parameter )
        {
            var text = $"\"{AssemblyInfoHelper.AssemblyTitle}\"" + Environment.NewLine +
                       //AssemblyInfoHelper.AssemblyProduct + Environment.NewLine +
                       AssemblyInfoHelper.AssemblyCopyright + Environment.NewLine +
                       //AssemblyInfoHelper.AssemblyCompany + Environment.NewLine +
                       //AssemblyInfoHelper.AssemblyDescription + Environment.NewLine +
                       Environment.NewLine +
                       $"Version {AssemblyInfoHelper.AssemblyVersion}, ({AssemblyInfoHelper.AssemblyLastWriteTime})" +
                       Environment.NewLine +
                       Environment.NewLine +
                       "Shortcut's:" + Environment.NewLine +
                       "  Ctrl+C:     Copy selected download url to clipboard" + Environment.NewLine +
                       "  Ctrl+B:     Browse output file (if exists)" + Environment.NewLine +
                       "  Ctrl+D:     Minimized application window" + Environment.NewLine +
                       "  Ctrl+O:     Open output file (if exists)" + Environment.NewLine +
                       "  Ctrl+P:     Pause selected download" + Environment.NewLine +
                       "  Ctrl+S:     Start selected download" + Environment.NewLine +
                       "  Ctrl+V:     Paste download url from clipboard" + Environment.NewLine +
                       "  Ctrl+W:     Exit application" + Environment.NewLine +
                       "  Ctrl+Z:     Cancel selected download" + Environment.NewLine +
                       "  Insert:     Open add new download dialog" + Environment.NewLine +
                       "  Delete:     Delete download (with or without output file)" + Environment.NewLine +
                       "  Enter:      Open rename output file dialog" + Environment.NewLine +
                       "  F1:         About dialog" + Environment.NewLine;
            await Extensions.MessageBox_ShowInformation( text, "about" );
        }
    }
}
