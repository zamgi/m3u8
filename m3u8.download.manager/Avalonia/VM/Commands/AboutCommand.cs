using System;
using System.Linq;
using System.Windows.Input;

using Avalonia.Media;
using MessageBox.Avalonia.Enums;

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
            const string CAPTION = "about";

            var text = $"\"{AssemblyInfoHelper.AssemblyTitle}\"" + Environment.NewLine +
                       AssemblyInfoHelper.AssemblyCopyright + Environment.NewLine +
                       Environment.NewLine +
                       $"Version {AssemblyInfoHelper.AssemblyVersion}, ({AssemblyInfoHelper.AssemblyLastWriteTime})" +
                       Environment.NewLine +
                       Environment.NewLine +
                       "Shortcut's:" + Environment.NewLine +
                       " Ctrl+C: Copy selected download url to clipboard" + Environment.NewLine +
                       " Ctrl+B: Browse output file (if exists)" + Environment.NewLine +
                       " Ctrl+D: Minimized application window" + Environment.NewLine +
                       " Ctrl+O: Open output file (if exists)" + Environment.NewLine +
                       " Ctrl+P: Pause selected download" + Environment.NewLine +
                       " Ctrl+S: Start selected download" + Environment.NewLine +
                       " Ctrl+V: Paste download url from clipboard" + Environment.NewLine +
                       " Ctrl+W: Exit application" + Environment.NewLine +
                       " Ctrl+Z: Cancel selected download" + Environment.NewLine +
                       " Insert: Open add new download dialog" + Environment.NewLine +
                       " Delete: Delete download (with or without output file)" + Environment.NewLine +
                       " Enter:  Open rename output file dialog" + Environment.NewLine +
                       " F1:     About dialog" + Environment.NewLine;

            var fontFamilies = (from fn in FontManager.Current.GetInstalledFontFamilyNames()
                                select new FontFamily( fn )
                              ).ToArray();

            var fontFamily = (from f in fontFamilies
                              where (f.Name.EqualIgnoreCase( "Courier New"      )) || //Windows
                                    (f.Name.EqualIgnoreCase( "Consolas"         )) || //Windows
                                    (f.Name.EqualIgnoreCase( "Book"             )) || //Linux
                                    (f.Name.EqualIgnoreCase( "DejaVu Sans Mono" ))    //Linux
                              select f
                             ).FirstOrDefault();
            if ( fontFamily == null )
            {
                fontFamily = FontFamily.Default;
            }
            if ( fontFamily != null )
            {
                var msgbox = Extensions.Create_MsBoxStandardWindow( text, CAPTION, ButtonEnum.Ok, Icon.Info, fontFamily );
                await msgbox.ShowEx();
            }
            else
            {
                await Extensions.MessageBox_ShowInformation( text, CAPTION );
            }            
        }
    }
}
