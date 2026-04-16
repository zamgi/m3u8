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
        public enum TabPageKind
        {
            Parallelism,
            Other,
            WebProxy,
            //More,
        }

        #region [.ctor().]
        private SettingsForm() => InitializeComponent();
        public SettingsForm( DownloadController dc/*, SettingsPropertyChangeController sc*/, TabPageKind? tabPageKind = default ) : this()
        {
            parallelismSettingsUC.Init( dc );
            otherSettingsUC      .Init( dc );
            //webProxyUC.SetWebProxyInfo( sc.GetDefaultWebProxyInfo() );

            if ( tabPageKind.HasValue )
            {
                switch ( tabPageKind )
                {
                    case TabPageKind.Parallelism: tabControl.SelectedTab = parallelismTabPage; break;
                    case TabPageKind.Other      : tabControl.SelectedTab = otherTabPage;       break;
                    case TabPageKind.WebProxy   : tabControl.SelectedTab = webProxyTabPage;    break;    
                    //case SettingsTabEnum.More       : tabControl.SelectedTab = moreTabPage;        break;
                }
            }

            #region [.ImageList 4 tabControl.]
            var imgLst = tabControl.ImageList = new ImageList() { ImageSize = new Size(16, 16) };
            imgLst.Images.Add( Resources.dop             ); parallelismTabPage.ImageIndex = 0;
            imgLst.Images.Add( Resources.settings_ico    ); otherTabPage      .ImageIndex = 1;
            imgLst.Images.Add( Resources.workgroup_16x16 ); webProxyTabPage   .ImageIndex = 2;
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
