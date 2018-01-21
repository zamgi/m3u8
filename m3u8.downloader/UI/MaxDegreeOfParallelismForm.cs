using System;
using System.Windows.Forms;

namespace m3u8
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed partial class MaxDegreeOfParallelismForm : Form
    {
        public MaxDegreeOfParallelismForm()
        {
            InitializeComponent();

            numericUpDown.Maximum = int.MaxValue;
        }

        private void infinityCheckBox_CheckedChanged( object sender, EventArgs e )
        {
            numericUpDown.Enabled = !infinityCheckBox.Checked;
        }

        public int MaxDegreeOfParallelism
        {
            get { return (IsInfinity ? int.MaxValue : Convert.ToInt32( numericUpDown.Value )); }
            set
            {
                numericUpDown   .Value   = value;
                numericUpDown   .Enabled = (value != int.MaxValue);
                infinityCheckBox.Checked = (value == int.MaxValue);
            }
        }
        public bool IsInfinity
        {
            get { return (infinityCheckBox.Checked); }
            set
            {
                infinityCheckBox.Checked = value;
                numericUpDown   .Enabled = !value;
            }
        }
    }
}
