namespace m3u8.download.manager.ui
{
    partial class StatusBarUC
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.statusBar = new System.Windows.Forms.StatusStrip();
            this.leftSideTextLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.leftSideTextLabel_2 = new ToolStripStatusLabelEx();
            this.exceptionWordsLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.parallelismLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.settingsLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusBar
            // 
            this.statusBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.statusBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] 
            {
                this.leftSideTextLabel,
                this.leftSideTextLabel_2,
                this.exceptionWordsLabel,
                this.parallelismLabel,
                this.settingsLabel
            });
            this.statusBar.Location = new System.Drawing.Point(0, 0);
            this.statusBar.ShowItemToolTips = true;
            this.statusBar.Size = new System.Drawing.Size(227, 25);
            this.statusBar.SizingGrip = false;
            this.statusBar.TabIndex = 5;
            // 
            // leftSideTextLabel
            // 
            this.leftSideTextLabel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.leftSideTextLabel.Size = new System.Drawing.Size(2, 20);
            this.leftSideTextLabel.Spring = true;
            this.leftSideTextLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // leftSideTextLabel_2
            // 
            this.leftSideTextLabel_2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.leftSideTextLabel_2.Size = new System.Drawing.Size(2, 20);
            this.leftSideTextLabel_2.Spring = true;
            this.leftSideTextLabel_2.AutoToolTip = true;
            this.leftSideTextLabel_2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // exceptionWordsLabel
            // 
            this.exceptionWordsLabel.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            this.exceptionWordsLabel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.exceptionWordsLabel.Size = new System.Drawing.Size(174, 20);
            this.exceptionWordsLabel.ForeColor = System.Drawing.Color.DimGray;
            this.exceptionWordsLabel.Text = "file name exceptions";
            this.exceptionWordsLabel.ToolTipText = "file name exception word editor";
            this.exceptionWordsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.exceptionWordsLabel.Click += new System.EventHandler(this.exceptionWordsLabel_Click);
            this.exceptionWordsLabel.MouseEnter += new System.EventHandler(this.statusBarLabel_MouseEnter);
            this.exceptionWordsLabel.MouseLeave += new System.EventHandler(this.statusBarLabel_MouseLeave);
            // 
            // parallelismLabel
            // 
            this.parallelismLabel.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            this.parallelismLabel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.parallelismLabel.Size = new System.Drawing.Size(16, 20);
            this.parallelismLabel.Text = "?";
            this.parallelismLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.parallelismLabel.Click += new System.EventHandler(this.parallelismLabel_Click);
            this.parallelismLabel.EnabledChanged += new System.EventHandler(this.parallelismLabel_EnabledChanged);
            this.parallelismLabel.MouseEnter += new System.EventHandler(this.statusBarLabel_MouseEnter);
            this.parallelismLabel.MouseLeave += new System.EventHandler(this.statusBarLabel_MouseLeave);
            // 
            // settingsLabel
            // 
            this.settingsLabel.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right)));
            this.settingsLabel.Image = m3u8.download.manager.Properties.Resources.settings.ToBitmap();
            this.settingsLabel.Size = new System.Drawing.Size(20, 20);
            this.settingsLabel.ToolTipText = "settings";
            this.settingsLabel.Click += new System.EventHandler(this.settingsLabel_Click);
            this.settingsLabel.MouseLeave += new System.EventHandler(this.statusBarLabel_MouseLeave);
            this.settingsLabel.MouseHover += new System.EventHandler(this.statusBarLabel_MouseEnter);
            // 
            // StatusBarUC
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.statusBar);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Size = new System.Drawing.Size(227, 25);
            this.statusBar.ResumeLayout(false);
            this.statusBar.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }        
        #endregion

        private System.Windows.Forms.StatusStrip statusBar;
        private System.Windows.Forms.ToolStripStatusLabel leftSideTextLabel;
        private ToolStripStatusLabelEx leftSideTextLabel_2;
        private System.Windows.Forms.ToolStripStatusLabel exceptionWordsLabel;
        private System.Windows.Forms.ToolStripStatusLabel parallelismLabel;
        private System.Windows.Forms.ToolStripStatusLabel settingsLabel;
    }
}
