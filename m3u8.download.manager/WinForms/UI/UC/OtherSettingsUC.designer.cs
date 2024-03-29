﻿namespace m3u8.download.manager.ui
{
    partial class OtherSettingsUC
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
            System.Windows.Forms.GroupBox gb3;
            System.Windows.Forms.GroupBox gb4;
            System.Windows.Forms.Label l1;
            System.Windows.Forms.Label l2;
            System.Windows.Forms.Label l4;
            System.Windows.Forms.Label l3;
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.showOnlyRequestRowsWithErrorsCheckBox = new System.Windows.Forms.CheckBox();
            this.showDownloadStatisticsInMainFormTitleCheckBox = new System.Windows.Forms.CheckBox();
            this.showAllDownloadsCompleted_NotificationCheckBox = new System.Windows.Forms.CheckBox();
            this.useDirectorySelectDialogModernCheckBox = new System.Windows.Forms.CheckBox();
            this.testDirectorySelectDialog = new System.Windows.Forms.Button();
            this.externalProgFilePathButton = new System.Windows.Forms.Button();
            this.externalProgFilePathTextBox = new System.Windows.Forms.TextBox();
            this.externalProgCaptionTextBox = new System.Windows.Forms.TextBox();
            this.externalProgApplyByDefaultCheckBox = new System.Windows.Forms.CheckBox();
            this.requestTimeoutByPartDTP = new System.Windows.Forms.DateTimePicker();
            this.uniqueUrlsOnlyCheckBox = new System.Windows.Forms.CheckBox();
            this.only4NotRunLabel1 = new System.Windows.Forms.Label();
            this.only4NotRunLabel2 = new System.Windows.Forms.Label();
            this.outputFileExtensionLabel = new System.Windows.Forms.Label();
            this.outputFileExtensionTextBox = new System.Windows.Forms.TextBox();
            this.attemptRequestCountByPartNUD = new System.Windows.Forms.NumericUpDownEx();            
            this.externalProgResetButton = new System.Windows.Forms.Button();
            this.collectGarbageButton = new System.Windows.Forms.Button();
            this.currentMemoryLabel = new System.Windows.Forms.Label();
            gb1 = new System.Windows.Forms.GroupBox();
            gb2 = new System.Windows.Forms.GroupBox();
            gb3 = new System.Windows.Forms.GroupBox();
            gb4 = new System.Windows.Forms.GroupBox();
            l1 = new System.Windows.Forms.Label();
            l2 = new System.Windows.Forms.Label();
            l4 = new System.Windows.Forms.Label();
            l3 = new System.Windows.Forms.Label();
            gb1.SuspendLayout();
            gb2.SuspendLayout();
            gb3.SuspendLayout();
            gb4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.attemptRequestCountByPartNUD)).BeginInit();
            this.SuspendLayout();
            // 
            // gb1
            //            
            gb1.Location = new System.Drawing.Point(13, 7);
            gb1.Size = new System.Drawing.Size(261, 150);
            gb1.TabIndex = 0;
            gb1.TabStop = false;
            gb1.Text = "Download params";
            gb1.Controls.Add(l1);
            gb1.Controls.Add(this.attemptRequestCountByPartNUD);
            gb1.Controls.Add(this.only4NotRunLabel1);
            gb1.Controls.Add(l2);
            gb1.Controls.Add(this.requestTimeoutByPartDTP);
            gb1.Controls.Add(this.only4NotRunLabel2);
            gb1.Controls.Add(this.outputFileExtensionLabel);
            gb1.Controls.Add(this.outputFileExtensionTextBox);
            gb1.Controls.Add(this.uniqueUrlsOnlyCheckBox);
            // 
            // l1
            // 
            l1.AutoSize = true;
            l1.Location = new System.Drawing.Point(12, 20);
            l1.Size = new System.Drawing.Size(148, 13);
            l1.TabIndex = 0;
            l1.Text = "attempt request count by part:";
            // 
            // l2
            // 
            l2.AutoSize = true;
            l2.Location = new System.Drawing.Point(43, 62);
            l2.Size = new System.Drawing.Size(117, 13);
            l2.TabIndex = 3;
            l2.Text = "request timeout by part:";
            // 
            // attemptRequestCountByPartNUD
            // 
            this.attemptRequestCountByPartNUD.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.attemptRequestCountByPartNUD.Location = new System.Drawing.Point(167, 18);
            this.attemptRequestCountByPartNUD.Minimum = new decimal(new int[] { 1, 0, 0, 0 } );
            this.attemptRequestCountByPartNUD.Size = new System.Drawing.Size(89, 16);
            this.attemptRequestCountByPartNUD.TabIndex = 1;
            this.attemptRequestCountByPartNUD.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.attemptRequestCountByPartNUD.Value = new decimal(new int[] { 1, 0, 0, 0 } );
            // 
            // requestTimeoutByPartDTP
            // 
            this.requestTimeoutByPartDTP.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.requestTimeoutByPartDTP.Location = new System.Drawing.Point(167, 60);
            this.requestTimeoutByPartDTP.ShowUpDown = true;
            this.requestTimeoutByPartDTP.Size = new System.Drawing.Size(91, 20);
            this.requestTimeoutByPartDTP.TabIndex = 2;
            // 
            // uniqueUrlsOnlyCheckBox
            // 
            this.uniqueUrlsOnlyCheckBox.AutoSize = true;
            this.uniqueUrlsOnlyCheckBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.uniqueUrlsOnlyCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.uniqueUrlsOnlyCheckBox.Location = new System.Drawing.Point(43, 98);
            this.uniqueUrlsOnlyCheckBox.Size = new System.Drawing.Size(117, 17);
            this.uniqueUrlsOnlyCheckBox.TabIndex = 4;
            this.uniqueUrlsOnlyCheckBox.Text = "use unique urls only";
            this.uniqueUrlsOnlyCheckBox.UseVisualStyleBackColor = true;
            // 
            // only4NotRunLabel1
            // 
            this.only4NotRunLabel1.AutoSize = true;
            this.only4NotRunLabel1.ForeColor = System.Drawing.Color.DarkGoldenrod;
            this.only4NotRunLabel1.Location = new System.Drawing.Point(7, 37);
            this.only4NotRunLabel1.Size = new System.Drawing.Size(157, 13);
            this.only4NotRunLabel1.TabIndex = 10;
            this.only4NotRunLabel1.Text = "(only for not-running downloads)";
            this.only4NotRunLabel1.Visible = false;
            // 
            // only4NotRunLabel2
            // 
            this.only4NotRunLabel2.AutoSize = true;
            this.only4NotRunLabel2.ForeColor = System.Drawing.Color.DarkGoldenrod;
            this.only4NotRunLabel2.Location = new System.Drawing.Point(7, 79);
            this.only4NotRunLabel2.Size = new System.Drawing.Size(157, 13);
            this.only4NotRunLabel2.TabIndex = 11;
            this.only4NotRunLabel2.Text = "(only for not-running downloads)";
            this.only4NotRunLabel2.Visible = false;
            // 
            // outputFileExtensionLabel
            // 
            this.outputFileExtensionLabel.AutoSize = true;
            this.outputFileExtensionLabel.Location = new System.Drawing.Point(20, 120);
            this.outputFileExtensionLabel.Size = new System.Drawing.Size(139, 21);
            this.outputFileExtensionLabel.TabIndex = 5;
            this.outputFileExtensionLabel.Text = "default output file extension:";
            // 
            // outputFileExtensionTextBox
            // 
            this.outputFileExtensionTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.outputFileExtensionTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.outputFileExtensionTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.5F);
            this.outputFileExtensionTextBox.Location = new System.Drawing.Point(167, 120);
            this.outputFileExtensionTextBox.Size = new System.Drawing.Size(89, 26);
            this.outputFileExtensionTextBox.TabIndex = 6;
            this.outputFileExtensionTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.outputFileExtensionTextBox.WordWrap = false;

            // 
            // gb2
            // 
            gb2.Location = new System.Drawing.Point(13, 163);
            gb2.Size = new System.Drawing.Size(261, 140);
            gb2.TabIndex = 1;
            gb2.TabStop = false;
            gb2.Text = "UI / download log UI";
            gb2.Controls.Add(this.showOnlyRequestRowsWithErrorsCheckBox);
            gb2.Controls.Add(this.showDownloadStatisticsInMainFormTitleCheckBox);
            gb2.Controls.Add(this.showAllDownloadsCompleted_NotificationCheckBox);
            gb2.Controls.Add(this.useDirectorySelectDialogModernCheckBox);
            gb2.Controls.Add(this.testDirectorySelectDialog);
            // 
            // showOnlyRequestRowsWithErrorsCheckBox
            // 
            this.showOnlyRequestRowsWithErrorsCheckBox.AutoSize = true;
            this.showOnlyRequestRowsWithErrorsCheckBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.showOnlyRequestRowsWithErrorsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.showOnlyRequestRowsWithErrorsCheckBox.Location = new System.Drawing.Point(30, 25);
            //this.showOnlyRequestRowsWithErrorsCheckBox.Size = new System.Drawing.Size(185, 17);
            this.showOnlyRequestRowsWithErrorsCheckBox.TabIndex = 0;
            this.showOnlyRequestRowsWithErrorsCheckBox.Text = "show only request rows with errors";
            this.showOnlyRequestRowsWithErrorsCheckBox.UseVisualStyleBackColor = true;
            // 
            // showDownloadStatisticsInMainFormTitleCheckBox
            // 
            this.showDownloadStatisticsInMainFormTitleCheckBox.AutoSize = true;
            this.showDownloadStatisticsInMainFormTitleCheckBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.showDownloadStatisticsInMainFormTitleCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.showDownloadStatisticsInMainFormTitleCheckBox.Location = new System.Drawing.Point(30, 45);
            //this.showDownloadStatisticsInMainFormTitleCheckBox.Size = new System.Drawing.Size(177, 30);
            this.showDownloadStatisticsInMainFormTitleCheckBox.TabIndex = 1;
            this.showDownloadStatisticsInMainFormTitleCheckBox.Text = "show download statistics in main\r\nwindow title";
            this.showDownloadStatisticsInMainFormTitleCheckBox.UseVisualStyleBackColor = true;            
            // 
            // showAllDownloadsCompleted_NotificationCheckBox
            // 
            this.showAllDownloadsCompleted_NotificationCheckBox.AutoSize = true;
            this.showAllDownloadsCompleted_NotificationCheckBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.showAllDownloadsCompleted_NotificationCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.showAllDownloadsCompleted_NotificationCheckBox.Location = new System.Drawing.Point(30, 80);
            this.showAllDownloadsCompleted_NotificationCheckBox.TabIndex = 2;
            this.showAllDownloadsCompleted_NotificationCheckBox.Text = "show all downloads completed notification";
            this.showAllDownloadsCompleted_NotificationCheckBox.UseVisualStyleBackColor = true;
            // 
            // useDirectorySelectDialogModernCheckBox
            // 
            this.useDirectorySelectDialogModernCheckBox.AutoSize = true;
            this.useDirectorySelectDialogModernCheckBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.useDirectorySelectDialogModernCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.useDirectorySelectDialogModernCheckBox.Location = new System.Drawing.Point(30, 105);
            //this.useDirectorySelectDialogClassicCheckBox.Size = new System.Drawing.Size(177, 30);
            this.useDirectorySelectDialogModernCheckBox.TabIndex = 2;
            this.useDirectorySelectDialogModernCheckBox.Text = "use directory select dialog modern style";
            this.useDirectorySelectDialogModernCheckBox.UseVisualStyleBackColor = true;
            // 
            // testDirectorySelectDialog
            // 
            this.testDirectorySelectDialog.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));            
            this.testDirectorySelectDialog.Cursor = System.Windows.Forms.Cursors.Hand;
            this.testDirectorySelectDialog.Location = new System.Drawing.Point(240, 105);
            this.testDirectorySelectDialog.Size = new System.Drawing.Size(16, 18);
            this.testDirectorySelectDialog.TabIndex = 3;
            this.toolTip.SetToolTip(this.testDirectorySelectDialog, "test directory select dialog" );
            this.testDirectorySelectDialog.UseVisualStyleBackColor = true;
            this.testDirectorySelectDialog.Click += new System.EventHandler(this.testDirectorySelectDialog_Click);
            this.testDirectorySelectDialog.Paint += new System.Windows.Forms.PaintEventHandler(this.testDirectorySelectDialog_Paint);
            // 
            // gb3
            // 
            gb3.Location = new System.Drawing.Point(285, 7);
            gb3.Size = new System.Drawing.Size(261, 150);
            gb3.TabIndex = 2;
            gb3.TabStop = false;
            gb3.Text = "External program";
            gb3.Controls.Add(this.externalProgResetButton);
            gb3.Controls.Add(this.externalProgFilePathButton);
            gb3.Controls.Add(this.externalProgFilePathTextBox);
            gb3.Controls.Add(this.externalProgCaptionTextBox);
            gb3.Controls.Add(this.externalProgApplyByDefaultCheckBox);
            gb3.Controls.Add(l3);
            gb3.Controls.Add(l4);
            // 
            // l3
            // 
            l3.AutoSize = true;
            l3.Location = new System.Drawing.Point(16, 19);
            l3.Size = new System.Drawing.Size(38, 13);
            l3.TabIndex = 0;
            l3.Text = "Name:";
            // 
            // l4
            // 
            l4.AutoSize = true;
            l4.Location = new System.Drawing.Point( 16, 69);
            l4.Size = new System.Drawing.Size( 32, 13 );
            l4.TabIndex = 1;
            l4.Text = "Path:";
            // 
            // externalProgFilePathButton
            // 
            this.externalProgFilePathButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));            
            this.externalProgFilePathButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.externalProgFilePathButton.Location = new System.Drawing.Point(245, 85);
            this.externalProgFilePathButton.Size = new System.Drawing.Size(16, 18);
            this.externalProgFilePathButton.TabIndex = 9;
            this.toolTip.SetToolTip(this.externalProgFilePathButton, "browse");
            this.externalProgFilePathButton.UseVisualStyleBackColor = true;
            this.externalProgFilePathButton.Click += new System.EventHandler(this.externalProgFilePathButton_Click);
            this.externalProgFilePathButton.Image = new System.Drawing.Bitmap( Properties.Resources.browse, new System.Drawing.Size( 10, 10 ) );
            this.externalProgFilePathButton.Margin = new System.Windows.Forms.Padding(0);
            // 
            // externalProgFilePathTextBox
            // 
            this.externalProgFilePathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
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
            this.externalProgCaptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.externalProgCaptionTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.externalProgCaptionTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.5F);
            this.externalProgCaptionTextBox.Location = new System.Drawing.Point(19, 35);
            this.externalProgCaptionTextBox.Size = new System.Drawing.Size(226, 18);
            this.externalProgCaptionTextBox.TabIndex = 7;
            this.externalProgCaptionTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.externalProgCaptionTextBox.WordWrap = false;
            this.externalProgCaptionTextBox.TextChanged += new System.EventHandler(this.externalProgCaptionTextBox_TextChanged);
            // 
            // externalProgResetButton
            // 
            this.externalProgResetButton.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.externalProgResetButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.externalProgResetButton.Location = new System.Drawing.Point( 241, 0 );
            this.externalProgResetButton.Size = new System.Drawing.Size( 18, 22 );
            this.externalProgResetButton.TabIndex = 10;
            this.externalProgResetButton.UseVisualStyleBackColor = true;
            this.externalProgResetButton.Click += new System.EventHandler( this.externalProgResetButton_Click );
            this.externalProgResetButton.Image = new System.Drawing.Bitmap( Properties.Resources.reset, new System.Drawing.Size( 12, 12 ) );
            this.toolTip.SetToolTip( this.externalProgResetButton, "reset" );
            // 
            // externalProgApplyByDefaultCheckBox
            // 
            this.externalProgApplyByDefaultCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;            
            this.externalProgApplyByDefaultCheckBox.AutoSize = true;
            this.externalProgApplyByDefaultCheckBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.externalProgApplyByDefaultCheckBox.Location = new System.Drawing.Point(6, 115);            
            this.externalProgApplyByDefaultCheckBox.Text = "Apply to all new downloads by default";
            //this.externalProgApplyByDefaultCheckBox.Size = new System.Drawing.Size( 245, 18 );
            //this.externalProgApplyByDefaultCheckBox.AutoEllipsis = true;
            // 
            // gb4
            // 
            gb4.Location = new System.Drawing.Point(285, 163);
            gb4.Size = new System.Drawing.Size(261, 85);
            gb4.TabIndex = 4;
            gb4.TabStop = false;
            gb4.Text = "GC";
            gb4.Controls.Add(this.currentMemoryLabel);
            gb4.Controls.Add(this.collectGarbageButton);
            // 
            // collectGarbageButton
            // 
            this.collectGarbageButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.collectGarbageButton.Location = new System.Drawing.Point(71, 22);
            this.collectGarbageButton.Size = new System.Drawing.Size(119, 23);
            this.collectGarbageButton.TabIndex = 0;
            this.collectGarbageButton.Text = "Collect Garbage";
            this.collectGarbageButton.UseVisualStyleBackColor = true;
            this.collectGarbageButton.Click += new System.EventHandler(this.collectGarbageButton_Click);
            // 
            // currentMemoryLabel
            // 
            this.currentMemoryLabel.AutoEllipsis = true;
            this.currentMemoryLabel.BackColor = System.Drawing.Color.White;
            this.currentMemoryLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.currentMemoryLabel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.currentMemoryLabel.ForeColor = System.Drawing.Color.DimGray;
            this.currentMemoryLabel.Location = new System.Drawing.Point(34, 52);
            this.currentMemoryLabel.Margin = new System.Windows.Forms.Padding(0);
            this.currentMemoryLabel.Size = new System.Drawing.Size(189, 18);
            this.currentMemoryLabel.TabIndex = 1;
            this.currentMemoryLabel.Text = "...";
            this.currentMemoryLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.currentMemoryLabel.Visible = false;
            // 
            // OtherSettingsUC
            // 
            this.AutoScroll = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(560, 315);
            this.Controls.Add(gb1);
            this.Controls.Add(gb2);            
            this.Controls.Add(gb3);
            this.Controls.Add(gb4);
            this.Text = "settings";
            gb1.ResumeLayout(false);
            gb1.PerformLayout();
            gb2.ResumeLayout(false);
            gb2.PerformLayout();
            gb3.ResumeLayout(false);
            gb3.PerformLayout();
            gb4.ResumeLayout(false);
            gb4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.attemptRequestCountByPartNUD)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        #endregion

        private System.Windows.Forms.NumericUpDownEx attemptRequestCountByPartNUD;
        private System.Windows.Forms.DateTimePicker requestTimeoutByPartDTP;
        private System.Windows.Forms.CheckBox showOnlyRequestRowsWithErrorsCheckBox;
        private System.Windows.Forms.CheckBox showDownloadStatisticsInMainFormTitleCheckBox;
        private System.Windows.Forms.CheckBox showAllDownloadsCompleted_NotificationCheckBox;
        private System.Windows.Forms.CheckBox useDirectorySelectDialogModernCheckBox;
        private System.Windows.Forms.Button testDirectorySelectDialog;
        private System.Windows.Forms.CheckBox uniqueUrlsOnlyCheckBox;
        private System.Windows.Forms.Label only4NotRunLabel1;
        private System.Windows.Forms.Label only4NotRunLabel2;
        private System.Windows.Forms.Label outputFileExtensionLabel;
        private System.Windows.Forms.TextBox outputFileExtensionTextBox;
        private System.Windows.Forms.TextBox externalProgFilePathTextBox;
        private System.Windows.Forms.TextBox externalProgCaptionTextBox;
        private System.Windows.Forms.CheckBox externalProgApplyByDefaultCheckBox;        
        private System.Windows.Forms.Button externalProgFilePathButton;
        private System.Windows.Forms.Button externalProgResetButton;
        private System.Windows.Forms.Button collectGarbageButton;
        private System.Windows.Forms.Label currentMemoryLabel;
        private System.Windows.Forms.ToolTip toolTip;
    }
}