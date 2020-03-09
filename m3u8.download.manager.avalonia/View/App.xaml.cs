using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class App : Application
    {
        public static (string m3u8FileUrl, bool autoStartDownload)[] _InputParamsArray;

        public override void Initialize() => AvaloniaXamlLoader.Load( this );
        public override void OnFrameworkInitializationCompleted()
        {
            if ( ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop )
            {
                desktop.MainWindow = new MainWindow( _InputParamsArray );
                _InputParamsArray = null;
            }

            base.OnFrameworkInitializationCompleted();
        }        
    }
}
