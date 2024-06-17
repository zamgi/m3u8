using System;
using System.Collections.Generic;
using System.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class _MenuItemEx_Base< T > : MenuItem//, IStyleable
    {
        /// <summary>
        /// 
        /// </summary>
        protected sealed class SubMenuItem : MenuItem//, IStyleable
        {
            private Image _InnerImage;
            public SubMenuItem( T value, EventHandler< RoutedEventArgs > onClick ) : this( value, value.ToString(), onClick ) { }
            public SubMenuItem( T value, string text, EventHandler< RoutedEventArgs > onClick )
            {
                this.Click += onClick;
                Value = value;

                this.Icon = _InnerImage = new Image();
                this.Header = new TextBlock() { Text = text, TextTrimming = TextTrimming.CharacterEllipsis };
                this.Text   = text;
            }

            //Type IStyleable.StyleKey => typeof(MenuItem);
            protected override Type StyleKeyOverride => typeof(MenuItem);
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

        //Type IStyleable.StyleKey => typeof(MenuItem);
        protected override Type StyleKeyOverride => typeof(MenuItem);

        protected abstract string  MainToolTipText    { get; }
        protected abstract Bitmap  MainImage          { get; }
        protected abstract IBrush  MainForeground     { get; }
        protected abstract IBrush  SelectedBackground { get; }
        protected virtual  string  Suffix => null;

        protected abstract void FillDropDownItems();
        protected abstract T DefaultValue { get; }
        protected abstract bool IsEqual( T x, T y );

        protected TextBlock _InnerTextBlock;
        private   Image     _InnerImage;
        public IImage Image
        {
            get => _InnerImage.Source;
            set => _InnerImage.Source = value;
        }
        protected _MenuItemEx_Base() { }        
        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.FontWeight = FontWeight.Bold;
            this.Foreground = MainForeground;
            var sp = new StackPanel();
            _InnerTextBlock = new TextBlock() { Text = "-", HorizontalAlignment = HorizontalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis, Margin = new Thickness( 10, 0 ) };
            SetTip4TopItem( MainToolTipText );
            sp.Children.Add( _InnerTextBlock );
            _InnerImage = new Image() { Source = MainImage };
            sp.Children.Add( _InnerImage );
            this.Header = sp;

            _Value = DefaultValue;

            FillDropDownItems();
        }
        protected void SetTip4TopItem( string txt )
        {
            ToolTip.SetTip( this, txt );
            //this.SetValue( ToolTip.FontWeightProperty, FontWeight.Regular );
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

                    var txt = (value?.ToString() ?? string.Empty) + Suffix;// $"   {value}   ";
                    _InnerTextBlock.Text   = txt;                    
                    _InnerTextBlock.Margin = new Thickness( (txt.Length <= 1) ? 14 : 10, 0 );
                    SetTip4TopItem( MainToolTipText );

                    foreach ( SubMenuItem mi in this.Items.OfType< SubMenuItem >() )
                    {
                        if ( IsEqual( mi.Value, value ) )
                        {
                            mi.Background = this.SelectedBackground;
                            mi.Foreground = this.Foreground;
                            mi.Image      = this.Image; //this.MainImage;
                            mi.FontWeight = this.FontWeight;
                            SetTip4TopItem( $"{MainToolTipText}: {mi.Text}" );
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
    public abstract class _MenuItemEx__IntValue : _MenuItemEx_Base< int >
    {
        protected override int DefaultValue => -1;
        protected override bool IsEqual( int x, int y ) => (x == y);
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class DownloadInstanceMenuItem : _MenuItemEx__IntValue
    {
        public DownloadInstanceMenuItem() { }

        protected override Bitmap MainImage          => new Bitmap( ResourceLoader._GetResource_( "/Resources/downloadInstance.ico" ) );
        protected override string MainToolTipText    => "downloads instance count";
        protected override IBrush MainForeground     => Brushes.DodgerBlue;
        protected override IBrush SelectedBackground => Brushes.LightBlue;
        protected override string Suffix             => "  (inst)";

        protected override void FillDropDownItems()
        {
            var subMenuItems = new List< SubMenuItem >( 10 );
            for ( var i = 1; i <= 10; i++ )
            {
                subMenuItems.Add( new SubMenuItem( i, $"{i}{Suffix}", SubMenuItem_Click ) { FontWeight = FontWeight.Regular } );
            }
            this.ItemsSource = subMenuItems;
        }


        public void SetValueAndIsVisible( int? maxCrossDownloadInstance )
        {
            this.IsVisible = maxCrossDownloadInstance.HasValue;
            if ( maxCrossDownloadInstance.HasValue ) this.Value = maxCrossDownloadInstance.Value;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class DegreeOfParallelismMenuItem : _MenuItemEx__IntValue
    {
        public DegreeOfParallelismMenuItem() { }

        protected override string MainToolTipText    => "degree of parallelism";
        protected override Bitmap MainImage          => new Bitmap( ResourceLoader._GetResource_( "/Resources/dop.ico" ) );
        protected override IBrush MainForeground     => Brushes.Green;
        protected override IBrush SelectedBackground => Brushes.LightGreen;
        protected override string Suffix             => "  (dop)";

        protected override void FillDropDownItems()
        {
            var subMenuItems = new List< SubMenuItem >( 10 );
            foreach ( var i in new[] { 1, 2, 4, 8, 12, 16, 24, 32, 64 } )
            {
                subMenuItems.Add( new SubMenuItem( i, $"{i}{Suffix}", SubMenuItem_Click ) { FontWeight = FontWeight.Regular } );
            }
            this.ItemsSource = subMenuItems;
        }
    }
    //------------------------------------------------------------------------------------------//

    /// <summary>
    /// 
    /// </summary>
    public abstract class _MenuItemEx__DecimalValue : _MenuItemEx_Base< decimal? >
    {
        protected override decimal? DefaultValue => -1;
        protected override bool IsEqual( decimal? x, decimal? y )
        {
            if ( x.HasValue )
            {
                if ( y.HasValue )
                {
                    return (x.Value == y.Value);
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
    public sealed class SpeedThresholdToolButton : _MenuItemEx__DecimalValue
    {
        private const string MAX_SPEED = "Max (unlim)";
        private const string MBPS      = "Mbps";

        public SpeedThresholdToolButton() { }

        protected override string  MainToolTipText    => "speed limit";
        protected override Bitmap  MainImage          => new Bitmap( ResourceLoader._GetResource_( "/Resources/speed/speed_main_1.png" ) );
        protected override IBrush  MainForeground     => Brushes.CadetBlue;
        protected override IBrush  SelectedBackground => Brushes.LightGreen;

        public override decimal? Value 
        { 
            get => _Value;
            set
            {
                if ( !IsEqual( _Value, value ) )
                {
                    _Value = value;

                    var txt = (value.HasValue ? $"{value.Value} {MBPS}" : MAX_SPEED);
                    _InnerTextBlock.Text = txt;
                    SetTip4TopItem( $"{MainToolTipText}: {txt}" );
                    this.Image = MainImage;

                    foreach ( SubMenuItem mi in this.Items.OfType< SubMenuItem >() )
                    {
                        if ( IsEqual( mi.Value, value ) )
                        {
                            mi.Background = this.SelectedBackground;
                            mi.FontWeight = this.FontWeight;

                            this.Image           = mi.Image;
                            this.Foreground      = mi.Foreground;
                            _InnerTextBlock.Text = mi.Text;
                            SetTip4TopItem( $"{MainToolTipText}: {mi.Text}" );
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
            var subMenuItems = new object[]
            {
                new SubMenuItem( null, MAX_SPEED   , SubMenuItem_Click ) { FontWeight = FontWeight.Regular, Foreground = new SolidColorBrush( Color.FromRgb(  81, 189, 255 ) ), Image = new Bitmap( ResourceLoader._GetResource_( "/Resources/speed/speed_main_1.png" ) ) },
                new SubMenuItem(   60, $"60 {MBPS}", SubMenuItem_Click ) { FontWeight = FontWeight.Regular, Foreground = new SolidColorBrush( Color.FromRgb(  20,   0,  47 ) ), Image = new Bitmap( ResourceLoader._GetResource_( "/Resources/speed/speed_1.ico" ) ) },
                new SubMenuItem(   50, $"50 {MBPS}", SubMenuItem_Click ) { FontWeight = FontWeight.Regular, Foreground = new SolidColorBrush( Color.FromRgb(  75,   0, 179 ) ), Image = new Bitmap( ResourceLoader._GetResource_( "/Resources/speed/speed_1.ico" ) ) },
                new SubMenuItem(   40, $"40 {MBPS}", SubMenuItem_Click ) { FontWeight = FontWeight.Regular, Foreground = new SolidColorBrush( Color.FromRgb( 164, 110, 255 ) ), Image = new Bitmap( ResourceLoader._GetResource_( "/Resources/speed/speed_1.ico" ) ) },
                new SubMenuItem(   20, $"20 {MBPS}", SubMenuItem_Click ) { FontWeight = FontWeight.Regular, Foreground = new SolidColorBrush( Color.FromRgb( 190, 144, 255 ) ), Image = new Bitmap( ResourceLoader._GetResource_( "/Resources/speed/speed_2.ico" ) ) },
                new SubMenuItem(   10, $"10 {MBPS}", SubMenuItem_Click ) { FontWeight = FontWeight.Regular, Foreground = new SolidColorBrush( Color.FromRgb( 234, 118,  33 ) ), Image = new Bitmap( ResourceLoader._GetResource_( "/Resources/speed/speed_3.ico" ) ) },
                new SubMenuItem(    5, $"5 {MBPS}" , SubMenuItem_Click ) { FontWeight = FontWeight.Regular, Foreground = new SolidColorBrush( Color.FromRgb( 252, 146,   0 ) ), Image = new Bitmap( ResourceLoader._GetResource_( "/Resources/speed/speed_4.ico" ) ) },
                new SubMenuItem(    1, $"1 {MBPS}" , SubMenuItem_Click ) { FontWeight = FontWeight.Regular, Foreground = new SolidColorBrush( Color.FromRgb( 178, 202,   0 ) ), Image = new Bitmap( ResourceLoader._GetResource_( "/Resources/speed/speed_5.ico" ) ) },

                new Separator(),
                new SubMenuItem( 0.7M, $"0.7 {MBPS}" , SubMenuItem_Click ) { FontWeight = FontWeight.Regular, Foreground = new SolidColorBrush( Color.FromRgb( 0, 0, 0 ) ), Image = new Bitmap( ResourceLoader._GetResource_( "/Resources/speed/speed_6.ico" ) ) },
                new SubMenuItem( 0.5M, $"0.5 {MBPS}" , SubMenuItem_Click ) { FontWeight = FontWeight.Regular, Foreground = new SolidColorBrush( Color.FromRgb( 0, 0, 0 ) ), Image = new Bitmap( ResourceLoader._GetResource_( "/Resources/speed/speed_6.ico" ) ) },
                new SubMenuItem( 0.3M, $"0.3 {MBPS}" , SubMenuItem_Click ) { FontWeight = FontWeight.Regular, Foreground = new SolidColorBrush( Color.FromRgb( 0, 0, 0 ) ), Image = new Bitmap( ResourceLoader._GetResource_( "/Resources/speed/speed_6.ico" ) ) },
                new SubMenuItem( 0.1M, $"0.1 {MBPS}" , SubMenuItem_Click ) { FontWeight = FontWeight.Regular, Foreground = new SolidColorBrush( Color.FromRgb( 0, 0, 0 ) ), Image = new Bitmap( ResourceLoader._GetResource_( "/Resources/speed/speed_6.ico" ) ) },
            };
            this.ItemsSource = subMenuItems;

            this.Value = null;
        }
    }
    //------------------------------------------------------------------------------------------//
}
