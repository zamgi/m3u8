namespace m3u8.downloader
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose( bool disposing )
        {
            if ( disposing && (components != null) )
            {
                components.Dispose();
            }
            base.Dispose( disposing );
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.l1 = new System.Windows.Forms.Label();
            this.topPanel = new System.Windows.Forms.Panel();
            this.m3u8FileUrlTextBox = new System.Windows.Forms.TextBox();
            this.m3u8FileResultTextBox = new System.Windows.Forms.TextBox();
            this.outputFileNameTextBox = new System.Windows.Forms.TextBox();
            this.l2 = new System.Windows.Forms.Label();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.outputFileNameClearButton = new System.Windows.Forms.ButtonWithFocusCues();
            this.statusBar = new System.Windows.Forms.StatusStrip();
            this.endStepActionLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.autoCloseApplicationWhenEndsDownloadLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.autoMinimizeWindowWhenStartsDownloadLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.excludesWordsLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.maxDegreeOfParallelismLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.m3u8FileWholeLoadAndSaveButton = new System.Windows.Forms.ButtonWithFocusCues();
            this.m3u8FileTextContentLoadButton = new System.Windows.Forms.ButtonWithFocusCues();
            this.mainLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.topPanel.SuspendLayout();
            this.statusBar.SuspendLayout();
            this.mainLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // l1
            // 
            this.l1.AutoSize = true;
            this.l1.ForeColor = System.Drawing.Color.SteelBlue;
            this.l1.Location = new System.Drawing.Point(3, 9);
            this.l1.Name = "l1";
            this.l1.Size = new System.Drawing.Size(69, 13);
            this.l1.TabIndex = 1;
            this.l1.Text = ".m3u8 file url:";
            // 
            // topPanel
            // 
            this.topPanel.Controls.Add(this.m3u8FileUrlTextBox);
            this.topPanel.Controls.Add(this.l1);
            this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.topPanel.Location = new System.Drawing.Point(0, 0);
            this.topPanel.Name = "topPanel";
            this.topPanel.Size = new System.Drawing.Size(1015, 81);
            this.topPanel.TabIndex = 0;
            // 
            // m3u8FileUrlTextBox
            // 
            this.m3u8FileUrlTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m3u8FileUrlTextBox.Location = new System.Drawing.Point(6, 26);
            this.m3u8FileUrlTextBox.Multiline = true;
            this.m3u8FileUrlTextBox.Name = "m3u8FileUrlTextBox";
            this.m3u8FileUrlTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.m3u8FileUrlTextBox.Size = new System.Drawing.Size(1006, 52);
            this.m3u8FileUrlTextBox.TabIndex = 0;
            this.m3u8FileUrlTextBox.TextChanged += new System.EventHandler(this.m3u8FileUrlTextBox_TextChanged);
            // 
            // m3u8FileResultTextBox
            // 
            this.m3u8FileResultTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.mainLayoutPanel.SetColumnSpan(this.m3u8FileResultTextBox, 5);
            this.m3u8FileResultTextBox.Location = new System.Drawing.Point(3, 33);
            this.m3u8FileResultTextBox.Multiline = true;
            this.m3u8FileResultTextBox.Name = "m3u8FileResultTextBox";
            this.m3u8FileResultTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.m3u8FileResultTextBox.Size = new System.Drawing.Size(1009, 440);
            this.m3u8FileResultTextBox.TabIndex = 5;
            // 
            // outputFileNameTextBox
            // 
            this.outputFileNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.outputFileNameTextBox.Location = new System.Drawing.Point(424, 5);
            this.outputFileNameTextBox.Name = "outputFileNameTextBox";
            this.outputFileNameTextBox.Size = new System.Drawing.Size(563, 20);
            this.outputFileNameTextBox.TabIndex = 3;
            // 
            // l2
            // 
            this.l2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.l2.AutoSize = true;
            this.l2.ForeColor = System.Drawing.Color.SteelBlue;
            this.l2.Location = new System.Drawing.Point(363, 2);
            this.l2.Name = "l2";
            this.l2.Size = new System.Drawing.Size(55, 26);
            this.l2.TabIndex = 2;
            this.l2.Text = "output \r\nfile name :";
            this.l2.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // outputFileNameClearButton
            // 
            this.outputFileNameClearButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.outputFileNameClearButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.outputFileNameClearButton.Location = new System.Drawing.Point(993, 5);
            this.outputFileNameClearButton.Name = "outputFileNameClearButton";
            this.outputFileNameClearButton.Size = new System.Drawing.Size(19, 20);
            this.outputFileNameClearButton.TabIndex = 4;
            this.outputFileNameClearButton.Text = "X";
            this.toolTip.SetToolTip(this.outputFileNameClearButton, "clear \'output file name\'");
            this.outputFileNameClearButton.UseVisualStyleBackColor = true;
            this.outputFileNameClearButton.Click += new System.EventHandler(this.outputFileNameClearButton_Click);
            // 
            // statusBar
            // 
            this.statusBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.endStepActionLabel,
            this.autoCloseApplicationWhenEndsDownloadLabel,
            this.autoMinimizeWindowWhenStartsDownloadLabel,
            this.excludesWordsLabel,
            this.maxDegreeOfParallelismLabel});
            this.statusBar.Location = new System.Drawing.Point(0, 557);
            this.statusBar.Name = "statusBar";
            this.statusBar.ShowItemToolTips = true;
            this.statusBar.Size = new System.Drawing.Size(1015, 22);
            this.statusBar.TabIndex = 2;
            // 
            // endStepActionLabel
            // 
            this.endStepActionLabel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.endStepActionLabel.Name = "endStepActionLabel";
            this.endStepActionLabel.Size = new System.Drawing.Size(675, 17);
            this.endStepActionLabel.Spring = true;
            this.endStepActionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // autoCloseApplicationWhenEndsDownloadLabel
            // 
            this.autoCloseApplicationWhenEndsDownloadLabel.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            this.autoCloseApplicationWhenEndsDownloadLabel.Name = "autoCloseApplicationWhenEndsDownloadLabel";
            this.autoCloseApplicationWhenEndsDownloadLabel.Size = new System.Drawing.Size(60, 17);
            this.autoCloseApplicationWhenEndsDownloadLabel.Text = "auto close";
            this.autoCloseApplicationWhenEndsDownloadLabel.ToolTipText = "auto close application when ends download";
            this.autoCloseApplicationWhenEndsDownloadLabel.Click += new System.EventHandler(this.autoCloseApplicationWhenEndsDownloadLabel_Click);
            this.autoCloseApplicationWhenEndsDownloadLabel.MouseLeave += new System.EventHandler(this.statusBarLabel_MouseLeave);
            this.autoCloseApplicationWhenEndsDownloadLabel.MouseHover += new System.EventHandler(this.statusBarLabel_MouseHover);
            // 
            // autoMinimizeWindowWhenStartsDownloadLabel
            // 
            this.autoMinimizeWindowWhenStartsDownloadLabel.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            this.autoMinimizeWindowWhenStartsDownloadLabel.Name = "autoMinimizeWindowWhenStartsDownloadLabel";
            this.autoMinimizeWindowWhenStartsDownloadLabel.Size = new System.Drawing.Size(75, 17);
            this.autoMinimizeWindowWhenStartsDownloadLabel.Text = "auto minimize";
            this.autoMinimizeWindowWhenStartsDownloadLabel.ToolTipText = "auto minimize window when starts download";
            this.autoMinimizeWindowWhenStartsDownloadLabel.Click += new System.EventHandler(this.autoMinimizeWindowWhenStartsDownloadLabel_Click);
            this.autoMinimizeWindowWhenStartsDownloadLabel.MouseLeave += new System.EventHandler(this.statusBarLabel_MouseLeave);
            this.autoMinimizeWindowWhenStartsDownloadLabel.MouseHover += new System.EventHandler(this.statusBarLabel_MouseHover);
            // 
            // excludesWordsLabel
            // 
            this.excludesWordsLabel.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            this.excludesWordsLabel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.excludesWordsLabel.Name = "excludesWordsLabel";
            this.excludesWordsLabel.Size = new System.Drawing.Size(174, 17);
            this.excludesWordsLabel.Text = "file name excludes words editor...";
            this.excludesWordsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.excludesWordsLabel.Click += new System.EventHandler(this.excludesWordsLabel_Click);
            this.excludesWordsLabel.MouseLeave += new System.EventHandler(this.statusBarLabel_MouseLeave);
            this.excludesWordsLabel.MouseHover += new System.EventHandler(this.statusBarLabel_MouseHover);
            // 
            // maxDegreeOfParallelismLabel
            // 
            this.maxDegreeOfParallelismLabel.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            this.maxDegreeOfParallelismLabel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.maxDegreeOfParallelismLabel.Name = "maxDegreeOfParallelismLabel";
            this.maxDegreeOfParallelismLabel.Size = new System.Drawing.Size(16, 17);
            this.maxDegreeOfParallelismLabel.Text = "?";
            this.maxDegreeOfParallelismLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.maxDegreeOfParallelismLabel.Click += new System.EventHandler(this.maxDegreeOfParallelismLabel_Click);
            this.maxDegreeOfParallelismLabel.MouseLeave += new System.EventHandler(this.statusBarLabel_MouseLeave);
            this.maxDegreeOfParallelismLabel.MouseHover += new System.EventHandler(this.statusBarLabel_MouseHover);
            // 
            // m3u8FileWholeLoadAndSaveButton
            // 
            this.m3u8FileWholeLoadAndSaveButton.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.m3u8FileWholeLoadAndSaveButton.AutoSize = true;
            this.m3u8FileWholeLoadAndSaveButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.m3u8FileWholeLoadAndSaveButton.Location = new System.Drawing.Point(183, 3);
            this.m3u8FileWholeLoadAndSaveButton.Name = "m3u8FileWholeLoadAndSaveButton";
            this.m3u8FileWholeLoadAndSaveButton.Size = new System.Drawing.Size(165, 23);
            this.m3u8FileWholeLoadAndSaveButton.TabIndex = 1;
            this.m3u8FileWholeLoadAndSaveButton.Text = "load && save whole .m3u8 file ...";
            this.m3u8FileWholeLoadAndSaveButton.UseVisualStyleBackColor = true;
            this.m3u8FileWholeLoadAndSaveButton.Click += new System.EventHandler(this.m3u8FileWholeLoadAndSaveButton_Click);
            // 
            // m3u8FileTextContentLoadButton
            // 
            this.m3u8FileTextContentLoadButton.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.m3u8FileTextContentLoadButton.AutoSize = true;
            this.m3u8FileTextContentLoadButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.m3u8FileTextContentLoadButton.Location = new System.Drawing.Point(3, 3);
            this.m3u8FileTextContentLoadButton.Name = "m3u8FileTextContentLoadButton";
            this.m3u8FileTextContentLoadButton.Size = new System.Drawing.Size(153, 23);
            this.m3u8FileTextContentLoadButton.TabIndex = 0;
            this.m3u8FileTextContentLoadButton.Text = "load .m3u8 file text-content...";
            this.m3u8FileTextContentLoadButton.UseVisualStyleBackColor = true;
            this.m3u8FileTextContentLoadButton.Click += new System.EventHandler(this.m3u8FileTextContentLoadButton_Click);
            // 
            // mainLayoutPanel
            // 
            this.mainLayoutPanel.ColumnCount = 5;
            this.mainLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 180F));
            this.mainLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 180F));
            this.mainLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.mainLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.mainLayoutPanel.Controls.Add(this.outputFileNameTextBox, 3, 0);
            this.mainLayoutPanel.Controls.Add(this.l2, 2, 0);
            this.mainLayoutPanel.Controls.Add(this.outputFileNameClearButton, 4, 0);
            this.mainLayoutPanel.Controls.Add(this.m3u8FileResultTextBox, 0, 1);
            this.mainLayoutPanel.Controls.Add(this.m3u8FileWholeLoadAndSaveButton, 1, 0);
            this.mainLayoutPanel.Controls.Add(this.m3u8FileTextContentLoadButton, 0, 0);
            this.mainLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainLayoutPanel.Location = new System.Drawing.Point(0, 81);
            this.mainLayoutPanel.Name = "mainLayoutPanel";
            this.mainLayoutPanel.RowCount = 2;
            this.mainLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.mainLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.mainLayoutPanel.Size = new System.Drawing.Size(1015, 476);
            this.mainLayoutPanel.TabIndex = 1;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1015, 579);
            this.Controls.Add(this.mainLayoutPanel);
            this.Controls.Add(this.statusBar);
            this.Controls.Add(this.topPanel);
            this.Icon = global::m3u8.Properties.Resources.m3u8_32x36;
            this.KeyPreview = true;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = ".m3u8 file downloader";
            this.topPanel.ResumeLayout(false);
            this.topPanel.PerformLayout();
            this.statusBar.ResumeLayout(false);
            this.statusBar.PerformLayout();
            this.mainLayoutPanel.ResumeLayout(false);
            this.mainLayoutPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        #endregion

        private System.Windows.Forms.Label l1;
        private System.Windows.Forms.Panel topPanel;
        private System.Windows.Forms.TextBox m3u8FileUrlTextBox;
        private System.Windows.Forms.ButtonWithFocusCues m3u8FileTextContentLoadButton;
        private System.Windows.Forms.TextBox m3u8FileResultTextBox;
        private System.Windows.Forms.ButtonWithFocusCues m3u8FileWholeLoadAndSaveButton;
        private System.Windows.Forms.TextBox outputFileNameTextBox;
        private System.Windows.Forms.Label l2;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.ButtonWithFocusCues outputFileNameClearButton;
        private System.Windows.Forms.StatusStrip statusBar;
        private System.Windows.Forms.ToolStripStatusLabel maxDegreeOfParallelismLabel;
        private System.Windows.Forms.ToolStripStatusLabel endStepActionLabel;
        private System.Windows.Forms.ToolStripStatusLabel excludesWordsLabel;
        private System.Windows.Forms.ToolStripStatusLabel autoMinimizeWindowWhenStartsDownloadLabel;
        private System.Windows.Forms.ToolStripStatusLabel autoCloseApplicationWhenEndsDownloadLabel;
        private System.Windows.Forms.TableLayoutPanel mainLayoutPanel;
    }
}
