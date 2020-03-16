namespace m3u8.download.manager.ui
{
    partial class AddNewDownloadForm
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
            this.l1 = new System.Windows.Forms.Label();
            this.l2 = new System.Windows.Forms.Label();
            this.l3 = new System.Windows.Forms.Label();
            this.topPanel = new System.Windows.Forms.Panel();
            this.m3u8FileUrlTextBox = new System.Windows.Forms.TextBox();
            this.outputFileNameTextBox = new System.Windows.Forms.TextBox();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.outputDirectorySelectButton = new System.Windows.Forms.ButtonWithFocusCues();
            this.outputFileNameSelectButton = new System.Windows.Forms.ButtonWithFocusCues();
            this.outputFileNameClearButton = new System.Windows.Forms.ButtonWithFocusCues();
            this.logPanel = new System.Windows.Forms.Panel();
            this.logUC = new m3u8.download.manager.ui.LogUC();
            this.mainLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.outputDirectoryTextBox = new System.Windows.Forms.TextBox();
            this.loadM3u8FileContentButton = new System.Windows.Forms.ButtonWithFocusCues();
            this.buttomPanel = new System.Windows.Forms.Panel();
            this.downloadStartButton = new System.Windows.Forms.ButtonWithFocusCues();
            this.downloadLaterButton = new System.Windows.Forms.ButtonWithFocusCues();
            this.statusBarUC = new m3u8.download.manager.ui.StatusBarUC();
            this.topPanel.SuspendLayout();
            this.logPanel.SuspendLayout();
            this.mainLayoutPanel.SuspendLayout();
            this.buttomPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // l1
            // 
            this.l1.AutoSize = true;
            this.l1.ForeColor = System.Drawing.Color.SteelBlue;
            this.l1.Location = new System.Drawing.Point(3, 3);
            this.l1.Name = "l1";
            this.l1.Size = new System.Drawing.Size(69, 13);
            this.l1.TabIndex = 0;
            this.l1.Text = ".m3u8 file url:";
            // 
            // l2
            // 
            this.l2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.l2.AutoSize = true;
            this.l2.ForeColor = System.Drawing.Color.SteelBlue;
            this.l2.Location = new System.Drawing.Point(6, 2);
            this.l2.Name = "l2";
            this.l2.Size = new System.Drawing.Size(55, 26);
            this.l2.TabIndex = 0;
            this.l2.Text = "output \r\nfile name :";
            this.l2.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // l3
            // 
            this.l3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.l3.AutoSize = true;
            this.l3.ForeColor = System.Drawing.Color.SteelBlue;
            this.l3.Location = new System.Drawing.Point(6, 32);
            this.l3.Name = "l3";
            this.l3.Size = new System.Drawing.Size(55, 26);
            this.l3.TabIndex = 4;
            this.l3.Text = "output \r\ndirectory :";
            this.l3.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // topPanel
            // 
            this.topPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.topPanel.Controls.Add(this.m3u8FileUrlTextBox);
            this.topPanel.Controls.Add(this.l1);
            this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.topPanel.Location = new System.Drawing.Point(0, 0);
            this.topPanel.Name = "topPanel";
            this.topPanel.Size = new System.Drawing.Size(803, 81);
            this.topPanel.TabIndex = 0;
            // 
            // m3u8FileUrlTextBox
            // 
            this.m3u8FileUrlTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m3u8FileUrlTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.m3u8FileUrlTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.5F);
            this.m3u8FileUrlTextBox.Location = new System.Drawing.Point(6, 20);
            this.m3u8FileUrlTextBox.Multiline = true;
            this.m3u8FileUrlTextBox.Name = "m3u8FileUrlTextBox";
            this.m3u8FileUrlTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.m3u8FileUrlTextBox.Size = new System.Drawing.Size(790, 54);
            this.m3u8FileUrlTextBox.TabIndex = 1;
            this.m3u8FileUrlTextBox.TextChanged += new System.EventHandler(this.m3u8FileUrlTextBox_TextChanged);
            // 
            // outputFileNameTextBox
            // 
            this.outputFileNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.outputFileNameTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.outputFileNameTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.5F);
            this.outputFileNameTextBox.Location = new System.Drawing.Point(67, 6);
            this.outputFileNameTextBox.Name = "outputFileNameTextBox";
            this.outputFileNameTextBox.Size = new System.Drawing.Size(539, 18);
            this.outputFileNameTextBox.TabIndex = 1;
            this.outputFileNameTextBox.WordWrap = false;
            // 
            // outputDirectorySelectButton
            // 
            this.outputDirectorySelectButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.outputDirectorySelectButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.outputDirectorySelectButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.outputDirectorySelectButton.Location = new System.Drawing.Point(641, 33);
            this.outputDirectorySelectButton.Name = "outputDirectorySelectButton";
            this.outputDirectorySelectButton.Size = new System.Drawing.Size(23, 23);
            this.outputDirectorySelectButton.TabIndex = 6;
            this.outputDirectorySelectButton.Text = "≡";
            this.outputDirectorySelectButton.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.toolTip.SetToolTip(this.outputDirectorySelectButton, "select \'output directory\'");
            this.outputDirectorySelectButton.UseCompatibleTextRendering = true;
            this.outputDirectorySelectButton.UseVisualStyleBackColor = true;
            this.outputDirectorySelectButton.Click += new System.EventHandler(this.outputDirectorySelectButton_Click);
            // 
            // outputFileNameSelectButton
            // 
            this.outputFileNameSelectButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.outputFileNameSelectButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.outputFileNameSelectButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.outputFileNameSelectButton.Location = new System.Drawing.Point(641, 3);
            this.outputFileNameSelectButton.Name = "outputFileNameSelectButton";
            this.outputFileNameSelectButton.Size = new System.Drawing.Size(23, 23);
            this.outputFileNameSelectButton.TabIndex = 3;
            this.outputFileNameSelectButton.Text = "≡";
            this.outputFileNameSelectButton.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.toolTip.SetToolTip(this.outputFileNameSelectButton, "select \'output file name\'");
            this.outputFileNameSelectButton.UseCompatibleTextRendering = true;
            this.outputFileNameSelectButton.UseVisualStyleBackColor = true;
            this.outputFileNameSelectButton.Click += new System.EventHandler(this.outputFileNameSelectButton_Click);
            // 
            // outputFileNameClearButton
            // 
            this.outputFileNameClearButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.outputFileNameClearButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.outputFileNameClearButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.outputFileNameClearButton.Location = new System.Drawing.Point(612, 3);
            this.outputFileNameClearButton.Name = "outputFileNameClearButton";
            this.outputFileNameClearButton.Size = new System.Drawing.Size(23, 23);
            this.outputFileNameClearButton.TabIndex = 2;
            this.outputFileNameClearButton.Text = "X";
            this.outputFileNameClearButton.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.toolTip.SetToolTip(this.outputFileNameClearButton, "clear \'output file name\'");
            this.outputFileNameClearButton.UseCompatibleTextRendering = true;
            this.outputFileNameClearButton.UseVisualStyleBackColor = true;
            this.outputFileNameClearButton.Click += new System.EventHandler(this.outputFileNameClearButton_Click);
            // 
            // logPanel
            // 
            this.logPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.logPanel.Controls.Add(this.logUC);
            this.logPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.logPanel.Location = new System.Drawing.Point(0, 141);
            this.logPanel.Name = "logPanel";
            //---this.logPanel.Padding = new System.Windows.Forms.Padding(6, 0, 3, 0);
            this.logPanel.Size = new System.Drawing.Size(803, 93);
            this.logPanel.TabIndex = 2;
            // 
            // logUC
            // 
            this.logUC.Dock = System.Windows.Forms.DockStyle.Fill;
            this.logUC.Location = new System.Drawing.Point(6, 0);
            this.logUC.Name = "logUC";
            this.logUC.Size = new System.Drawing.Size(790, 89);
            this.logUC.TabIndex = 0;
            // 
            // mainLayoutPanel
            // 
            this.mainLayoutPanel.ColumnCount = 5;
            this.mainLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.mainLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            this.mainLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            this.mainLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.mainLayoutPanel.Controls.Add(this.outputDirectoryTextBox, 1, 1);
            this.mainLayoutPanel.Controls.Add(this.outputDirectorySelectButton, 3, 1);
            this.mainLayoutPanel.Controls.Add(this.l3, 0, 1);
            this.mainLayoutPanel.Controls.Add(this.outputFileNameSelectButton, 3, 0);
            this.mainLayoutPanel.Controls.Add(this.outputFileNameTextBox, 1, 0);
            this.mainLayoutPanel.Controls.Add(this.l2, 0, 0);
            this.mainLayoutPanel.Controls.Add(this.outputFileNameClearButton, 2, 0);
            this.mainLayoutPanel.Controls.Add(this.loadM3u8FileContentButton, 4, 1);
            this.mainLayoutPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.mainLayoutPanel.Location = new System.Drawing.Point(0, 81);
            this.mainLayoutPanel.Name = "mainLayoutPanel";
            this.mainLayoutPanel.Padding = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.mainLayoutPanel.RowCount = 2;
            this.mainLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.mainLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.mainLayoutPanel.Size = new System.Drawing.Size(803, 60);
            this.mainLayoutPanel.TabIndex = 1;
            // 
            // outputDirectoryTextBox
            // 
            this.outputDirectoryTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.outputDirectoryTextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(235)))), ((int)(((byte)(235)))));
            this.outputDirectoryTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.outputDirectoryTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.5F);
            this.outputDirectoryTextBox.Location = new System.Drawing.Point(67, 36);
            this.outputDirectoryTextBox.Name = "outputDirectoryTextBox";
            this.outputDirectoryTextBox.Size = new System.Drawing.Size(539, 18);
            this.outputDirectoryTextBox.TabIndex = 5;
            this.outputDirectoryTextBox.WordWrap = false;
            // 
            // loadM3u8FileContentButton
            // 
            this.loadM3u8FileContentButton.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.loadM3u8FileContentButton.AutoSize = true;
            this.loadM3u8FileContentButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.loadM3u8FileContentButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.loadM3u8FileContentButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(70)))), ((int)(((byte)(70)))));
            this.loadM3u8FileContentButton.Location = new System.Drawing.Point(670, 33);
            this.loadM3u8FileContentButton.Name = "m3u8FileTextContentLoadButton";
            this.loadM3u8FileContentButton.Size = new System.Drawing.Size(130, 23);
            this.loadM3u8FileContentButton.TabIndex = 7;
            this.loadM3u8FileContentButton.Text = "(load .m3u8 file-content)";
            this.loadM3u8FileContentButton.UseVisualStyleBackColor = true;
            this.loadM3u8FileContentButton.Click += new System.EventHandler(this.loadM3u8FileContentButton_Click);
            // 
            // buttomPanel
            // 
            this.buttomPanel.BackColor = System.Drawing.Color.White;
            this.buttomPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.buttomPanel.Controls.Add(this.downloadStartButton);
            this.buttomPanel.Controls.Add(this.downloadLaterButton);
            this.buttomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.buttomPanel.Location = new System.Drawing.Point(0, 234);
            this.buttomPanel.Name = "buttomPanel";
            this.buttomPanel.Size = new System.Drawing.Size(803, 36);
            this.buttomPanel.TabIndex = 3;
            // 
            // downloadStartButton
            // 
            this.downloadStartButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.downloadStartButton.AutoSize = true;
            this.downloadStartButton.BackColor = System.Drawing.SystemColors.Control;
            this.downloadStartButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.downloadStartButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.downloadStartButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.downloadStartButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.downloadStartButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(70)))), ((int)(((byte)(70)))));
            this.downloadStartButton.Location = new System.Drawing.Point(233, 5);
            this.downloadStartButton.Name = "downloadStartButton";
            this.downloadStartButton.Size = new System.Drawing.Size(159, 23);
            this.downloadStartButton.TabIndex = 0;
            this.downloadStartButton.Text = "Start Download";
            this.downloadStartButton.UseVisualStyleBackColor = false;
            // 
            // downloadLaterButton
            // 
            this.downloadLaterButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.downloadLaterButton.AutoSize = true;
            this.downloadLaterButton.BackColor = System.Drawing.SystemColors.Control;
            this.downloadLaterButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.downloadLaterButton.DialogResult = System.Windows.Forms.DialogResult.Yes;
            this.downloadLaterButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.downloadLaterButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(70)))), ((int)(((byte)(70)))));
            this.downloadLaterButton.Location = new System.Drawing.Point(413, 5);
            this.downloadLaterButton.Name = "downloadLaterButton";
            this.downloadLaterButton.Size = new System.Drawing.Size(159, 23);
            this.downloadLaterButton.TabIndex = 1;
            this.downloadLaterButton.Text = "Download later";
            this.downloadLaterButton.UseVisualStyleBackColor = false;
            // 
            // statusBarUC
            // 
            this.statusBarUC.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.statusBarUC.Location = new System.Drawing.Point(0, 270);
            this.statusBarUC.Name = "statusBarUC";
            this.statusBarUC.Size = new System.Drawing.Size(803, 35);
            this.statusBarUC.TabIndex = 4;
            // 
            // AddNewDownloadForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(803, 225);
            this.Controls.Add(this.logPanel);
            this.Controls.Add(this.buttomPanel);
            this.Controls.Add(this.mainLayoutPanel);
            this.Controls.Add(this.statusBarUC);
            this.Controls.Add(this.topPanel);
            this.Icon = global::m3u8.download.manager.Properties.Resources.m3u8_32x36;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AddNewDownloadForm";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "add new download";
            this.topPanel.ResumeLayout(false);
            this.topPanel.PerformLayout();
            this.logPanel.ResumeLayout(false);
            this.mainLayoutPanel.ResumeLayout(false);
            this.mainLayoutPanel.PerformLayout();
            this.buttomPanel.ResumeLayout(false);
            this.buttomPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        #endregion

        private System.Windows.Forms.Panel topPanel;
        private System.Windows.Forms.TextBox m3u8FileUrlTextBox;
        private System.Windows.Forms.TextBox outputFileNameTextBox;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.ButtonWithFocusCues outputFileNameClearButton;
        private System.Windows.Forms.ButtonWithFocusCues loadM3u8FileContentButton;
        private System.Windows.Forms.Panel logPanel;
        private System.Windows.Forms.TableLayoutPanel mainLayoutPanel;
        private System.Windows.Forms.ButtonWithFocusCues outputFileNameSelectButton;
        private System.Windows.Forms.Panel buttomPanel;
        private System.Windows.Forms.ButtonWithFocusCues downloadStartButton;
        private System.Windows.Forms.ButtonWithFocusCues downloadLaterButton;
        private System.Windows.Forms.ButtonWithFocusCues outputDirectorySelectButton;
        private System.Windows.Forms.TextBox outputDirectoryTextBox;
        private LogUC logUC;
        private StatusBarUC statusBarUC;
        private System.Windows.Forms.Label l1;
        private System.Windows.Forms.Label l2;
        private System.Windows.Forms.Label l3;
    }
}