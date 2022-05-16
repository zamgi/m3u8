namespace m3u8.download.manager.ui
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            System.Windows.Forms.ToolStripSeparator s1 = new System.Windows.Forms.ToolStripSeparator();
            System.Windows.Forms.ToolStripSeparator s2 = new System.Windows.Forms.ToolStripSeparator();
            System.Windows.Forms.ToolStripSeparator s3 = new System.Windows.Forms.ToolStripSeparator();
            System.Windows.Forms.ToolStripSeparator s4 = new System.Windows.Forms.ToolStripSeparator();
            System.Windows.Forms.ToolStripSeparator s5 = new System.Windows.Forms.ToolStripSeparator();
            //---System.Windows.Forms.ToolStripSeparator s6 = new System.Windows.Forms.ToolStripSeparator();
            System.Windows.Forms.ToolStripSeparator s7 = new System.Windows.Forms.ToolStripSeparator();
            System.Windows.Forms.ToolStripSeparator s8 = new System.Windows.Forms.ToolStripSeparator();
            System.Windows.Forms.ToolStripSeparator s9 = new System.Windows.Forms.ToolStripSeparator();
            System.Windows.Forms.ToolStripSeparator s10 = new System.Windows.Forms.ToolStripSeparator();
            System.Windows.Forms.ToolStripSeparator s11 = new System.Windows.Forms.ToolStripSeparator();
            this.mainToolStrip = new System.Windows.Forms.ToolStrip();
            this.addNewDownloadToolButton = new System.Windows.Forms.ToolStripButton();
            this.startDownloadToolButton = new System.Windows.Forms.ToolStripButton();
            this.pauseDownloadToolButton = new System.Windows.Forms.ToolStripButton();
            this.cancelDownloadToolButton = new System.Windows.Forms.ToolStripButton();
            this.deleteDownloadToolButton = new System.Windows.Forms.ToolStripButton();
            this.deleteAllFinishedDownloadToolButton = new System.Windows.Forms.ToolStripButton();
            this.showLogToolButton = new System.Windows.Forms.ToolStripButton();
            this.copyToolButton = new System.Windows.Forms.ToolStripButton();
            this.pasteToolButton = new System.Windows.Forms.ToolStripButton();
            this.aboutToolButton = new System.Windows.Forms.ToolStripButton();
            this.degreeOfParallelismToolButton = new System.Windows.Forms.DegreeOfParallelismToolButton();
            this.downloadInstanceToolButton = new System.Windows.Forms.DownloadInstanceToolButton();
            this.mainSplitContainer = new System.Windows.Forms.SplitContainer();
            this.downloadListUC = new m3u8.download.manager.ui.DownloadListUC();
            this.logUC = new m3u8.download.manager.ui.LogUC();
            this.mainContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.startDownloadMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pauseDownloadMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cancelDownloadMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteDownloadMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteWithOutputFileMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.browseOutputFileMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openOutputFileMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openOutputFilesWithExternalMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.changeOutputDirectoryMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteAllFinishedDownloadMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.startAllDownloadsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pauseAllDownloadsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cancelAllDownloadsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteAllDownloadsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteAllWithOutputFilesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusBarUC = new m3u8.download.manager.ui.StatusBarUC();
            this.mainToolStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).BeginInit();
            this.mainSplitContainer.Panel1.SuspendLayout();
            this.mainSplitContainer.Panel2.SuspendLayout();
            this.mainSplitContainer.SuspendLayout();
            this.mainContextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainToolStrip
            // 
            this.mainToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.mainToolStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.mainToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.addNewDownloadToolButton,
                s1,
                this.startDownloadToolButton,
                this.pauseDownloadToolButton,
                this.cancelDownloadToolButton,
                s2,
                this.deleteDownloadToolButton,
                this.deleteAllFinishedDownloadToolButton,
                s3,
                this.showLogToolButton,
                s4,
                this.copyToolButton,
                this.pasteToolButton,
                s5,
                this.degreeOfParallelismToolButton,
                this.downloadInstanceToolButton,
                //---s6,
                this.aboutToolButton
            });
            this.mainToolStrip.TabIndex = 0;
            // 
            // addNewDownloadToolButton
            // 
            this.addNewDownloadToolButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.addNewDownloadToolButton.Image = global::m3u8.download.manager.Properties.Resources.plus;
            this.addNewDownloadToolButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.addNewDownloadToolButton.ToolTipText = "Add new download  (Insert)";
            this.addNewDownloadToolButton.Click += new System.EventHandler(this.addNewDownloadToolButton_Click);
            // 
            // startDownloadToolButton
            // 
            this.startDownloadToolButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.startDownloadToolButton.Enabled = false;
            this.startDownloadToolButton.Image = global::m3u8.download.manager.Properties.Resources.control_start;
            this.startDownloadToolButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.startDownloadToolButton.ToolTipText = "Start download  (Ctrl + S)";
            this.startDownloadToolButton.Click += new System.EventHandler(this.startDownloadToolButton_Click);
            // 
            // pauseDownloadToolButton
            // 
            this.pauseDownloadToolButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.pauseDownloadToolButton.Enabled = false;
            this.pauseDownloadToolButton.Image = global::m3u8.download.manager.Properties.Resources.control_pause;
            this.pauseDownloadToolButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.pauseDownloadToolButton.ToolTipText = "Pause download  (Ctrl + P)";
            this.pauseDownloadToolButton.Click += new System.EventHandler(this.pauseDownloadToolButton_Click);
            // 
            // cancelDownloadToolButton
            // 
            this.cancelDownloadToolButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.cancelDownloadToolButton.Enabled = false;
            this.cancelDownloadToolButton.Image = global::m3u8.download.manager.Properties.Resources.control_cancel;
            this.cancelDownloadToolButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.cancelDownloadToolButton.ToolTipText = "Cancel download  (Ctrl + Z)";
            this.cancelDownloadToolButton.Click += new System.EventHandler(this.cancelDownloadToolButton_Click);
            // 
            // deleteDownloadToolButton
            // 
            this.deleteDownloadToolButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.deleteDownloadToolButton.Enabled = false;
            this.deleteDownloadToolButton.Image = global::m3u8.download.manager.Properties.Resources.control_delete;
            this.deleteDownloadToolButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.deleteDownloadToolButton.ToolTipText = "Delete download  (Del)\r\nWith output file  (Shift + Del)";
            this.deleteDownloadToolButton.Click += new System.EventHandler(this.deleteDownloadToolButton_Click);
            // 
            // deleteAllFinishedDownloadToolButton
            // 
            this.deleteAllFinishedDownloadToolButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.deleteAllFinishedDownloadToolButton.Enabled = false;
            this.deleteAllFinishedDownloadToolButton.Image = global::m3u8.download.manager.Properties.Resources.control_delete_all_finished;
            this.deleteAllFinishedDownloadToolButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.deleteAllFinishedDownloadToolButton.ToolTipText = "Delete all finished downloads";
            this.deleteAllFinishedDownloadToolButton.Click += new System.EventHandler(this.deleteAllFinishedDownloadToolButton_Click);
            // 
            // showLogToolButton
            // 
            this.showLogToolButton.Checked = true;
            this.showLogToolButton.CheckOnClick = true;
            this.showLogToolButton.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showLogToolButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.showLogToolButton.Image = global::m3u8.download.manager.Properties.Resources.log;
            this.showLogToolButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.showLogToolButton.ToolTipText = "Show/Hide download log";
            this.showLogToolButton.Click += new System.EventHandler(this.showLogToolButton_Click);
            // 
            // copyToolButton
            // 
            this.copyToolButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.copyToolButton.Image = global::m3u8.download.manager.Properties.Resources.copy;
            this.copyToolButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.copyToolButton.ToolTipText = "Copy  (Ctrl + C)";
            this.copyToolButton.Click += new System.EventHandler(this.copyToolButton_Click);
            // 
            // pasteToolButton
            // 
            this.pasteToolButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.pasteToolButton.Image = global::m3u8.download.manager.Properties.Resources.paste;
            this.pasteToolButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.pasteToolButton.ToolTipText = "Paste  (Ctrl + V)";
            this.pasteToolButton.Click += new System.EventHandler(this.pasteToolButton_Click);
            // 
            // aboutToolButton
            // 
            this.aboutToolButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.aboutToolButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.aboutToolButton.Image = global::m3u8.download.manager.Properties.Resources.help;
            this.aboutToolButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.aboutToolButton.ToolTipText = "About";
            this.aboutToolButton.Click += new System.EventHandler(this.aboutToolButton_Click);
            // 
            // degreeOfParallelismToolButton
            // 
            this.degreeOfParallelismToolButton.ValueChanged += new System.Windows.Forms.ToolStripDropDownButtonEx.ValueChangedEventHandler(this.degreeOfParallelismToolButton_ValueChanged);
            // 
            // downloadInstanceToolButton
            //             
            this.downloadInstanceToolButton.ValueChanged += new System.Windows.Forms.ToolStripDropDownButtonEx.ValueChangedEventHandler(this.downloadInstanceToolButton_ValueChanged);
            // 
            // mainSplitContainer
            // 
            this.mainSplitContainer.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this.mainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // mainSplitContainer.Panel1
            // 
            this.mainSplitContainer.Panel1.BackColor = System.Drawing.SystemColors.Control;
            this.mainSplitContainer.Panel1.Controls.Add(this.downloadListUC);
            // 
            // mainSplitContainer.Panel2
            // 
            this.mainSplitContainer.Panel2.BackColor = System.Drawing.SystemColors.Control;
            this.mainSplitContainer.Panel2.Controls.Add(this.logUC);
            this.mainSplitContainer.SplitterDistance = 230;
            this.mainSplitContainer.TabIndex = 1;
            // 
            // downloadListUC
            // 
            this.downloadListUC.Dock = System.Windows.Forms.DockStyle.Fill;
            this.downloadListUC.TabIndex = 0;
            this.downloadListUC.SelectionChanged += new m3u8.download.manager.ui.DownloadListUC.SelectionChangedEventHandler(this.downloadListUC_SelectionChanged);
            this.downloadListUC.OutputFileNameClick += new m3u8.download.manager.ui.DownloadListUC.OutputFileNameClickEventHandler(this.downloadListUC_OutputFileNameClick);
            this.downloadListUC.OutputDirectoryClick += new m3u8.download.manager.ui.DownloadListUC.OutputDirectoryClickEventHandler(this.downloadListUC_OutputDirectoryClick);
            this.downloadListUC.IsDrawCheckMark += new DownloadListUC.IsDrawCheckMarkDelegate(this.downloadListUC_IsDrawCheckMark);
            // 
            // logUC
            // 
            this.logUC.Dock = System.Windows.Forms.DockStyle.Fill;
            this.logUC.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.5F);
            this.logUC.TabIndex = 0;            
            // 
            // mainContextMenu
            // 
            this.mainContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.startDownloadMenuItem,
                this.pauseDownloadMenuItem,
                this.cancelDownloadMenuItem,
                s7,
                this.deleteDownloadMenuItem,
                this.deleteWithOutputFileMenuItem,
                s8,
                this.browseOutputFileMenuItem,
                this.openOutputFileMenuItem,
                this.openOutputFilesWithExternalMenuItem,
                this.changeOutputDirectoryMenuItem,
                s9,
                this.deleteAllFinishedDownloadMenuItem,
                s10,
                this.startAllDownloadsMenuItem,
                this.pauseAllDownloadsMenuItem,
                this.cancelAllDownloadsMenuItem,
                s11,
                this.deleteAllDownloadsMenuItem,
                this.deleteAllWithOutputFilesMenuItem
            });
            // 
            // startDownloadMenuItem
            // 
            this.startDownloadMenuItem.Image = global::m3u8.download.manager.Properties.Resources.running;
            this.startDownloadMenuItem.ShortcutKeyDisplayString = "(Ctrl + S)";
            this.startDownloadMenuItem.Text = "Start";
            this.startDownloadMenuItem.Click += new System.EventHandler(this.startDownloadMenuItem_Click);
            // 
            // pauseDownloadMenuItem
            // 
            this.pauseDownloadMenuItem.Image = global::m3u8.download.manager.Properties.Resources.paused;
            this.pauseDownloadMenuItem.ShortcutKeyDisplayString = "(Ctrl + P)";
            this.pauseDownloadMenuItem.Text = "Pause";
            this.pauseDownloadMenuItem.Click += new System.EventHandler(this.pauseDownloadMenuItem_Click);
            // 
            // cancelDownloadMenuItem
            // 
            this.cancelDownloadMenuItem.Image = global::m3u8.download.manager.Properties.Resources.cancel;
            this.cancelDownloadMenuItem.ShortcutKeyDisplayString = "(Ctrl + Z)";
            this.cancelDownloadMenuItem.Text = "Cancel";
            this.cancelDownloadMenuItem.Click += new System.EventHandler(this.cancelDownloadMenuItem_Click);
            // 
            // deleteDownloadMenuItem
            // 
            this.deleteDownloadMenuItem.Image = global::m3u8.download.manager.Properties.Resources.delete;
            this.deleteDownloadMenuItem.ShortcutKeyDisplayString = "(Del)";
            this.deleteDownloadMenuItem.Text = "Delete";
            this.deleteDownloadMenuItem.Click += new System.EventHandler(this.deleteDownloadMenuItem_Click);
            // 
            // deleteWithOutputFileMenuItem
            // 
            this.deleteWithOutputFileMenuItem.ShortcutKeyDisplayString = "(Shift + Del)";
            this.deleteWithOutputFileMenuItem.Text = "Delete with Output file";
            this.deleteWithOutputFileMenuItem.Click += new System.EventHandler(this.deleteWithOutputFileMenuItem_Click);
            // 
            // browseOutputFileMenuItem
            // 
            this.browseOutputFileMenuItem.ShortcutKeyDisplayString = "(Ctrl + B)";
            this.browseOutputFileMenuItem.Text = "    Browse";
            this.browseOutputFileMenuItem.Click += new System.EventHandler(this.browseOutputFileMenuItem_Click);
            // 
            // openOutputFileMenuItem
            // 
            this.openOutputFileMenuItem.ShortcutKeyDisplayString = "(Ctrl + O), (Enter)";
            this.openOutputFileMenuItem.Text = "    Open";
            this.openOutputFileMenuItem.Click += new System.EventHandler(this.openOutputFileMenuItem_Click);
            // 
            // openOutputFilesWithExternalMenuItem
            // 
            this.openOutputFilesWithExternalMenuItem.ShortcutKeyDisplayString = "(Ctrl + E)";
            this.openOutputFilesWithExternalMenuItem.Text = "    Open with '%EXTERNAL-PROG%'";
            this.openOutputFilesWithExternalMenuItem.Click += new System.EventHandler(this.openOutputFilesWithExternalMenuItem_Click);
            // 
            // changeOutputDirectoryMenuItem
            // 
            //---this.changeOutputDirectoryMenuItem.ShortcutKeyDisplayString = "(Ctrl + L)";
            this.changeOutputDirectoryMenuItem.Text = "    Change output directory";
            this.changeOutputDirectoryMenuItem.Click += new System.EventHandler(this.changeOutputDirectoryMenuItem_Click);
            // 
            // deleteAllFinishedDownloadMenuItem
            // 
            this.deleteAllFinishedDownloadMenuItem.Image = global::m3u8.download.manager.Properties.Resources.control_delete_all_finished;
            this.deleteAllFinishedDownloadMenuItem.Text = "Delete all finished downloads";
            this.deleteAllFinishedDownloadMenuItem.Click += new System.EventHandler(this.deleteAllFinishedDownloadToolButton_Click);
            // 
            // startAllDownloadsMenuItem
            // 
            this.startAllDownloadsMenuItem.Image = global::m3u8.download.manager.Properties.Resources.start_all;
            this.startAllDownloadsMenuItem.Text = "      Start all...";
            this.startAllDownloadsMenuItem.Click += new System.EventHandler(this.startAllDownloadsMenuItem_Click);
            // 
            // pauseAllDownloadsMenuItem
            // 
            this.pauseAllDownloadsMenuItem.Image = global::m3u8.download.manager.Properties.Resources.pause_all;
            this.pauseAllDownloadsMenuItem.Text = "      Pause all...";
            this.pauseAllDownloadsMenuItem.Click += new System.EventHandler(this.pauseAllDownloadsMenuItem_Click);
            // 
            // cancelAllDownloadsMenuItem
            // 
            this.cancelAllDownloadsMenuItem.Image = global::m3u8.download.manager.Properties.Resources.cancel_all;
            this.cancelAllDownloadsMenuItem.Text = "      Cancel all...";
            this.cancelAllDownloadsMenuItem.Click += new System.EventHandler(this.cancelAllDownloadsMenuItem_Click);
            // 
            // deleteAllDownloadsMenuItem
            // 
            this.deleteAllDownloadsMenuItem.Image = global::m3u8.download.manager.Properties.Resources.delete_all;
            this.deleteAllDownloadsMenuItem.Text = "      Delete all...";
            this.deleteAllDownloadsMenuItem.Click += new System.EventHandler(this.deleteAllDownloadsMenuItem_Click);
            // 
            // deleteAllWithOutputFilesMenuItem
            // 
            this.deleteAllWithOutputFilesMenuItem.Text = "      Delete all with Output files...";
            this.deleteAllWithOutputFilesMenuItem.Click += new System.EventHandler(this.deleteAllWithOutputFilesMenuItem_Click);
            // 
            // statusBarUC
            // 
            this.statusBarUC.AutoSize = true;
            this.statusBarUC.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.statusBarUC.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.statusBarUC.IsVisibleExcludesWordsLabel = true;
            this.statusBarUC.IsVisibleParallelismLabel = true;
            this.statusBarUC.IsVisibleSettingsLabel = true;
            this.statusBarUC.LeftSideTextLabelText = null;
            this.statusBarUC.Location = new System.Drawing.Point(0, 540);
            this.statusBarUC.Margin = new System.Windows.Forms.Padding(0);
            this.statusBarUC.TabIndex = 4;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1050, 579);
            this.Controls.Add(this.mainSplitContainer);
            this.Controls.Add(this.statusBarUC);
            this.Controls.Add(this.mainToolStrip);
            this.Icon = global::m3u8.download.manager.Properties.Resources.m3u8_32x36;
            this.KeyPreview = true;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.mainToolStrip.ResumeLayout(false);
            this.mainToolStrip.PerformLayout();
            this.mainSplitContainer.Panel1.ResumeLayout(false);
            this.mainSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).EndInit();
            this.mainSplitContainer.ResumeLayout(false);
            this.mainContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        #endregion

        private System.Windows.Forms.ToolStrip mainToolStrip;
        private System.Windows.Forms.ToolStripButton addNewDownloadToolButton;
        private System.Windows.Forms.ToolStripButton startDownloadToolButton;
        private System.Windows.Forms.ToolStripButton pauseDownloadToolButton;
        private System.Windows.Forms.ToolStripButton cancelDownloadToolButton;
        private System.Windows.Forms.ToolStripButton deleteDownloadToolButton;
        private System.Windows.Forms.ToolStripButton deleteAllFinishedDownloadToolButton;
        private System.Windows.Forms.ToolStripButton showLogToolButton;
        private System.Windows.Forms.ToolStripButton copyToolButton;
        private System.Windows.Forms.ToolStripButton pasteToolButton;
        private System.Windows.Forms.ToolStripButton aboutToolButton;
        private System.Windows.Forms.SplitContainer mainSplitContainer;
        private DownloadListUC downloadListUC;
        private LogUC logUC;
        private StatusBarUC statusBarUC;

        private System.Windows.Forms.ContextMenuStrip mainContextMenu;
        private System.Windows.Forms.ToolStripMenuItem startDownloadMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pauseDownloadMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cancelDownloadMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteDownloadMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteWithOutputFileMenuItem;
        private System.Windows.Forms.ToolStripMenuItem browseOutputFileMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openOutputFileMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openOutputFilesWithExternalMenuItem;
        private System.Windows.Forms.ToolStripMenuItem changeOutputDirectoryMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteAllFinishedDownloadMenuItem;       
        //---private System.Windows.Forms.ToolStripMenuItem allDownloadsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem startAllDownloadsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pauseAllDownloadsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cancelAllDownloadsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteAllDownloadsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteAllWithOutputFilesMenuItem;
        private System.Windows.Forms.DegreeOfParallelismToolButton degreeOfParallelismToolButton;
        private System.Windows.Forms.DownloadInstanceToolButton downloadInstanceToolButton;
    }
}