using System.Drawing;
using System.Linq;

using _Resources_ = m3u8.download.manager.Properties.Resources;
using _ToolStripSpeedThreshold_ = m3u8.download.manager.ui.ToolStripSpeedThreshold;

namespace System.Windows.Forms
{
    /// <summary>
    /// 
    /// </summary>
    internal abstract class ToolStripDropDownButtonEx< T > : ToolStripDropDownButton
    {
        /// <summary>
        /// 
        /// </summary>
        protected sealed class ToolStripMenuItemEx : ToolStripMenuItem
        {
            public ToolStripMenuItemEx( T value, EventHandler onClick ) : base( value.ToString(), null, onClick ) => Value = value;
            public T Value { get; }
        }

        /// <summary>
        /// 
        /// </summary>
        public delegate void ValueChangedEventHandler( T value );

        public event ValueChangedEventHandler ValueChanged;

        protected abstract string MainToolTipText   { get; }
        protected abstract Image  MainImage         { get; }
        protected abstract Color  MainForeColor     { get; }
        protected abstract Color  SelectedBackColor { get; }

        protected abstract void FillDropDownItems();
        protected abstract T DefaultValue { get; }
        protected abstract bool IsEqual( T x, T y );

        protected ToolStripDropDownButtonEx()
        {
            this.Font                  = new Font( "Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, ((byte) (204)) );
            this.ForeColor             = MainForeColor;
            this.ImageTransparentColor = Color.Magenta;
            this.ShowDropDownArrow     = false;
            //this.TextImageRelation     = TextImageRelation.ImageBeforeText;
            this.TextImageRelation     = TextImageRelation.TextAboveImage;
            this.ImageScaling          = ToolStripItemImageScaling.None;
            this.Image                 = MainImage;
            this.ToolTipText           = MainToolTipText;
            this.Text                  = "-";

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

                    this.Text = $"   {value}   ";
                    this.ToolTipText = MainToolTipText + ": " + value;

                    var font = new Font( this.Font, FontStyle.Regular );
                    foreach ( ToolStripMenuItemEx mi in this.DropDownItems )
                    {
                        if ( IsEqual( mi.Value, value ) )
                        {
                            mi.BackColor = this.SelectedBackColor;
                            mi.ForeColor = this.ForeColor;
                            mi.Font      = this.Font;
                            mi.Image     = this.Image; //_Resources_.checked_blue_16.ToBitmap(); //
                        }
                        else
                        {
                            mi.BackColor = this.BackColor;
                            mi.ForeColor = Color.Black;
                            mi.Font      = font;
                            mi.Image     = null;
                        }                            
                    }

                    this.ValueChanged?.Invoke( this.Value );
                }
            }
        }

        protected void ToolStripMenuItemEx_EventHandler( object sender, EventArgs e ) => this.Value = ((ToolStripMenuItemEx) sender).Value;
        protected void Fire_ValueChanged() => this.ValueChanged?.Invoke( this.Value );
    }

    //------------------------------------------------------------------------------------------//
    /// <summary>
    /// 
    /// </summary>
    internal abstract class ToolStripDropDownButtonEx__IntValue : ToolStripDropDownButtonEx< int >
    {
        protected override int DefaultValue => -1;
        protected override bool IsEqual( int x, int y ) => (x == y);
    }

    /// <summary>
    /// 
    /// </summary>
    internal sealed class DownloadInstanceToolButton : ToolStripDropDownButtonEx__IntValue
    {
        public DownloadInstanceToolButton() { }

        protected override string MainToolTipText   => "downloads instance count";
        protected override Image  MainImage         => _Resources_.downloadInstance.ToBitmap();
        protected override Color  MainForeColor     => Color.DodgerBlue;
        protected override Color  SelectedBackColor => Color.LightBlue;

        protected override void FillDropDownItems()
        {
            var font = new Font( this.Font, FontStyle.Regular );
            for ( var i = 1; i <= 10; i++ )
            {
                this.DropDownItems.Add( new ToolStripMenuItemEx( i, ToolStripMenuItemEx_EventHandler ) { Font = font, ImageScaling = ToolStripItemImageScaling.None } ); //, DisplayStyle = ToolStripItemDisplayStyle.Text } );
            }
        }

        public void SetValueAndVisible( int? maxCrossDownloadInstance )
        {
            this.Visible = maxCrossDownloadInstance.HasValue;
            if ( maxCrossDownloadInstance.HasValue ) this.Value = maxCrossDownloadInstance.Value;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    internal sealed class DegreeOfParallelismToolButton : ToolStripDropDownButtonEx__IntValue
    {
        public DegreeOfParallelismToolButton() { }

        protected override string MainToolTipText   => "degree of parallelism";
        protected override Image  MainImage         => _Resources_.dop_16.ToBitmap();
        protected override Color  MainForeColor     => Color.Green;
        protected override Color  SelectedBackColor => Color.LightGreen;

        protected override void FillDropDownItems()
        {
            var font = new Font( this.Font, FontStyle.Regular );
            this.DropDownItems.Add( new ToolStripMenuItemEx(  1, ToolStripMenuItemEx_EventHandler ) { Font = font, ImageScaling = ToolStripItemImageScaling.None } );
            this.DropDownItems.Add( new ToolStripMenuItemEx(  2, ToolStripMenuItemEx_EventHandler ) { Font = font, ImageScaling = ToolStripItemImageScaling.None } );
            this.DropDownItems.Add( new ToolStripMenuItemEx(  4, ToolStripMenuItemEx_EventHandler ) { Font = font, ImageScaling = ToolStripItemImageScaling.None } );
            this.DropDownItems.Add( new ToolStripMenuItemEx(  8, ToolStripMenuItemEx_EventHandler ) { Font = font, ImageScaling = ToolStripItemImageScaling.None } );
            this.DropDownItems.Add( new ToolStripMenuItemEx( 12, ToolStripMenuItemEx_EventHandler ) { Font = font, ImageScaling = ToolStripItemImageScaling.None } );
            this.DropDownItems.Add( new ToolStripMenuItemEx( 16, ToolStripMenuItemEx_EventHandler ) { Font = font, ImageScaling = ToolStripItemImageScaling.None } );
            this.DropDownItems.Add( new ToolStripMenuItemEx( 24, ToolStripMenuItemEx_EventHandler ) { Font = font, ImageScaling = ToolStripItemImageScaling.None } );
            this.DropDownItems.Add( new ToolStripMenuItemEx( 32, ToolStripMenuItemEx_EventHandler ) { Font = font, ImageScaling = ToolStripItemImageScaling.None } );
            this.DropDownItems.Add( new ToolStripMenuItemEx( 64, ToolStripMenuItemEx_EventHandler ) { Font = font, ImageScaling = ToolStripItemImageScaling.None } );

            //for ( int i = 0; i <= 7; i++ )
            //{
            //    this.DropDownItems.Add( new ToolStripMenuItemEx( (1 << 1), ToolStripMenuItemEx_EventHandler ) { Font = font, ImageScaling = ToolStripItemImageScaling.None } );
            //}
        }
    }
    //------------------------------------------------------------------------------------------//

    /// <summary>
    /// 
    /// </summary>
    internal abstract class ToolStripDropDownButtonEx__DoubleValue : ToolStripDropDownButtonEx< double? >
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
    internal sealed class SpeedThresholdToolButton : ToolStripDropDownButtonEx__DoubleValue
    {
        private const string MAX_SPEED = "Max (unlim)";
        private const string MBPS      = "Mbps";

        public SpeedThresholdToolButton() { }

        protected override string MainToolTipText   => "speed limit";
        protected override Image  MainImage         => _Resources_.speed_main_1;
        protected override Color  MainForeColor     => Color.CadetBlue;
        protected override Color  SelectedBackColor => Color.LightGreen;

        public override double? Value 
        { 
            get => _Value;
            set
            {
                if ( !IsEqual( _Value, value ) )
                {
                    _Value = value;

                    var t = (value.HasValue ? $"{value.Value} {MBPS}" : MAX_SPEED);
                    this.Text        = t;
                    this.ToolTipText = MainToolTipText + ": " + t;
                    this.Image       = MainImage;

                    var font = new Font( this.Font, FontStyle.Regular );
                    var was_find_mi = false;
                    foreach ( var mi in this.DropDownItems.Cast< ToolStripItem >().OfType< ToolStripMenuItemEx >() )
                    {
                        if ( IsEqual( mi.Value, value ) )
                        {
                            mi.BackColor = this.SelectedBackColor;
                            mi.Font      = this.Font;

                            this.Image       = mi.Image;
                            this.ForeColor   = mi.ForeColor;
                            this.Text        = mi.Text;
                            this.ToolTipText = MainToolTipText + ": " + mi.Text;
                            was_find_mi = true;
                        }
                        else
                        {
                            mi.BackColor = this.BackColor;
                            mi.Font      = font;
                        }
                    }

                    if ( !was_find_mi && _Value.HasValue )
                    {
                        _ToolStripSpeedThreshold.BackColor = this.SelectedBackColor;
                        //_ToolStripSpeedThreshold.Font = this.Font;

                        var val_int = (int) _Value.Value;
                        if ( _ToolStripSpeedThreshold.Value != val_int )
                        {
                            _ToolStripSpeedThreshold.Value = val_int;
                        }
                    }
                    else
                    {
                        _ToolStripSpeedThreshold.BackColor = this.BackColor;
                        //_ToolStripSpeedThreshold.Font = font;
                    }

                    Fire_ValueChanged();
                }
            }
        }
        public (double? value, double valueSaved) ValueWithSaved
        {
            get => (_Value, _Value.GetValueOrDefault( _ToolStripSpeedThreshold.Value ));
            set
            {
                var (v, saved) = value;
                this.Value = v;
                if ( !v.HasValue || (v != saved) )
                {
                    var val_int = (int) saved;
                    if ( _ToolStripSpeedThreshold.Value != val_int )
                    {
                        _ToolStripSpeedThreshold.SpeedThreshold_ValueChanged -= ToolStripSpeedThreshold_ValueChanged;
                        _ToolStripSpeedThreshold.Value = val_int;
                        _ToolStripSpeedThreshold.SpeedThreshold_ValueChanged += ToolStripSpeedThreshold_ValueChanged;
                    }
                }
            }
        }
        public double ValueSaved => _ToolStripSpeedThreshold.Value;

        private _ToolStripSpeedThreshold_ _ToolStripSpeedThreshold;
        private void ToolStripSpeedThreshold_ValueChanged( object sender, EventArgs e ) => this.Value = ((_ToolStripSpeedThreshold_) sender).Value;
        protected override void FillDropDownItems()
        {            
            var font = new Font( this.Font, FontStyle.Regular );

            this.DropDownItems.Add( new ToolStripMenuItemEx( null, ToolStripMenuItemEx_EventHandler ) { Text = MAX_SPEED   , ForeColor = Color.FromArgb(  81, 189, 255 ), Image = _Resources_.speed_main_1      , Font = font, ImageScaling = ToolStripItemImageScaling.None } );
            this.DropDownItems.Add( new ToolStripMenuItemEx(   60, ToolStripMenuItemEx_EventHandler ) { Text = $"60 {MBPS}", ForeColor = Color.FromArgb(  20,   0,  47 ), Image = _Resources_.speed_1.ToBitmap(), Font = font, ImageScaling = ToolStripItemImageScaling.None } );
            this.DropDownItems.Add( new ToolStripMenuItemEx(   50, ToolStripMenuItemEx_EventHandler ) { Text = $"50 {MBPS}", ForeColor = Color.FromArgb(  75,   0, 179 ), Image = _Resources_.speed_1.ToBitmap(), Font = font, ImageScaling = ToolStripItemImageScaling.None } );
            this.DropDownItems.Add( new ToolStripMenuItemEx(   40, ToolStripMenuItemEx_EventHandler ) { Text = $"40 {MBPS}", ForeColor = Color.FromArgb( 164, 110, 255 ), Image = _Resources_.speed_1.ToBitmap(), Font = font, ImageScaling = ToolStripItemImageScaling.None } );
            this.DropDownItems.Add( new ToolStripMenuItemEx(   20, ToolStripMenuItemEx_EventHandler ) { Text = $"20 {MBPS}", ForeColor = Color.FromArgb( 190, 144, 255 ), Image = _Resources_.speed_2.ToBitmap(), Font = font, ImageScaling = ToolStripItemImageScaling.None } );
            this.DropDownItems.Add( new ToolStripMenuItemEx(   10, ToolStripMenuItemEx_EventHandler ) { Text = $"10 {MBPS}", ForeColor = Color.FromArgb( 234, 118,  33 ), Image = _Resources_.speed_3.ToBitmap(), Font = font, ImageScaling = ToolStripItemImageScaling.None } );
            this.DropDownItems.Add( new ToolStripMenuItemEx(    5, ToolStripMenuItemEx_EventHandler ) { Text = $"5 {MBPS}" , ForeColor = Color.FromArgb( 252, 146,   0 ), Image = _Resources_.speed_4.ToBitmap(), Font = font, ImageScaling = ToolStripItemImageScaling.None } );
            this.DropDownItems.Add( new ToolStripMenuItemEx(    1, ToolStripMenuItemEx_EventHandler ) { Text = $"1 {MBPS}" , ForeColor = Color.FromArgb( 178, 202,   0 ), Image = _Resources_.speed_5.ToBitmap(), Font = font, ImageScaling = ToolStripItemImageScaling.None } );
            
            this.DropDownItems.Add( new ToolStripSeparator() );
            this.DropDownItems.Add( _ToolStripSpeedThreshold = new _ToolStripSpeedThreshold_( ToolStripSpeedThreshold_ValueChanged ) { Font = font } );

            this.Value = null;
        }
    }
}
