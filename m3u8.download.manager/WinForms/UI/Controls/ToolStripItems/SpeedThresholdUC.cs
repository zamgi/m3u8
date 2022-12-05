using System;
using System.Drawing;
using System.Windows.Forms;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed partial class SpeedThresholdUC : UserControl
    {
        public SpeedThresholdUC() => InitializeComponent();
        protected override void Dispose( bool disposing )
        {
            if ( disposing )
            {
                components?.Dispose();
            }
            base.Dispose( disposing );
        }


        public int SpeedThreshold_Value
        {
            get => Convert.ToInt32( speedThresholdNumericUpDownEx.Value );
            set => speedThresholdNumericUpDownEx.Value = value;
        }

        public event EventHandler SpeedThreshold_ValueChanged
        {
            add    => speedThresholdNumericUpDownEx.ValueChanged += value;
            remove => speedThresholdNumericUpDownEx.ValueChanged -= value;
        }
        public event EventHandler SpeedThreshold_TextChanged
        {
            add    => speedThresholdNumericUpDownEx.TextChanged += value;
            remove => speedThresholdNumericUpDownEx.TextChanged -= value;
        }


        private void SpeedThresholdNumericUpDownEx_KeyDown( object sender, KeyEventArgs e )
        {
            if ( e.KeyCode == Keys.Enter )
            {
                UC_Click( sender, e );
            }
        }
        private void SpeedThresholdNumericUpDownEx_GotFocus( object sender, EventArgs e ) => this.Parent?.Refresh();
        private void UC_Click( object sender, EventArgs e )
        {
            var p = this.Parent;
            if ( p != null )
            {
                p.Focus();
                if ( p is ToolStripDropDownMenu toolStripDropDownMenu )
                {
                    speedThresholdNumericUpDownEx.Fire_ValueChanged();
                    toolStripDropDownMenu.Close();                    
                }
            }
        }

        private void L1_MouseEnter( object sender, EventArgs e ) => l1.ForeColor = Color.DimGray;
        private void L1_MouseLeave( object sender, EventArgs e ) => l1.ForeColor = Color.Black;
    }
}
