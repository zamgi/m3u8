namespace m3u8.download.manager.ui
{
    partial class SettingsForm
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
            System.Windows.Forms.GroupBox gb1;
            System.Windows.Forms.GroupBox gb2;
            System.Windows.Forms.Label l4;
            System.Windows.Forms.Label l3;
            this.showOnlyRequestRowsWithErrorsCheckBox = new System.Windows.Forms.CheckBox();
            this.showDownloadStatisticsInMainFormTitleCheckBox = new System.Windows.Forms.CheckBox();
            this.externalProgFilePathButton = new System.Windows.Forms.Button();
            this.externalProgFilePathTextBox = new System.Windows.Forms.TextBox();
            this.externalProgCaptionTextBox = new System.Windows.Forms.TextBox();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.l1 = new System.Windows.Forms.Label();
            this.l2 = new System.Windows.Forms.Label();
            this.requestTimeoutByPartDTP = new System.Windows.Forms.DateTimePicker();
            this.uniqueUrlsOnlyCheckBox = new System.Windows.Forms.CheckBox();
            this.only4NotRunLabel1 = new System.Windows.Forms.Label();
            this.only4NotRunLabel2 = new System.Windows.Forms.Label();
            this.outputFileExtensionLabel = new System.Windows.Forms.Label();
            this.outputFileExtensionTextBox = new System.Windows.Forms.TextBox();
            this.attemptRequestCountByPartNUD = new System.Windows.Forms.NumericUpDownEx();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.externalProgResetButton = new System.Windows.Forms.Button();
            gb1 = new System.Windows.Forms.GroupBox();
            gb2 = new System.Windows.Forms.GroupBox();
            l4 = new System.Windows.Forms.Label();
            l3 = new System.Windows.Forms.Label();
            gb1.SuspendLayout();
            gb2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.attemptRequestCountByPartNUD)).BeginInit();
            this.SuspendLayout();
            // 
            // gb1
            // 
            gb1.Controls.Add(this.showOnlyRequestRowsWithErrorsCheckBox);
            gb1.Controls.Add(this.showDownloadStatisticsInMainFormTitleCheckBox);
            gb1.Location = new System.Drawing.Point(13, 155);
            gb1.Size = new System.Drawing.Size(261, 110);
            gb1.TabIndex = 7;
            gb1.TabStop = false;
            gb1.Text = "UI / download log UI";
            // 
            // showOnlyRequestRowsWithErrorsCheckBox
            // 
            this.showOnlyRequestRowsWithErrorsCheckBox.AutoSize = true;
            this.showOnlyRequestRowsWithErrorsCheckBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.showOnlyRequestRowsWithErrorsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.showOnlyRequestRowsWithErrorsCheckBox.Location = new System.Drawing.Point(30, 30);
            this.showOnlyRequestRowsWithErrorsCheckBox.Size = new System.Drawing.Size(185, 17);
            this.showOnlyRequestRowsWithErrorsCheckBox.TabIndex = 0;
            this.showOnlyRequestRowsWithErrorsCheckBox.Text = "show only request rows with errors";
            this.showOnlyRequestRowsWithErrorsCheckBox.UseVisualStyleBackColor = true;
            // 
            // showDownloadStatisticsInMainFormTitleCheckBox
            // 
            this.showDownloadStatisticsInMainFormTitleCheckBox.AutoSize = true;
            this.showDownloadStatisticsInMainFormTitleCheckBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.showDownloadStatisticsInMainFormTitleCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.showDownloadStatisticsInMainFormTitleCheckBox.Location = new System.Drawing.Point(30, 60);
            this.showDownloadStatisticsInMainFormTitleCheckBox.Size = new System.Drawing.Size(177, 30);
            this.showDownloadStatisticsInMainFormTitleCheckBox.TabIndex = 1;
            this.showDownloadStatisticsInMainFormTitleCheckBox.Text = "show download statistics in main\r\nwindow title";
            this.showDownloadStatisticsInMainFormTitleCheckBox.UseVisualStyleBackColor = true;
            // 
            // gb2
            // 
            gb2.Controls.Add(this.externalProgResetButton);
            gb2.Controls.Add(this.externalProgFilePathButton);
            gb2.Controls.Add(this.externalProgFilePathTextBox);
            gb2.Controls.Add(this.externalProgCaptionTextBox);
            gb2.Controls.Add(l4);
            gb2.Controls.Add(l3);
            gb2.Location = new System.Drawing.Point(13, 271);
            gb2.Size = new System.Drawing.Size(261, 127);
            gb2.TabIndex = 8;
            gb2.TabStop = false;
            gb2.Text = "External program";
            // 
            // externalProgFilePathButton
            // 
            this.externalProgFilePathButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.externalProgFilePathButton.AutoEllipsis = true;
            this.externalProgFilePathButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.externalProgFilePathButton.Location = new System.Drawing.Point(246, 85);
            this.externalProgFilePathButton.Size = new System.Drawing.Size(13, 18);
            this.externalProgFilePathButton.TabIndex = 9;
            this.toolTip.SetToolTip(this.externalProgFilePathButton, "browse");
            this.externalProgFilePathButton.UseVisualStyleBackColor = true;
            this.externalProgFilePathButton.Click += new System.EventHandler(this.externalProgFilePathButton_Click);
            // 
            // externalProgFilePathTextBox
            // 
            this.externalProgFilePathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.externalProgFilePathTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.externalProgFilePathTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.5F);
            this.externalProgFilePathTextBox.Location = new System.Drawing.Point(6, 85);
            this.externalProgFilePathTextBox.Size = new System.Drawing.Size(239, 18);
            this.externalProgFilePathTextBox.TabIndex = 8;
            this.externalProgFilePathTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.externalProgFilePathTextBox.WordWrap = false;
            this.externalProgFilePathTextBox.TextChanged += new System.EventHandler(this.externalProgFilePathTextBox_TextChanged);
            // 
            // externalProgCaptionTextBox
            // 
            this.externalProgCaptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.externalProgCaptionTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.externalProgCaptionTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.5F);
            this.externalProgCaptionTextBox.Location = new System.Drawing.Point(19, 35);
            this.externalProgCaptionTextBox.Size = new System.Drawing.Size(226, 18);
            this.externalProgCaptionTextBox.TabIndex = 7;
            this.externalProgCaptionTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.externalProgCaptionTextBox.WordWrap = false;
            this.externalProgCaptionTextBox.TextChanged += new System.EventHandler(this.externalProgCaptionTextBox_TextChanged);
            // 
            // l4
            // 
            l4.AutoSize = true;
            l4.Location = new System.Drawing.Point(16, 69);
            l4.Size = new System.Drawing.Size(32, 13);
            l4.TabIndex = 1;
            l4.Text = "Path:";
            // 
            // l3
            // 
            l3.AutoSize = true;
            l3.Location = new System.Drawing.Point(16, 19);
            l3.Size = new System.Drawing.Size(38, 13);
            l3.TabIndex = 0;
            l3.Text = "Name:";
            // 
            // okButton
            // 
            this.okButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.okButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.okButton.Location = new System.Drawing.Point(65, 412);
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 8;
            this.okButton.Text = "Ok";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.cancelButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.cancelButton.Location = new System.Drawing.Point(146, 412);
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 9;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // l1
            // 
            this.l1.AutoSize = true;
            this.l1.Location = new System.Drawing.Point(12, 12);
            this.l1.Size = new System.Drawing.Size(148, 13);
            this.l1.TabIndex = 0;
            this.l1.Text = "attempt request count by part:";
            // 
            // l2
            // 
            this.l2.AutoSize = true;
            this.l2.Location = new System.Drawing.Point(43, 54);
            this.l2.Size = new System.Drawing.Size(117, 13);
            this.l2.TabIndex = 3;
            this.l2.Text = "request timeout by part:";
            // 
            // requestTimeoutByPartDTP
            // 
            this.requestTimeoutByPartDTP.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.requestTimeoutByPartDTP.Location = new System.Drawing.Point(167, 52);
            this.requestTimeoutByPartDTP.ShowUpDown = true;
            this.requestTimeoutByPartDTP.Size = new System.Drawing.Size(91, 20);
            this.requestTimeoutByPartDTP.TabIndex = 2;
            // 
            // uniqueUrlsOnlyCheckBox
            // 
            this.uniqueUrlsOnlyCheckBox.AutoSize = true;
            this.uniqueUrlsOnlyCheckBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.uniqueUrlsOnlyCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.uniqueUrlsOnlyCheckBox.Location = new System.Drawing.Point(43, 90);
            this.uniqueUrlsOnlyCheckBox.Size = new System.Drawing.Size(117, 17);
            this.uniqueUrlsOnlyCheckBox.TabIndex = 4;
            this.uniqueUrlsOnlyCheckBox.Text = "use unique urls only";
            this.uniqueUrlsOnlyCheckBox.UseVisualStyleBackColor = true;
            // 
            // only4NotRunLabel1
            // 
            this.only4NotRunLabel1.AutoSize = true;
            this.only4NotRunLabel1.ForeColor = System.Drawing.Color.DarkGoldenrod;
            this.only4NotRunLabel1.Location = new System.Drawing.Point(7, 29);
            this.only4NotRunLabel1.Size = new System.Drawing.Size(157, 13);
            this.only4NotRunLabel1.TabIndex = 10;
            this.only4NotRunLabel1.Text = "(only for not-running downloads)";
            this.only4NotRunLabel1.Visible = false;
            // 
            // only4NotRunLabel2
            // 
            this.only4NotRunLabel2.AutoSize = true;
            this.only4NotRunLabel2.ForeColor = System.Drawing.Color.DarkGoldenrod;
            this.only4NotRunLabel2.Location = new System.Drawing.Point(7, 71);
            this.only4NotRunLabel2.Size = new System.Drawing.Size(157, 13);
            this.only4NotRunLabel2.TabIndex = 11;
            this.only4NotRunLabel2.Text = "(only for not-running downloads)";
            this.only4NotRunLabel2.Visible = false;
            // 
            // outputFileExtensionLabel
            // 
            this.outputFileExtensionLabel.AutoSize = true;
            this.outputFileExtensionLabel.Location = new System.Drawing.Point(20, 120);
            this.outputFileExtensionLabel.Size = new System.Drawing.Size(139, 13);
            this.outputFileExtensionLabel.TabIndex = 5;
            this.outputFileExtensionLabel.Text = "default output file extension:";
            // 
            // outputFileExtensionTextBox
            // 
            this.outputFileExtensionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.outputFileExtensionTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.outputFileExtensionTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.5F);
            this.outputFileExtensionTextBox.Location = new System.Drawing.Point(167, 120);
            this.outputFileExtensionTextBox.Size = new System.Drawing.Size(89, 18);
            this.outputFileExtensionTextBox.TabIndex = 6;
            this.outputFileExtensionTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.outputFileExtensionTextBox.WordWrap = false;
            // 
            // attemptRequestCountByPartNUD
            // 
            this.attemptRequestCountByPartNUD.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.attemptRequestCountByPartNUD.Location = new System.Drawing.Point(167, 10);
            this.attemptRequestCountByPartNUD.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.attemptRequestCountByPartNUD.Size = new System.Drawing.Size(89, 16);
            this.attemptRequestCountByPartNUD.TabIndex = 1;
            this.attemptRequestCountByPartNUD.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.attemptRequestCountByPartNUD.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // externalProgResetButton
            // 
            this.externalProgResetButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.externalProgResetButton.AutoEllipsis = true;
            this.externalProgResetButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.externalProgResetButton.Location = new System.Drawing.Point(241, 0);
            this.externalProgResetButton.Size = new System.Drawing.Size(18, 22);
            this.externalProgResetButton.TabIndex = 10;
            this.externalProgResetButton.Text = "reset";
            this.externalProgResetButton.UseVisualStyleBackColor = true;
            this.externalProgResetButton.Click += new System.EventHandler(this.externalProgResetButton_Click);
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(286, 447);
            this.Controls.Add(gb2);
            this.Controls.Add(this.outputFileExtensionLabel);
            this.Controls.Add(this.outputFileExtensionTextBox);
            this.Controls.Add(this.only4NotRunLabel2);
            this.Controls.Add(this.only4NotRunLabel1);
            this.Controls.Add(gb1);
            this.Controls.Add(this.requestTimeoutByPartDTP);
            this.Controls.Add(this.l2);
            this.Controls.Add(this.l1);
            this.Controls.Add(this.uniqueUrlsOnlyCheckBox);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.attemptRequestCountByPartNUD);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "settings";
            gb1.ResumeLayout(false);
            gb1.PerformLayout();
            gb2.ResumeLayout(false);
            gb2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.attemptRequestCountByPartNUD)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        #endregion

        private System.Windows.Forms.Label l1;
        private System.Windows.Forms.Label l2;
        private System.Windows.Forms.NumericUpDownEx attemptRequestCountByPartNUD;
        private System.Windows.Forms.DateTimePicker requestTimeoutByPartDTP;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.CheckBox showOnlyRequestRowsWithErrorsCheckBox;
        private System.Windows.Forms.CheckBox showDownloadStatisticsInMainFormTitleCheckBox;
        private System.Windows.Forms.CheckBox uniqueUrlsOnlyCheckBox;
        private System.Windows.Forms.Label only4NotRunLabel1;
        private System.Windows.Forms.Label only4NotRunLabel2;
        private System.Windows.Forms.Label outputFileExtensionLabel;
        private System.Windows.Forms.TextBox outputFileExtensionTextBox;
        private System.Windows.Forms.TextBox externalProgFilePathTextBox;
        private System.Windows.Forms.TextBox externalProgCaptionTextBox;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.Button externalProgFilePathButton;
        private System.Windows.Forms.Button externalProgResetButton;
    }
}