using System.Drawing;
using System.Linq;

namespace System.Windows.Forms
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class CheckBoxImitationDisabled : CheckBox
    {
        public static readonly Color Default_ForeColor_4_Checked    = Color.FromArgb( 70, 70, 70 );
        public static readonly Color Default_ForeColor_4_NotChecked = Color.Silver;

        private Image  _OriginImage;
        private Bitmap _DisabledBitmap;
        public CheckBoxImitationDisabled() => Set_CheckBox_ForeColorAndImage();
        protected override void Dispose( bool disposing )
        {
            if ( disposing ) _DisabledBitmap?.Dispose();
            base.Dispose( disposing );
        }

        protected override void OnCheckStateChanged( EventArgs e )
        {
            base.OnCheckStateChanged( e );
            Set_CheckBox_ForeColorAndImage();
        }
        protected override void OnEnabledChanged( EventArgs e )
        {
            base.OnEnabledChanged( e );
            Set_CheckBox_ForeColorAndImage();
        }
        protected override void OnPaint( PaintEventArgs e )
        {
            if ( this.Enabled )
            {
                base.OnPaint( e );
            }
            else
            {
                var gr = e.Graphics;
                var rc = e.ClipRectangle;

                #region [.draw CheckBox.]
                CheckBoxRenderer.DrawParentBackground( gr, rc, this );

                var chst = this.CheckState switch
                { 
                    CheckState.Checked => VisualStyles.CheckBoxState.CheckedDisabled,
                    CheckState.Unchecked => VisualStyles.CheckBoxState.UncheckedDisabled,
                    CheckState.Indeterminate => VisualStyles.CheckBoxState.MixedDisabled,
                    _ => throw (new ArgumentException())
                };
                const int Y_OFFSET = 1;
                rc.Y -= Y_OFFSET;
                var sz = CheckBoxRenderer.GetGlyphSize( gr, chst );
                var pt = rc.Location;
                pt = new Point( pt.X, pt.Y + (rc.Height - sz.Height)/2 );
                CheckBoxRenderer.DrawCheckBox( gr, pt, chst );
                #endregion

                #region [.draw text - v1.]
                rc.X     += sz.Width;
                rc.Width -= sz.Width;

                var tff = this.AutoEllipsis ? TextFormatFlags.EndEllipsis : TextFormatFlags.Default;
                tff |= this.TextAlign switch
                {
                    ContentAlignment.TopLeft => TextFormatFlags.Top | TextFormatFlags.Left,
                    ContentAlignment.MiddleLeft => TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter,
                    ContentAlignment.BottomLeft => TextFormatFlags.Bottom | TextFormatFlags.Left,

                    ContentAlignment.TopCenter => TextFormatFlags.Top | TextFormatFlags.HorizontalCenter,
                    ContentAlignment.MiddleCenter => TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter,
                    ContentAlignment.BottomCenter => TextFormatFlags.Bottom | TextFormatFlags.HorizontalCenter,

                    ContentAlignment.TopRight => TextFormatFlags.Top | TextFormatFlags.Right,
                    ContentAlignment.MiddleRight => TextFormatFlags.VerticalCenter | TextFormatFlags.Right,
                    ContentAlignment.BottomRight => TextFormatFlags.Bottom | TextFormatFlags.Right,

                    _ => TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter,
                };
                TextRenderer.DrawText( gr, this.Text, this.Font, rc, ForeColor_4_NotChecked, tff );
                #endregion

                #region comm. draw text - v2.
                /*
                const int CHECKBOX_PADDING = 4;
                sz.Width += CHECKBOX_PADDING;
                rc.X     += sz.Width;
                rc.Width -= sz.Width;

                using var br = new SolidBrush( ForeColor_4_NotChecked );
                using var sf = new StringFormat( StringFormat.GenericDefault ) 
                { 
                    FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.LineLimit, 
                    Trimming = this.AutoEllipsis ? StringTrimming.EllipsisCharacter : StringTrimming.None,
                    Alignment = this.TextAlign switch
                    {
                        ContentAlignment.TopLeft => StringAlignment.Near,
                        ContentAlignment.MiddleLeft => StringAlignment.Near,
                        ContentAlignment.BottomLeft => StringAlignment.Near,

                        ContentAlignment.TopCenter => StringAlignment.Center,
                        ContentAlignment.MiddleCenter => StringAlignment.Center,
                        ContentAlignment.BottomCenter => StringAlignment.Center,

                        ContentAlignment.TopRight => StringAlignment.Far,
                        ContentAlignment.MiddleRight => StringAlignment.Far,
                        ContentAlignment.BottomRight => StringAlignment.Far,

                        _ => StringAlignment.Center
                    },
                    LineAlignment = this.TextAlign switch
                    {
                        ContentAlignment.TopLeft => StringAlignment.Near,
                        ContentAlignment.MiddleLeft => StringAlignment.Center,
                        ContentAlignment.BottomLeft => StringAlignment.Far,

                        ContentAlignment.TopCenter => StringAlignment.Near,
                        ContentAlignment.MiddleCenter => StringAlignment.Center,
                        ContentAlignment.BottomCenter => StringAlignment.Far,

                        ContentAlignment.TopRight => StringAlignment.Near,
                        ContentAlignment.MiddleRight => StringAlignment.Center,
                        ContentAlignment.BottomRight => StringAlignment.Far,

                        _ => StringAlignment.Center
                    }
                };
                gr.DrawString( this.Text, this.Font, br, rc, sf );
                //*/
                #endregion
            }
        }
        public new Image Image 
        { 
            get => base.Image;
            set
            {
                base.Image = value;
                Set_CheckBox_ForeColorAndImage();
            }
        }

        public Color ForeColor_4_Checked    { get; set; } = Default_ForeColor_4_Checked;
        public Color ForeColor_4_NotChecked { get; set; } = Default_ForeColor_4_NotChecked;

        private void Set_CheckBox_ForeColorAndImage()
        {
            var isChecked = (this.Checked || (this.CheckState != CheckState.Unchecked)) && this.Enabled;
            if ( base.Image != null )
            {
                if ( isChecked )
                {
                    if ( _OriginImage != null ) base.Image = _OriginImage;
                }
                else
                {
                    _OriginImage = base.Image;
                    if ( _DisabledBitmap == null )
                    {
                        _DisabledBitmap = CreateDisabledImage( _OriginImage );
                    }
                    base.Image = _DisabledBitmap;
                }
            }

            this.ForeColor = isChecked ? ForeColor_4_Checked : ForeColor_4_NotChecked;
        }
        private static Bitmap CreateDisabledImage( Image origin )
        {
            var disabled = new Bitmap( origin.Width, origin.Height );
            using var gr = Graphics.FromImage( disabled );
            ControlPaint.DrawImageDisabled( gr, origin, 0, 0, Color.Transparent );
            return (disabled);
        }


        //public static void Set_AllCheckBox_ForeColorAndImage( Control c )
        //{
        //    if ( c is CheckBoxImitationDisabled ch )
        //    {
        //        ch.Set_CheckBox_ForeColorAndImage();
        //    }
        //    else
        //    {
        //        foreach ( var cc in c.Controls.Cast< Control >() )
        //        {
        //            if ( cc is CheckBoxImitationDisabled cch ) cch.Set_CheckBox_ForeColorAndImage();
        //            else Set_AllCheckBox_ForeColorAndImage( cc );
        //        }
        //    }
        //}
        public static void Set_AllCheckBox_ForeColorAndImage( ControlCollection cs )
        {
            foreach ( var c in cs.Cast< Control >() )
            {
                if ( c is CheckBoxImitationDisabled ch ) ch.Set_CheckBox_ForeColorAndImage();
                else Set_AllCheckBox_ForeColorAndImage( c.Controls );
            }
        }
    }
}
