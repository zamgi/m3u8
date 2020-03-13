using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class ParallelismForm : Window
    {
        #region [.ctor().]
        public ParallelismForm()
        {
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load( this );
        }

        #endregion

    }
}
