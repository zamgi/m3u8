using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using m3u8.download.manager.controllers;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed partial class SettingsForm : Form
    {
        /// <summary>
        /// 
        /// </summary>
        public enum SettingsTabEnum
        {
            Parallelism,
            Other,
        }

        #region [.ctor().]
        private SettingsForm() => InitializeComponent();
        public SettingsForm( DownloadController dc, SettingsTabEnum? settingsTab = default ) : this()
        {
            parallelismSettingsUC.Init( dc );
            otherSettingsUC      .Init( dc );

            if ( settingsTab.HasValue )
            {
                switch ( settingsTab )
                {
                    case SettingsTabEnum.Parallelism: ((TabControl) parallelismTabPage.Parent).SelectedTab = parallelismTabPage; break;
                    case SettingsTabEnum.Other      : ((TabControl) otherTabPage      .Parent).SelectedTab = otherTabPage;       break;
                }
            }
        }

        protected override void Dispose( bool disposing )
        {
            if ( disposing )
            {
                components?.Dispose();
            }
            base.Dispose( disposing );
        }
        #endregion

        protected override void OnShown( EventArgs e )
        {
            base.OnShown( e );

            otherSettingsUC.OnShown();
        }
        protected override void OnClosing( CancelEventArgs e )
        {
            base.OnClosing( e );

            otherSettingsUC.OnClosing( DialogResult, e );
        }
        private void TabControl_DrawItem( object sender, DrawItemEventArgs e )
        {
            var tabControl = (TabControl) sender;
            var tabPage    = tabControl.TabPages[ e.Index ];
            
            //e.DrawBackground();
            using var br = new SolidBrush( tabPage.BackColor );
            e.Graphics.FillRectangle( br, e.Bounds );

            var color = (tabPage == parallelismTabPage) ? Brushes.DodgerBlue : Brushes.DarkOrange;

            using var sf = new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center, Trimming = StringTrimming.None, FormatFlags = StringFormatFlags.NoWrap };
            using var ft = new Font( tabPage.Font, FontStyle.Underline );
            e.Graphics.DrawString( tabPage.Text, ft, color, e.Bounds, sf );

            //ALWAYS DRAW ITS OWNER CONTROL
            //if ( (e.State & DrawItemState.Selected) == DrawItemState.Selected ) //if ( (e.State & DrawItemState.Focus) == DrawItemState.Focus )
            //{
            //    var rc = e.Bounds;
            //    rc.Inflate( -2, -2 );

            //    ControlPaint.DrawFocusRectangle( e.Graphics, rc );
            //    //e.Graphics.DrawRectangle( Pens.Silver, rc );
            //    //e.DrawFocusRectangle();
            //}
        }

        #region [.public props.]
        public ParallelismSettingsUC Parallelism => parallelismSettingsUC;
        public OtherSettingsUC Other => otherSettingsUC;
        #endregion
    }
}
