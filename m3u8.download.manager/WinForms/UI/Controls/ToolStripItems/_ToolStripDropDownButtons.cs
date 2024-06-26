using System.Drawing;
using System.Linq;

using _Resources_                    = m3u8.download.manager.Properties.Resources;
using _ToolStripSpeedThreshold_      = m3u8.download.manager.ui.ToolStripSpeedThreshold;
using _ToolStripDegreeOfParallelism_ = m3u8.download.manager.ui.ToolStripDegreeOfParallelism;
using m3u8.download.manager.ui;

namespace System.Windows.Forms
{
    /// <summary>
    /// 
    /// </summary>
    internal abstract class _ToolStripDropDownButtonEx_Base< T > : ToolStripDropDownButton
    {
        /// <summary>
        /// 
        /// </summary>
        protected sealed class ToolStripMenuItemEx : ToolStripMenuItem
        {
            public ToolStripMenuItemEx( T value, EventHandler onClick ) : base( value.ToString(), null, onClick ) => Value = value;
            public ToolStripMenuItemEx( T value, string suffix, EventHandler onClick ) : base( $"{value}{suffix}", null, onClick ) => Value = value;
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
        protected virtual string  Suffix => null;

        protected abstract void FillDropDownItems();
        protected abstract T DefaultValue { get; }
        protected abstract bool IsEqual( T x, T y );

        protected _ToolStripDropDownButtonEx_Base()
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

                    this.Text        = $"{value}{Suffix}"; //$"   {value}{Suffix}   ";
                    this.ToolTipText = $"{MainToolTipText}: {value}";

                    var font = new Font( this.Font, FontStyle.Regular );
                    foreach ( ToolStripMenuItemEx mi in this.DropDownItems.OfType< ToolStripMenuItemEx >() )
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
    internal abstract class _ToolStripDropDownButtonEx__IntValue : _ToolStripDropDownButtonEx_Base< int >
    {
        protected override int DefaultValue => -1;
        protected override bool IsEqual( int x, int y ) => (x == y);
    }

    /// <summary>
    /// 
    /// </summary>
    internal sealed class DownloadInstanceToolButton : _ToolStripDropDownButtonEx__IntValue
    {
        public DownloadInstanceToolButton() { }

        protected override string MainToolTipText   => "downloads instance count";
        protected override Image  MainImage         => _Resources_.download_inst.ToBitmap();
        protected override Color  MainForeColor     => Color.DodgerBlue;
        protected override Color  SelectedBackColor => Color.LightBlue;
        protected override string Suffix            => "  (inst)";

        protected override void FillDropDownItems()
        {
            var font = new Font( this.Font, FontStyle.Regular );
            for ( var i = 1; i <= 10; i++ )
            {
                this.DropDownItems.Add( new ToolStripMenuItemEx( i, Suffix, ToolStripMenuItemEx_EventHandler ) { Font = font, ImageScaling = ToolStripItemImageScaling.None } ); //, DisplayStyle = ToolStripItemDisplayStyle.Text } );
            }
        }

        public void SetValueAndVisible( int? maxCrossDownloadInstance )
        {
            this.Visible = maxCrossDownloadInstance.HasValue;
            if ( maxCrossDownloadInstance.HasValue ) this.Value = maxCrossDownloadInstance.Value;
        }
    }
    //------------------------------------------------------------------------------------------//

    /// <summary>
    /// 
    /// </summary>
    internal sealed class DegreeOfParallelismToolButton : _ToolStripDropDownButtonEx__IntValue
    {
        private Font _Font;
        public DegreeOfParallelismToolButton() { }
        protected override void Dispose( bool disposing )
        {
            if ( disposing )
            {
                _Font?.Dispose();
            }
            base.Dispose( disposing );
        }

        protected override string MainToolTipText   => "degree of parallelism";
        protected override Image  MainImage         => _Resources_.dop_16.ToBitmap();
        protected override Color  MainForeColor     => Color.Green;
        protected override Color  SelectedBackColor => Color.LightGreen;
        protected override string Suffix            => "  (dop)";

        public override int Value 
        { 
            get => _Value;
            set => SetValue( value );
        }
        
        /// <summary>
        /// 
        /// </summary>
        private enum SetValueState
        {
            NoChanged,
            SettedPredefineValue,
            SettedCustomValue,
        }
        private SetValueState SetValue( int value, bool raiseValueChangedEvent = true )
        {
            var state = SetValueState.NoChanged;

            if ( !IsEqual( _Value, value ) )
            {
                _Value = value;

                var t = $"{value}{Suffix}";
                this.Text        = t;
                this.ToolTipText = $"{MainToolTipText}: {t}";
                this.Image       = MainImage;

                _Font?.Dispose();
                _Font = new Font( this.Font, FontStyle.Regular );
                var was_found_mi = false;
                foreach ( var mi in this.DropDownItems.Cast< ToolStripItem >().OfType< ToolStripMenuItemEx >() )
                {
                    if ( IsEqual( mi.Value, value ) )
                    {
                        mi.BackColor = this.SelectedBackColor;
                        mi.Font      = this.Font;
                        mi.Image     = this.MainImage;
                        mi.ForeColor = this.ForeColor;

                        this.Text        = mi.Text;
                        this.ToolTipText = $"{MainToolTipText}: {mi.Text}";
                        was_found_mi = true;
                    }
                    else
                    {
                        mi.BackColor = this.BackColor;
                        mi.Font      = _Font;
                        mi.Image     = null;
                        mi.ForeColor = Color.Empty;
                    }
                }

                if ( was_found_mi )
                {
                    _ToolStripUC.BackColor = this.BackColor;
                    //_ToolStripUC.Font = _Font;

                    state = SetValueState.SettedPredefineValue;
                }
                else 
                {
                    _ToolStripUC.BackColor = this.SelectedBackColor;
                    //_ToolStripUC.Font = this.Font;

                    if ( _ToolStripUC._Value != _Value )
                    {
                        _ToolStripUC._Value = _Value;                        
                    }
                    state = SetValueState.SettedCustomValue;
                }

                if ( (state != SetValueState.NoChanged) && raiseValueChangedEvent )
                {
                    Fire_ValueChanged();
                }
            }

            return (state);
        }

        public (int value, int valueSaved) ValueWithSaved
        {
            get => (_Value, (int) _ToolStripUC._Value);
            set
            {
                var (v, saved) = value;
                var state = SetValue( v, raiseValueChangedEvent: false );
                if ( state != SetValueState.SettedCustomValue )
                {
                    if ( _ToolStripUC._Value != saved )
                    {
                        _ToolStripUC._OnValueChanged -= ToolStripDegreeOfParallelism_ValueChanged;
                        _ToolStripUC._Value = saved;
                        _ToolStripUC._OnValueChanged += ToolStripDegreeOfParallelism_ValueChanged;
                    }
                }
                if ( state != SetValueState.NoChanged )
                {
                    Fire_ValueChanged();
                }
            }
        }
        public int ValueSaved => (int) _ToolStripUC._Value;


        private _ToolStripDegreeOfParallelism_ _ToolStripUC;
        private void ToolStripDegreeOfParallelism_ValueChanged( object sender, EventArgs e ) => this.Value = (int) ((_ToolStripDegreeOfParallelism_) sender)._Value;
        protected override void FillDropDownItems()
        {
            var font = new Font( this.Font, FontStyle.Regular );
            this.DropDownItems.Add( new ToolStripMenuItemEx(  1, Suffix, ToolStripMenuItemEx_EventHandler ) { Font = font, ImageScaling = ToolStripItemImageScaling.None } );
            this.DropDownItems.Add( new ToolStripMenuItemEx(  2, Suffix, ToolStripMenuItemEx_EventHandler ) { Font = font, ImageScaling = ToolStripItemImageScaling.None } );
            this.DropDownItems.Add( new ToolStripMenuItemEx(  4, Suffix, ToolStripMenuItemEx_EventHandler ) { Font = font, ImageScaling = ToolStripItemImageScaling.None } );
            this.DropDownItems.Add( new ToolStripMenuItemEx(  8, Suffix, ToolStripMenuItemEx_EventHandler ) { Font = font, ImageScaling = ToolStripItemImageScaling.None } );
            this.DropDownItems.Add( new ToolStripMenuItemEx( 12, Suffix, ToolStripMenuItemEx_EventHandler ) { Font = font, ImageScaling = ToolStripItemImageScaling.None } );
            this.DropDownItems.Add( new ToolStripMenuItemEx( 16, Suffix, ToolStripMenuItemEx_EventHandler ) { Font = font, ImageScaling = ToolStripItemImageScaling.None } );
            this.DropDownItems.Add( new ToolStripMenuItemEx( 24, Suffix, ToolStripMenuItemEx_EventHandler ) { Font = font, ImageScaling = ToolStripItemImageScaling.None } );
            this.DropDownItems.Add( new ToolStripMenuItemEx( 32, Suffix, ToolStripMenuItemEx_EventHandler ) { Font = font, ImageScaling = ToolStripItemImageScaling.None } );
            this.DropDownItems.Add( new ToolStripMenuItemEx( 64, Suffix, ToolStripMenuItemEx_EventHandler ) { Font = font, ImageScaling = ToolStripItemImageScaling.None } );

            this.DropDownItems.Add( new ToolStripSeparator() );
            this.DropDownItems.Add( _ToolStripUC = new _ToolStripDegreeOfParallelism_( ToolStripDegreeOfParallelism_ValueChanged ) { Font = font } );
        }
    }
    //------------------------------------------------------------------------------------------//

    /// <summary>
    /// 
    /// </summary>
    internal abstract class _ToolStripDropDownButtonEx__DecimalValue : _ToolStripDropDownButtonEx_Base< decimal? >
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
    internal sealed class SpeedThresholdToolButton : _ToolStripDropDownButtonEx__DecimalValue
    {
        private const string MAX_SPEED = "Max (unlim)";
        private const string MBPS      = "Mbps";

        private Font _Font;
        public SpeedThresholdToolButton() { }
        protected override void Dispose( bool disposing )
        {
            if ( disposing )
            {
                _Font?.Dispose();
            }
            base.Dispose( disposing );
        }

        protected override string MainToolTipText   => "speed limit";
        protected override Image  MainImage         => _Resources_.speed_main_1;
        protected override Color  MainForeColor     => Color.CadetBlue;
        protected override Color  SelectedBackColor => Color.LightGreen;

        public override decimal? Value 
        { 
            get => _Value;
            set => SetValue( value );
        }

        /// <summary>
        /// 
        /// </summary>
        private enum SetValueState
        {
            NoChanged,
            SettedPredefineValue,
            SettedCustomValue,
        }
        private SetValueState SetValue( decimal? value, bool raiseValueChangedEvent = true )
        {
            var state = SetValueState.NoChanged;

            if ( !IsEqual( _Value, value ) )
            {
                _Value = value;

                var t = (value.HasValue ? $"{value.Value} {MBPS}" : MAX_SPEED);
                this.Text        = t;
                this.ToolTipText = $"{MainToolTipText}: {t}";
                this.Image       = MainImage;

                _Font?.Dispose();
                _Font = new Font( this.Font, FontStyle.Regular );
                var was_found_mi = false;
                foreach ( var mi in this.DropDownItems.Cast< ToolStripItem >().OfType< ToolStripMenuItemEx >() )
                {
                    if ( IsEqual( mi.Value, value ) )
                    {
                        mi.BackColor = this.SelectedBackColor;
                        mi.Font      = this.Font;

                        this.Image       = mi.Image;
                        this.ForeColor   = mi.ForeColor;
                        this.Text        = mi.Text;
                        this.ToolTipText = $"{MainToolTipText}: {mi.Text}";
                        was_found_mi = true;
                    }
                    else
                    {
                        mi.BackColor = this.BackColor;
                        mi.Font      = _Font;
                    }
                }

                if ( was_found_mi )
                {
                    _ToolStripUC.BackColor = this.BackColor;
                    //_ToolStripSpeedThreshold.Font = _Font;

                    state = SetValueState.SettedPredefineValue;
                }
                else if ( _Value.HasValue )
                {
                    _ToolStripUC.BackColor = this.SelectedBackColor;
                    //_ToolStripSpeedThreshold.Font = this.Font;

                    var val_dec = _Value.Value;
                    if ( _ToolStripUC._Value != val_dec )
                    {
                        _ToolStripUC._Value = val_dec;                        
                    }
                    state = SetValueState.SettedCustomValue;
                }

                if ( (state != SetValueState.NoChanged) && raiseValueChangedEvent )
                {
                    Fire_ValueChanged();
                }
            }

            return (state);
        }

        public (decimal? value, decimal valueSaved) ValueWithSaved
        {
            get => (_Value, _ToolStripUC._Value);
            set
            {
                var (v, saved) = value;
                var state = SetValue( v, raiseValueChangedEvent: false );
                if ( state != SetValueState.SettedCustomValue )
                {
                    if ( _ToolStripUC._Value != saved )
                    {
                        _ToolStripUC._OnValueChanged -= ToolStripSpeedThreshold_ValueChanged;
                        _ToolStripUC._Value = saved;
                        _ToolStripUC._OnValueChanged += ToolStripSpeedThreshold_ValueChanged;
                    }
                }
                if ( state != SetValueState.NoChanged )
                {
                    Fire_ValueChanged();
                }
            }
        }
        public decimal ValueSaved => _ToolStripUC._Value;


        private _ToolStripSpeedThreshold_ _ToolStripUC;
        private void ToolStripSpeedThreshold_ValueChanged( object sender, EventArgs e ) => this.Value = ((_ToolStripSpeedThreshold_) sender)._Value;
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
            this.DropDownItems.Add( new ToolStripMenuItemEx( 0.7M, ToolStripMenuItemEx_EventHandler ) { Text = $"0.7 {MBPS}", ForeColor = Color.FromArgb( 0, 0, 0 ), Image = _Resources_.speed_6.ToBitmap(), Font = font, ImageScaling = ToolStripItemImageScaling.None } );
            this.DropDownItems.Add( new ToolStripMenuItemEx( 0.5M, ToolStripMenuItemEx_EventHandler ) { Text = $"0.5 {MBPS}", ForeColor = Color.FromArgb( 0, 0, 0 ), Image = _Resources_.speed_6.ToBitmap(), Font = font, ImageScaling = ToolStripItemImageScaling.None } );
            this.DropDownItems.Add( new ToolStripMenuItemEx( 0.3M, ToolStripMenuItemEx_EventHandler ) { Text = $"0.3 {MBPS}", ForeColor = Color.FromArgb( 0, 0, 0 ), Image = _Resources_.speed_6.ToBitmap(), Font = font, ImageScaling = ToolStripItemImageScaling.None } );
            this.DropDownItems.Add( new ToolStripMenuItemEx( 0.1M, ToolStripMenuItemEx_EventHandler ) { Text = $"0.1 {MBPS}", ForeColor = Color.FromArgb( 0, 0, 0 ), Image = _Resources_.speed_6.ToBitmap(), Font = font, ImageScaling = ToolStripItemImageScaling.None } );

            this.DropDownItems.Add( new ToolStripSeparator() );
            this.DropDownItems.Add( _ToolStripUC = new _ToolStripSpeedThreshold_( ToolStripSpeedThreshold_ValueChanged ) { Font = font } );

            this.Value = null;
        }
    }
    //------------------------------------------------------------------------------------------//
}
