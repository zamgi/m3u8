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
    public abstract class MenuItemBase< T > : MenuItem, IStyleable
    {
        /// <summary>
        /// 
        /// </summary>
        protected sealed class SubMenuItem : MenuItem, IStyleable
        {
            private Image _InnerImage;
            public SubMenuItem( T value, EventHandler< RoutedEventArgs > onClick ) : this( value, value.ToString(), onClick ){}
            public SubMenuItem( T value, string text, EventHandler< RoutedEventArgs > onClick )
            {
                this.Click += onClick;
                Value = value;

                this.Icon = _InnerImage = new Image();
                this.Header = new TextBlock() { Text = text, TextTrimming = TextTrimming.CharacterEllipsis };
                this.Text   = text;
            }

            Type IStyleable.StyleKey => typeof(MenuItem);
            public string Text { get; }

            public T Value { get; }
            public IImage Image
            {
                get => _InnerImage.Source;
                set => _InnerImage.Source = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public delegate void ValueChangedEventHandler( T value );

        public event ValueChangedEventHandler ValueChanged;

        Type IStyleable.StyleKey => typeof(MenuItem);

        protected abstract string  MainToolTipText    { get; }
        protected abstract IBitmap MainImage          { get; }
        protected abstract IBrush  MainForeground     { get; }
        protected abstract IBrush  SelectedBackground { get; }

        protected abstract void FillDropDownItems();
        protected abstract T DefaultValue { get; }
        protected abstract bool IsEqual( T x, T y );

        protected TextBlock _InnerTextBlock;
        private Image       _InnerImage;
        public IImage Image
        {
            get => _InnerImage.Source;
            set => _InnerImage.Source = value;
        }
        protected MenuItemBase() { }
        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.FontWeight = FontWeight.Bold;
            this.Foreground = MainForeground;
            var sp = new StackPanel();
            _InnerTextBlock = new TextBlock() { Text = "-", HorizontalAlignment = HorizontalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis };
            _InnerTextBlock.SetValue( ToolTip.TipProperty, MainToolTipText );
            sp.Children.Add( _InnerTextBlock );
            _InnerImage = new Image() { Source = MainImage };
            sp.Children.Add( _InnerImage );
            this.Header = sp;

            _Value = DefaultValue;

            FillDropDownItems();
        }

        protected T _Value;
        public virtual T Value
        {
            get => _Value;
            set
            {
                if ( !IsEqual( _Value, value ) )
                {
                    _Value = value;

                    _InnerTextBlock.Text = $"   {value}   ";
                    _InnerTextBlock.SetValue( ToolTip.TipProperty, MainToolTipText + ": " + value );

                    foreach ( SubMenuItem mi in this.Items )
                    {
                        if ( IsEqual( mi.Value, value ) )
                        {
                            mi.Background = this.SelectedBackground;
                            mi.Foreground = this.Foreground;
                            mi.Image      = this.Image; //this.MainImage;
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
        protected void Fire_ValueChanged() => this.ValueChanged?.Invoke( this.Value );
    }
    //------------------------------------------------------------------------------------------//
    /// <summary>
    /// 
    /// </summary>
    public abstract class MenuItemBase__IntValue : MenuItemBase< int >
    {
        protected override int DefaultValue => -1;
        protected override bool IsEqual( int x, int y ) => (x == y);
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class DownloadInstanceMenuItem : MenuItemBase__IntValue
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
    public sealed class DegreeOfParallelismMenuItem : MenuItemBase__IntValue
    {
        public DegreeOfParallelismMenuItem() { }

        protected override string  MainToolTipText    => "degree of parallelism";
        protected override IBitmap MainImage          => new Bitmap( ResourceLoader._GetResource_( "/Resources/dop.ico" ) );
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
    //------------------------------------------------------------------------------------------//

    /// <summary>
    /// 
    /// </summary>
    public abstract class MenuItemBase__DoubleValue : MenuItemBase< double? >
    {
        protected override double? DefaultValue => -1;
        protected override bool IsEqual( double? x, double? y ) //=> (Math.Abs( x - y ) <= double.Epsilon);
        {
            if ( x.HasValue )
            {
                if ( y.HasValue )
                {
                    return (Math.Abs( x.Value - y.Value ) <= double.Epsilon);
                }
            }
            else if ( !y.HasValue )
            {
                return (true);
            }
            return (false);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class SpeedThresholdToolButton : MenuItemBase__DoubleValue
    {
        private const string MAX_SPEED = "Max (unlim)";
        private const string MBPS      = "Mbps";

        public SpeedThresholdToolButton() { }

        protected override string  MainToolTipText    => "speed limit";
        protected override IBitmap MainImage          => new Bitmap( ResourceLoader._GetResource_( "/Resources/speed/speed_main_1.png" ) );
        protected override IBrush  MainForeground     => Brushes.CadetBlue;
        protected override IBrush  SelectedBackground => Brushes.LightGreen;

        public override double? Value 
        { 
            get => _Value;
            set
            {
                if ( !IsEqual( _Value, value ) )
                {
                    _Value = value;

                    var t = (value.HasValue ? $"{value.Value} {MBPS}" : MAX_SPEED);
                    _InnerTextBlock.Text = t;
                    _InnerTextBlock.SetValue( ToolTip.TipProperty, MainToolTipText + ": " + t );
                    this.Image = MainImage;

                    foreach ( SubMenuItem mi in this.Items )
                    {
                        if ( IsEqual( mi.Value, value ) )
                        {
                            mi.Background = this.SelectedBackground;
                            mi.FontWeight = this.FontWeight;

                            this.Image           = mi.Image;
                            this.Foreground      = mi.Foreground;
                            _InnerTextBlock.Text = mi.Text;
                            _InnerTextBlock.SetValue( ToolTip.TipProperty, MainToolTipText + ": " + mi.Text );
                        }
                        else
                        {
                            mi.Background = this.Background;
                            mi.FontWeight = FontWeight.Regular;
                        }
                    }

                    Fire_ValueChanged();
                }
            }
        }

        protected override void FillDropDownItems()
        {
            var subMenuItems = new[]
            {
                new SubMenuItem( null, MAX_SPEED   , SubMenuItem_Click ) { FontWeight = FontWeight.Regular, Foreground = new SolidColorBrush( Color.FromRgb(  81, 189, 255 ) ), Image = new Bitmap( ResourceLoader._GetResource_( "/Resources/speed/speed_main_1.png" ) ) },
                new SubMenuItem(   40, $"40 {MBPS}", SubMenuItem_Click ) { FontWeight = FontWeight.Regular, Foreground = new SolidColorBrush( Color.FromRgb( 190, 144, 255 ) ), Image = new Bitmap( ResourceLoader._GetResource_( "/Resources/speed/speed_1.ico" ) ) },
                new SubMenuItem(   20, $"20 {MBPS}", SubMenuItem_Click ) { FontWeight = FontWeight.Regular, Foreground = new SolidColorBrush( Color.FromRgb( 164, 110, 255 ) ), Image = new Bitmap( ResourceLoader._GetResource_( "/Resources/speed/speed_2.ico" ) ) },
                new SubMenuItem(   10, $"10 {MBPS}", SubMenuItem_Click ) { FontWeight = FontWeight.Regular, Foreground = new SolidColorBrush( Color.FromRgb( 234, 118,  33 ) ), Image = new Bitmap( ResourceLoader._GetResource_( "/Resources/speed/speed_3.ico" ) ) },
                new SubMenuItem(    5, $"5 {MBPS}" , SubMenuItem_Click ) { FontWeight = FontWeight.Regular, Foreground = new SolidColorBrush( Color.FromRgb( 252, 146,   0 ) ), Image = new Bitmap( ResourceLoader._GetResource_( "/Resources/speed/speed_4.ico" ) ) },
                new SubMenuItem(    1, $"1 {MBPS}" , SubMenuItem_Click ) { FontWeight = FontWeight.Regular, Foreground = new SolidColorBrush( Color.FromRgb( 178, 202,   0 ) ), Image = new Bitmap( ResourceLoader._GetResource_( "/Resources/speed/speed_5.ico" ) ) },
            };
            this.Items = subMenuItems;

            this.Value = null;
        }
    }
}
