using System;
using System.Windows.Input;

using Avalonia.Media;

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

            var text = $"\"{(AssemblyInfoHelper.AssemblyTitle ?? "-")}\" {AssemblyInfoHelper.FrameWorkName}" +
#if DEBUG
                       " / (DEBUG)" +
#endif
                       Environment.NewLine +
                       (AssemblyInfoHelper.AssemblyCopyright ?? "-") + Environment.NewLine +
                       Environment.NewLine +
                       $"Version {(AssemblyInfoHelper.AssemblyVersion ?? "-")}, ({(AssemblyInfoHelper.AssemblyLastWriteTime ?? "-")})" +
                       Environment.NewLine +
                       Environment.NewLine +
                       "Shortcut's:" + Environment.NewLine +
                       "  Ctrl+C: Copy selected download url to clipboard" + Environment.NewLine +
                       "  Ctrl+B: Browse output file (if exists)" + Environment.NewLine +
                       "  Ctrl+D: Minimized application window" + Environment.NewLine +
                       "  Ctrl+O: Open output file (if exists)" + Environment.NewLine +
                       "  Ctrl+P: Pause selected download" + Environment.NewLine +
                       "  Ctrl+S: Start selected download" + Environment.NewLine +
                       "  Ctrl+V: Paste download url from clipboard" + Environment.NewLine +
                       "  Ctrl+W: Exit application" + Environment.NewLine +
                       "  Ctrl+X: Cancel selected download" + Environment.NewLine +
                       "  Ctrl+Z: Undo deleted download" + Environment.NewLine +
                       "  Insert: Open add new download dialog" + Environment.NewLine +
                       "  Delete: Delete download (with or without output file)" + Environment.NewLine +
                       "  Enter:  Open rename output file dialog" + Environment.NewLine +
                       "  F1:     About dialog" + Environment.NewLine +
                       "  (Ctrl+Shift+G:  Collect Garbage)" + Environment.NewLine;

            if ( !FontHelper.TryGetMonospace( out var fontFamily ) )
            {
                fontFamily = FontFamily.Default;
            }


            await MessageBoxWindow.Show( text, CAPTION, MessageBoxWindow.ButtonTypeEnum.Ok, MessageBoxWindow.IconTypeEnum.None, fontFamily/*, new Size( 500, 250 )*/ );

            #region comm.
            //try
            //{
            //    var msgbox = Extensions.Create_MsBoxStandardWindow( text, CAPTION, ButtonEnum.Ok, Icon.None/*Info*/, fontFamily );
            //    await msgbox.ShowEx();
            //}
            //catch ( Exception ex )
            //{
            //    Debug.WriteLine( ex );

            //    var wnd = new Window()
            //    {
            //        Title                 = CAPTION,
            //        Content               = text,
            //        SizeToContent         = SizeToContent.WidthAndHeight,
            //        Padding               = new Thickness( 25 ),
            //        WindowStartupLocation = WindowStartupLocation.CenterOwner,
            //        ShowInTaskbar         = false,
            //        CanMaximize           = false,
            //        CanMinimize           = false,
            //        //WindowDecorations     = WindowDecorations.Full,
            //        //Icon                  = new WindowIcon( ResourceLoader._GetResource_( "/Resources/m3u8_32x36.ico" ) ),
            //    };
            //    wnd.KeyDown += (s, e) => { if ( e.Key == Key.Escape ) wnd.Close(); };
            //    var topWnd = Extensions.GetTopWindow();
            //    if ( topWnd != null )
            //    {
            //        await wnd.ShowDialog( topWnd );
            //    }
            //    else
            //    {
            //        wnd.Show();
            //    }
            //}
            #endregion
        }
    }
}
