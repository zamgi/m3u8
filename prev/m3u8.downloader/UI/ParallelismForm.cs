﻿using System;
using System.Windows.Forms;

namespace m3u8.downloader
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed partial class ParallelismForm : Form
    {
        #region [.ctor().]
        public ParallelismForm()
        {
            InitializeComponent();

            maxDegreeOfParallelismNUD.Maximum = int.MaxValue;
        }
        #endregion

        #region [.private methods.]
        private void infinityCheckBox_CheckedChanged( object sender, EventArgs e )
        {
            maxDegreeOfParallelismNUD                     .Enabled = !infinityCheckBox.Checked;
            useCrossAppInstanceDegreeOfParallelismCheckBox.Enabled = !infinityCheckBox.Checked;
        }
        private void maxDownloadAppInstanceCheckBox_CheckedChanged( object sender, EventArgs e )
        {
            maxDownloadAppInstanceLabel.Enabled = maxDownloadAppInstanceCheckBox.Checked;
            maxDownloadAppInstanceNUD  .Enabled = maxDownloadAppInstanceCheckBox.Checked;
        }
        #endregion

        #region [.public props.]
        public int  MaxDegreeOfParallelism
        {
            get => (IsInfinity ? int.MaxValue : Convert.ToInt32( maxDegreeOfParallelismNUD.Value ));
            set
            {
                maxDegreeOfParallelismNUD   .Value   = value;
                maxDegreeOfParallelismNUD   .Enabled = (value != int.MaxValue);
                infinityCheckBox.Checked = (value == int.MaxValue);
            }
        }
        public bool IsInfinity
        {
            get => infinityCheckBox.Checked;
            set
            {
                infinityCheckBox.Checked = value;
                maxDegreeOfParallelismNUD   .Enabled = !value;
            }
        }
        public bool UseCrossAppInstanceDegreeOfParallelism
        {
            get => !infinityCheckBox.Checked && useCrossAppInstanceDegreeOfParallelismCheckBox.Checked;
            set => useCrossAppInstanceDegreeOfParallelismCheckBox.Checked = value;
        }
        public int? MaxDownloadAppInstance
        {
            get => (maxDownloadAppInstanceCheckBox.Checked ? Convert.ToInt32( maxDownloadAppInstanceNUD.Value ) : ((int?) null));
            set
            {
                maxDownloadAppInstanceNUD     .Value   = value.GetValueOrDefault( 4 );
                maxDownloadAppInstanceCheckBox.Checked = value.HasValue;
                maxDownloadAppInstanceCheckBox_CheckedChanged( maxDownloadAppInstanceCheckBox, EventArgs.Empty );
            }
        }
        #endregion
    }
}
