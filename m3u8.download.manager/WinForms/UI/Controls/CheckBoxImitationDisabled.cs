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
            var isChecked = this.Checked || (this.CheckState != CheckState.Unchecked);
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
