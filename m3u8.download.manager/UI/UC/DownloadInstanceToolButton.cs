using System.Drawing;
using _Resources_ = m3u8.download.manager.Properties.Resources;

namespace System.Windows.Forms
{
    /// <summary>
    /// 
    /// </summary>
    internal abstract class ToolStripDropDownButtonEx : ToolStripDropDownButton
    {
        /// <summary>
        /// 
        /// </summary>
        protected sealed class ToolStripMenuItemEx : ToolStripMenuItem
        {
            public ToolStripMenuItemEx( int value, EventHandler onClick ) : base( value.ToString(), null, onClick ) => Value = value;
            public int Value { get; }
        }

        public delegate void ValueChangedEventHandler( int value );
        public event ValueChangedEventHandler ValueChanged;

        protected abstract string MainToolTipText   { get; }
        protected abstract Image  MainImage         { get; }
        protected abstract Color  MainForeColor     { get; }
        protected abstract Color  SelectedBackColor { get; }

        protected abstract void FillDropDownItems();

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
            this.Text = "-";

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

                    this.Text = $"   {value}   ";
                    this.ToolTipText = MainToolTipText + ": " + value;

                    var font = new Font( this.Font, FontStyle.Regular );
                    foreach ( ToolStripMenuItemEx mi in this.DropDownItems )
                    {
                        if ( mi.Value == value )
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
    }

    /// <summary>
    /// 
    /// </summary>
    internal sealed class DownloadInstanceToolButton : ToolStripDropDownButtonEx
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
    }

    /// <summary>
    /// 
    /// </summary>
    internal sealed class DegreeOfParallelismToolButton : ToolStripDropDownButtonEx
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
            this.DropDownItems.Add( new ToolStripMenuItemEx( 32, ToolStripMenuItemEx_EventHandler ) { Font = font, ImageScaling = ToolStripItemImageScaling.None } );
            this.DropDownItems.Add( new ToolStripMenuItemEx( 64, ToolStripMenuItemEx_EventHandler ) { Font = font, ImageScaling = ToolStripItemImageScaling.None } );
            /*for ( int i = 0; i <= 7; i++ )
            {
                var pow2 = (int) Math.Pow( 2, i );
                this.DropDownItems.Add( new ToolStripMenuItemEx( pow2, ToolStripMenuItemEx_EventHandler ) { Font = font, ImageScaling = ToolStripItemImageScaling.None } );
            }*/
        }
    }
}
