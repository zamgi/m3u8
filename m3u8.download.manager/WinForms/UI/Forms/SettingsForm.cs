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
            //More,
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
                    //case SettingsTabEnum.More       : ((TabControl) moreTabPage       .Parent).SelectedTab = moreTabPage;        break;
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

            if ( otherTabPage.IsSelected() )
            {
                otherSettingsUC.StartShowTotalMemory();
            }
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

            var color = (tabPage == parallelismTabPage) ? Brushes.DarkOliveGreen : ((tabPage == otherTabPage) ? Brushes.DarkOrange : Brushes.Silver);

            using var sf = new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center, Trimming = StringTrimming.None, FormatFlags = StringFormatFlags.NoWrap };
            //      var fs = ((tabPage == moreTabPage) || ((e.State & DrawItemState.Selected) != DrawItemState.Selected)) ? FontStyle.Regular : FontStyle.Underline;
            //using var ft = new Font( tabPage.Font, fs );
            e.Graphics.DrawString( tabPage.Text, /*ft*/tabPage.Font, color, e.Bounds, sf );

            #region comm.
            //ALWAYS DRAW ITS OWNER CONTROL
            //if ( (e.State & DrawItemState.Selected) == DrawItemState.Selected ) //if ( (e.State & DrawItemState.Focus) == DrawItemState.Focus )
            //{
            //    var rc = e.Bounds;
            //    rc.Inflate( -2, -2 );

            //    ControlPaint.DrawFocusRectangle( e.Graphics, rc );
            //    //e.Graphics.DrawRectangle( Pens.Silver, rc );
            //    //e.DrawFocusRectangle();
            //}
            #endregion
        }
        private void TabControl_Selected( object sender, TabControlEventArgs e )
        {
            if ( e.TabPage == otherTabPage )
            {
                otherSettingsUC.StartShowTotalMemory();
            }
        }

        #region [.public props.]
        public ParallelismSettingsUC Parallelism => parallelismSettingsUC;
        public OtherSettingsUC Other => otherSettingsUC;
        #endregion
    }
}
