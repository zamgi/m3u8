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
            this.attemptRequestCountByPartNUD = new System.Windows.Forms.NumericUpDown();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.l1 = new System.Windows.Forms.Label();
            this.l2 = new System.Windows.Forms.Label();
            this.requestTimeoutByPartDTP = new System.Windows.Forms.DateTimePicker();
            this.gb1 = new System.Windows.Forms.GroupBox();
            this.showOnlyRequestRowsWithErrorsCheckBox = new System.Windows.Forms.CheckBox();
            this.showDownloadStatisticsInMainFormTitleCheckBox = new System.Windows.Forms.CheckBox();
            this.uniqueUrlsOnlyCheckBox = new System.Windows.Forms.CheckBox();
            this.only4NotRunLabel1 = new System.Windows.Forms.Label();
            this.only4NotRunLabel2 = new System.Windows.Forms.Label();
            this.outputFileExtensionLabel = new System.Windows.Forms.Label();
            this.outputFileExtensionTextBox = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.attemptRequestCountByPartNUD)).BeginInit();
            this.gb1.SuspendLayout();
            this.SuspendLayout();
            // 
            // attemptRequestCountByPartNUD
            // 
            this.attemptRequestCountByPartNUD.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.attemptRequestCountByPartNUD.Location = new System.Drawing.Point(167, 10);
            this.attemptRequestCountByPartNUD.Minimum = new decimal( new int[] { 1, 0, 0, 0 } );
            this.attemptRequestCountByPartNUD.Size = new System.Drawing.Size(89, 16);
            this.attemptRequestCountByPartNUD.TabIndex = 1;
            this.attemptRequestCountByPartNUD.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.attemptRequestCountByPartNUD.Value = new decimal( new int[] { 1, 0, 0, 0 } );
            // 
            // okButton
            // 
            this.okButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.okButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.okButton.Location = new System.Drawing.Point(65, 275);
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
            this.cancelButton.Location = new System.Drawing.Point(146, 275);
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
            this.uniqueUrlsOnlyCheckBox.TabIndex = 4;
            this.uniqueUrlsOnlyCheckBox.Text = "use unique urls only";
            this.uniqueUrlsOnlyCheckBox.UseVisualStyleBackColor = true;
            // 
            // gb1
            // 
            this.gb1.Controls.Add(this.showOnlyRequestRowsWithErrorsCheckBox);
            this.gb1.Controls.Add(this.showDownloadStatisticsInMainFormTitleCheckBox);            
            this.gb1.Location = new System.Drawing.Point(13, 155);
            this.gb1.Size = new System.Drawing.Size(261, 110);
            this.gb1.TabIndex = 7;
            this.gb1.TabStop = false;
            this.gb1.Text = "UI / download log UI";
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
            this.showDownloadStatisticsInMainFormTitleCheckBox.Size = new System.Drawing.Size(185, 17);
            this.showDownloadStatisticsInMainFormTitleCheckBox.TabIndex = 1;
            this.showDownloadStatisticsInMainFormTitleCheckBox.Text = "show download statistics in main\r\nwindow title";
            this.showDownloadStatisticsInMainFormTitleCheckBox.UseVisualStyleBackColor = true;
            // 
            // only4NotRunLabel1
            // 
            this.only4NotRunLabel1.AutoSize = true;
            this.only4NotRunLabel1.ForeColor = System.Drawing.Color.DarkGoldenrod;
            this.only4NotRunLabel1.Location = new System.Drawing.Point(7, 29);
            this.only4NotRunLabel1.Size = new System.Drawing.Size(119, 13);
            this.only4NotRunLabel1.TabIndex = 10;
            this.only4NotRunLabel1.Text = "(only for not-running downloads)";
            this.only4NotRunLabel1.Visible = false;
            // 
            // only4NotRunLabel2
            // 
            this.only4NotRunLabel2.AutoSize = true;
            this.only4NotRunLabel2.ForeColor = System.Drawing.Color.DarkGoldenrod;
            this.only4NotRunLabel2.Location = new System.Drawing.Point(7, 71);
            this.only4NotRunLabel2.Size = new System.Drawing.Size(119, 13);
            this.only4NotRunLabel2.TabIndex = 11;
            this.only4NotRunLabel2.Text = "(only for not-running downloads)";
            this.only4NotRunLabel2.Visible = false;
            // 
            // outputFileExtensionLabel
            // 
            this.outputFileExtensionLabel.AutoSize = true;
            this.outputFileExtensionLabel.Location = new System.Drawing.Point(20, 120);
            this.outputFileExtensionLabel.Size = new System.Drawing.Size(117, 13);
            this.outputFileExtensionLabel.TabIndex = 5;
            this.outputFileExtensionLabel.Text = "default output file extension:";
            // 
            // outputFileExtensionTextBox
            // 
            this.outputFileExtensionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.outputFileExtensionTextBox.Location = new System.Drawing.Point(167, 120);
            this.outputFileExtensionTextBox.Size = new System.Drawing.Size(89, 18);
            this.outputFileExtensionTextBox.TabIndex = 6;
            this.outputFileExtensionTextBox.Font = new System.Drawing.Font( "Microsoft Sans Serif", 11.5F );
            this.outputFileExtensionTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.outputFileExtensionTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.outputFileExtensionTextBox.WordWrap = false;
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(286, 310);
            this.Controls.Add(this.outputFileExtensionLabel);
            this.Controls.Add(this.outputFileExtensionTextBox);
            this.Controls.Add(this.only4NotRunLabel2);
            this.Controls.Add(this.only4NotRunLabel1);
            this.Controls.Add(this.gb1);
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
            ((System.ComponentModel.ISupportInitialize)(this.attemptRequestCountByPartNUD)).EndInit();
            this.gb1.ResumeLayout(false);
            this.gb1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        #endregion

        private System.Windows.Forms.Label l1;
        private System.Windows.Forms.Label l2;
        private System.Windows.Forms.NumericUpDown attemptRequestCountByPartNUD;
        private System.Windows.Forms.DateTimePicker requestTimeoutByPartDTP;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.GroupBox gb1;
        private System.Windows.Forms.CheckBox showOnlyRequestRowsWithErrorsCheckBox;
        private System.Windows.Forms.CheckBox showDownloadStatisticsInMainFormTitleCheckBox;
        private System.Windows.Forms.CheckBox uniqueUrlsOnlyCheckBox;
        private System.Windows.Forms.Label only4NotRunLabel1;
        private System.Windows.Forms.Label only4NotRunLabel2;
        private System.Windows.Forms.Label outputFileExtensionLabel;
        private System.Windows.Forms.TextBox outputFileExtensionTextBox;
    }
}