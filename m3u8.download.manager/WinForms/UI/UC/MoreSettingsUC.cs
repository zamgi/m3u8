using System;
using System.Drawing;
using System.Windows.Forms;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed partial class MoreSettingsUC : UserControl
    {
        #region [.field's.]
        private Timer _GetTotalMemoryTimer;
        #endregion

        #region [.ctor().]
        public MoreSettingsUC()
        {
            InitializeComponent();

            this.SetForeColor4ParentOnly< GroupBox >( Color.DodgerBlue );
            currentMemoryLabel.ForeColor = Color.DimGray;
        }
        #endregion

        public void StartShowTotalMemory()
        {
            if ( _GetTotalMemoryTimer == null )
            {
                var tick = new EventHandler((s, e) =>
                {
                    CollectGarbage.GetTotalMemory( out var totalMemoryBytes );
                    currentMemoryLabel.Text    = $"Current Memory: {GetTotalMemoryFormatText( totalMemoryBytes )}.";
                    currentMemoryLabel.Visible = true;
                });
                _GetTotalMemoryTimer = new Timer( components ) { Interval = 1_000, Enabled = true };
                _GetTotalMemoryTimer.Tick += tick;
                tick( _GetTotalMemoryTimer, EventArgs.Empty );
            }            
        }

        #region [.private methods.]
        private static string GetTotalMemoryFormatText( long totalMemoryBytes ) => $"{(totalMemoryBytes / (1024.0 * 1024)):N2} MB";
        private void collectGarbageButton_Click( object sender, EventArgs e )
        {
            var btn = (Button) sender;
            btn.Text = "...";
            btn.Enabled = false;

            CollectGarbage.Collect_Garbage( out var totalMemoryBytes );
            //var totalMemoryBytes = await Task.Run( () => { CollectGarbage.Collect_Garbage( out var totalMemoryBytes_ ); return (totalMemoryBytes_); } );

            var text        = GetTotalMemoryFormatText( totalMemoryBytes );
            var toolTipText = $"Collect Garbage. Total Memory: {text}.";

            btn.Text = text;
            toolTip.SetToolTip( btn, toolTipText );
            btn.Enabled = true;
        }
        #endregion
    }
}
