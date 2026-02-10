using System.Drawing;

namespace System.Windows.Forms
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class ButtonWithFocusCues : Button
    {
        protected override bool ShowFocusCues => base.Focused;
        public bool ReadOnly
        {
            get => !this.Enabled;
            set
            {
                var enabled = !value;
                if ( this.Enabled != enabled )
                {
                    this.Enabled = enabled;
                    if ( enabled )
                    {
                        if ( _Cursor != null     ) this.Cursor    = _Cursor;
                        if ( _FlatStyle.HasValue ) this.FlatStyle = _FlatStyle.Value;
                        this.ForeColor = _ForeColor.GetValueOrDefault( Button.DefaultForeColor );
                    }
                    else
                    {
                        _Cursor    = this.Cursor;    this.Cursor    = Cursors.Default;
                        _FlatStyle = this.FlatStyle; this.FlatStyle = FlatStyle.System;
                        _ForeColor = this.ForeColor; this.ForeColor = SystemColors.GrayText;
                    }
                }
            }
        }
        private Cursor     _Cursor;
        private FlatStyle? _FlatStyle;
        private Color?     _ForeColor;
    }
}
