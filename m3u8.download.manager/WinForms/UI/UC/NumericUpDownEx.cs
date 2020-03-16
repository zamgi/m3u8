namespace System.Windows.Forms
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class NumericUpDownEx : NumericUpDown
    {
        protected override void OnMouseWheel( MouseEventArgs e )
        {
            if ( e is HandledMouseEventArgs hme )
            {
                hme.Handled = true;
            }

            if ( 0 < e.Delta )
            {
                var v = this.Value + this.Increment;
                this.Value = Math.Min( this.Maximum, v );
            }
            else if ( e.Delta < 0 )
            {
                var v = this.Value - this.Increment;
                this.Value = Math.Max( this.Minimum, v );
            }
        }
    }
}
