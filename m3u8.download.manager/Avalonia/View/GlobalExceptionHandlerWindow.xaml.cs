using System;
using System.Threading;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class GlobalExceptionHandlerWindow : Window
    {
        private CancellationTokenSource _Cts;
        public GlobalExceptionHandlerWindow()
        {
            _Cts = new CancellationTokenSource();

            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }
        public GlobalExceptionHandlerWindow( Exception ex ) : this() => this.DataContext = new { Exception = ex };

        public static void Show( Exception ex ) => (new GlobalExceptionHandlerWindow( ex )).Show();

        private void InitializeComponent() => AvaloniaXamlLoader.Load( this );

        protected override void OnOpened( EventArgs e )
        {
            base.OnOpened( e );

            Avalonia.Threading.Dispatcher.UIThread.MainLoop( _Cts.Token );
        }
        protected override void OnClosed( EventArgs e )
        {
            base.OnClosed( e );

            _Cts.Cancel();
        }
    }
}
