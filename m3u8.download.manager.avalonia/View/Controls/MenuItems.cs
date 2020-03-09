using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class MenuItemBase : MenuItem, IStyleable
    {
        /// <summary>
        /// 
        /// </summary>
        protected sealed class SubMenuItem : MenuItem, IStyleable
        {
            private Image _InnerImage;
            public SubMenuItem( int value, EventHandler< RoutedEventArgs > onClick )
            {
                this.Click += onClick;
                Value = value;

                this.Icon   = _InnerImage = new Image();
                this.Header = new TextBlock() { Text = value.ToString() };
            }

            Type IStyleable.StyleKey => typeof(MenuItem);

            public int Value { get; }
            public IBitmap Image
            {
                get => _InnerImage.Source;
                set => _InnerImage.Source = value;
            }
        }

        public delegate void ValueChangedEventHandler( int value );
        public event ValueChangedEventHandler ValueChanged;

        Type IStyleable.StyleKey => typeof(MenuItem);

        protected abstract string  MainToolTipText    { get; }
        protected abstract IBitmap MainImage          { get; }
        protected abstract IBrush  MainForeground     { get; }
        protected abstract IBrush  SelectedBackground { get; }

        protected abstract void FillDropDownItems();

        private TextBlock _InnerTextBlock;
        private Image     _InnerImage;
        protected MenuItemBase() { }
        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.FontWeight = FontWeight.Bold;
            this.Foreground = MainForeground;
            var sp = new StackPanel();
            _InnerTextBlock = new TextBlock() { Text = "-", HorizontalAlignment = HorizontalAlignment.Center };
            _InnerTextBlock.SetValue( ToolTip.TipProperty, MainToolTipText );
            sp.Children.Add( _InnerTextBlock );
            _InnerImage = new Image() { Source = MainImage };
            sp.Children.Add( _InnerImage );
            this.Header = sp;

            FillDropDownItems();
        }

        private int _Value = -1;
        public int Value
        {
            get => _Value;
            set
            {
                if ( _Value != value )
                {
                    _Value = value;

                    _InnerTextBlock.Text = $"   {value}   ";
                    _InnerTextBlock.SetValue( ToolTip.TipProperty, MainToolTipText + ": " + value );

                    foreach ( SubMenuItem mi in this.Items )
                    {
                        if ( mi.Value == value )
                        {
                            mi.Background = this.SelectedBackground;
                            mi.Foreground = this.Foreground;
                            mi.Image      = _InnerImage.Source; //this.MainImage;
                            mi.FontWeight = this.FontWeight;
                        }
                        else
                        {
                            mi.Background = this.Background;
                            mi.Foreground = Brushes.Black;
                            mi.Image      = null;
                            mi.FontWeight = FontWeight.Regular;
                        }
                    }

                    this.ValueChanged?.Invoke( this.Value );
                }
            }
        }

        protected void SubMenuItem_Click( object sender, RoutedEventArgs e ) => this.Value = ((SubMenuItem) sender).Value;
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class DownloadInstanceMenuItem : MenuItemBase
    {
        public DownloadInstanceMenuItem() { }

        protected override IBitmap MainImage          => new Bitmap( ResourceLoader._GetResource_( "/Resources/downloadInstance.ico" ) );
        protected override string  MainToolTipText    => "downloads instance count";
        protected override IBrush  MainForeground     => Brushes.DodgerBlue;
        protected override IBrush  SelectedBackground => Brushes.LightBlue;

        protected override void FillDropDownItems()
        {
            const int ITEMS_COUNT = 10;
            var subMenuItems = new SubMenuItem[ ITEMS_COUNT ];
            for ( var i = 0; i < ITEMS_COUNT; i++ )
            {
                subMenuItems[ i ] = new SubMenuItem( i + 1, SubMenuItem_Click ) { FontWeight = FontWeight.Regular };
            }
            this.Items = subMenuItems;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class DegreeOfParallelismMenuItem : MenuItemBase
    {
        public DegreeOfParallelismMenuItem() { }

        protected override string  MainToolTipText    => "degree of parallelism";
        protected override IBitmap MainImage          => new Bitmap( ResourceLoader._GetResource_( "/Resources/dop_16.ico" ) );
        protected override IBrush  MainForeground     => Brushes.Green;
        protected override IBrush  SelectedBackground => Brushes.LightGreen;

        protected override void FillDropDownItems()
        {
            var subMenuItems = new[]
            {
                new SubMenuItem(  1, SubMenuItem_Click ) { FontWeight = FontWeight.Regular },
                new SubMenuItem(  2, SubMenuItem_Click ) { FontWeight = FontWeight.Regular },
                new SubMenuItem(  4, SubMenuItem_Click ) { FontWeight = FontWeight.Regular },
                new SubMenuItem(  8, SubMenuItem_Click ) { FontWeight = FontWeight.Regular },
                new SubMenuItem( 12, SubMenuItem_Click ) { FontWeight = FontWeight.Regular },
                new SubMenuItem( 16, SubMenuItem_Click ) { FontWeight = FontWeight.Regular },
                new SubMenuItem( 32, SubMenuItem_Click ) { FontWeight = FontWeight.Regular },
                new SubMenuItem( 64, SubMenuItem_Click ) { FontWeight = FontWeight.Regular },
            };
            this.Items = subMenuItems;
        }
    }
}
