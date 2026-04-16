using System;
using System.Drawing;
using System.Windows.Forms;

using m3u8.download.manager.controllers;
using m3u8.download.manager.Properties;

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
            WebProxy,
            //More,
        }

        #region [.ctor().]
        private SettingsForm() => InitializeComponent();
        public SettingsForm( DownloadController dc/*, SettingsPropertyChangeController sc*/, SettingsTabEnum? settingsTab = default ) : this()
        {
            parallelismSettingsUC.Init( dc );
            otherSettingsUC      .Init( dc );
            //webProxyUC.SetWebProxyInfo( sc.GetDefaultWebProxyInfo() );

            if ( settingsTab.HasValue )
            {
                switch ( settingsTab )
                {
                    case SettingsTabEnum.Parallelism: tabControl.SelectedTab = parallelismTabPage; break;
                    case SettingsTabEnum.Other      : tabControl.SelectedTab = otherTabPage;       break;
                    case SettingsTabEnum.WebProxy   : tabControl.SelectedTab = webProxyTabPage;    break;    
                    //case SettingsTabEnum.More       : tabControl.SelectedTab = moreTabPage;        break;
                }
            }

            #region [.ImageList 4 tabControl.]
            var imgLst = tabControl.ImageList = new ImageList() { ImageSize = new Size(16, 16) };
            imgLst.Images.Add( Resources.dop_16   ); parallelismTabPage.ImageIndex = 0;
            imgLst.Images.Add( Resources.settings ); otherTabPage      .ImageIndex = 1;
            imgLst.Images.Add( Resources.domain   ); webProxyTabPage   .ImageIndex = 2;
            #endregion
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
        protected override void OnFormClosing( FormClosingEventArgs e )
        {
            base.OnFormClosing( e );

            otherSettingsUC.OnClosing( DialogResult, e );
        }
        private Color tabControl_GetForecolorForTabPageText( TabPage tabPage )
        {
            var color = (tabPage == parallelismTabPage) ? Color.DarkOliveGreen
                                                        : ((tabPage == otherTabPage) ? Color.DarkOrange
                                                                                     : ((tabPage == webProxyTabPage) ? Color.Blue : Color.Silver));
            return (color);
        }
        private void tabControl_Selected( object sender, TabControlEventArgs e )
        {
            if ( e.TabPage == otherTabPage )
            {
                otherSettingsUC.StartShowTotalMemory();
            }
        }

        #region [.public props.]
        public ParallelismSettingsUC Parallelism => parallelismSettingsUC;
        public OtherSettingsUC Other => otherSettingsUC;
        public WebProxyUC WebProxy => webProxyUC;
        #endregion
    }
}
