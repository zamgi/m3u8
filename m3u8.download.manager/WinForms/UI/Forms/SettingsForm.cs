using System;
using System.ComponentModel;
using System.Windows.Forms;

using m3u8.download.manager.controllers;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed partial class SettingsForm2 : Form
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
        private SettingsForm2() => InitializeComponent();
        public SettingsForm2( DownloadController dc, SettingsTabEnum? settingsTab = default ) : this()
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

        #region [.public props.]
        public ParallelismSettingsUC Parallelism => parallelismSettingsUC;
        public OtherSettingsUC Other => otherSettingsUC;
        #endregion
    }
}
