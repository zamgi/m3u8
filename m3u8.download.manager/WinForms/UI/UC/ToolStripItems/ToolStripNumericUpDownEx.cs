namespace System.Windows.Forms
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class ToolStripNumericUpDownEx : ToolStripControlHost
    {
        public ToolStripNumericUpDownEx() : base( new NumericUpDownEx() ) { }

        public NumericUpDownEx NumericUpDownEx => Control as NumericUpDownEx;

        public decimal Value
        {
            get => NumericUpDownEx.Value;
            set => NumericUpDownEx.Value = value;
        }

        // Subscribe and unsubscribe the control events you wish to expose.
        protected override void OnSubscribeControlEvents( Control c )
        {
            // Call the base so the base events are connected.
            base.OnSubscribeControlEvents( c );

            // Cast the control to a NumericUpDownEx control.
            var nud = (NumericUpDownEx) c;

            // Add the event.
            nud.TextChanged  += new EventHandler( OnTextChanged );
            nud.ValueChanged += new EventHandler( OnValueChanged );
        }
        protected override void OnUnsubscribeControlEvents( Control c )
        {
            // Call the base method so the basic events are unsubscribed.
            base.OnUnsubscribeControlEvents( c );

            // Cast the control to a NumericUpDownEx control.
            var nud = (NumericUpDownEx) c;

            // Remove the event.
            nud.TextChanged  -= new EventHandler( OnTextChanged );
            nud.ValueChanged -= new EventHandler( OnValueChanged );
        }

        public event EventHandler NumericUpDownEx_TextChanged;
        public event EventHandler NumericUpDownEx_ValueChanged;

        private void OnTextChanged( object sender, EventArgs e ) => NumericUpDownEx_TextChanged?.Invoke( this, e );
        private void OnValueChanged( object sender, EventArgs e ) => NumericUpDownEx_ValueChanged?.Invoke( this, e );
    }
}
