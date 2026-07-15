namespace m3u8.download.manager.ui
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
            System.Windows.Forms.GroupBox downloadParamsGroupBox;
            System.Windows.Forms.GroupBox ui_downloadLogUIGroupBox;
            System.Windows.Forms.GroupBox externalProgGroupBox;
            System.Windows.Forms.GroupBox ffmpegGroupBox;
            System.Windows.Forms.GroupBox gcGroupBox;
            System.Windows.Forms.GroupBox receivedAndWritedPartsGroupBox;
            System.Windows.Forms.Label attemptRequestCountLabel;
            System.Windows.Forms.Label requestTimeoutByPartLabel;
            System.Windows.Forms.Label externalProgFilePathLabel;
            System.Windows.Forms.Label externalProgCaptionlabel;
            System.Windows.Forms.PictureBox externalProgPictureBox;
            System.Windows.Forms.Label ffmpegFilePathLabel;
            System.Windows.Forms.Label ffmpegCaptionlabel;
            System.Windows.Forms.PictureBox ffmpegPictureBox;
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.showOnlyRequestRowsWithErrorsCheckBox = new System.Windows.Forms.CheckBox();
            this.showDownloadStatisticsInMainFormTitleCheckBox = new System.Windows.Forms.CheckBox();
            this.showAllDownloadsCompleted_NotificationCheckBox = new System.Windows.Forms.CheckBox();
            this.useDirectorySelectDialogModernCheckBox = new System.Windows.Forms.CheckBox();
            this.testDirectorySelectDialog = new System.Windows.Forms.Button();
            this.externalProgFilePathButton = new System.Windows.Forms.Button();
            this.externalProgFilePathTextBox = new System.Windows.Forms.TextBoxEx();
            this.externalProgCaptionTextBox = new System.Windows.Forms.TextBoxEx();
            this.externalProgApplyByDefaultCheckBox = new System.Windows.Forms.CheckBox();
            this.ffmpegFilePathTextBox = new System.Windows.Forms.TextBoxEx();
            this.ffmpegCaptionTextBox = new System.Windows.Forms.TextBoxEx();
            this.ffmpegApplyByDefaultCheckBox = new System.Windows.Forms.CheckBox();
            this.ffmpegFilePathButton = new System.Windows.Forms.Button();
            this.ffmpegResetButton = new System.Windows.Forms.Button();            
            this.requestTimeoutByPartDTP = new System.Windows.Forms.BorderDateTimePicker();
            this.uniqueUrlsOnlyCheckBox = new System.Windows.Forms.CheckBox();
            this.ignoreHostHttpHeaderCheckBox = new System.Windows.Forms.CheckBox();
            this.only4NotRunLabel1 = new System.Windows.Forms.Label();
            this.only4NotRunLabel2 = new System.Windows.Forms.Label();
            this.outputFileExtensionLabel = new System.Windows.Forms.Label();
            this.outputFileExtensionTextBox = new System.Windows.Forms.TextBoxEx();
            this.attemptRequestCountByPartNUD = new System.Windows.Forms.NumericUpDownEx();            
            this.externalProgResetButton = new System.Windows.Forms.Button();
            this.collectGarbageButton = new System.Windows.Forms.Button();
            this.currentMemoryLabel = new System.Windows.Forms.Label();
            this.receivedAndWritedPartsClearAllButton = new System.Windows.Forms.Button();
            this.receivedAndWritedPartsLabel = new System.Windows.Forms.Label();
            downloadParamsGroupBox = new System.Windows.Forms.GroupBox();
            ui_downloadLogUIGroupBox = new System.Windows.Forms.GroupBox();
            externalProgGroupBox = new System.Windows.Forms.GroupBox();
            externalProgPictureBox = new System.Windows.Forms.PictureBox();
            ffmpegGroupBox = new System.Windows.Forms.GroupBox();
            ffmpegPictureBox = new System.Windows.Forms.PictureBox();
            gcGroupBox = new System.Windows.Forms.GroupBox();
            receivedAndWritedPartsGroupBox = new System.Windows.Forms.GroupBox();
            attemptRequestCountLabel = new System.Windows.Forms.Label();
            requestTimeoutByPartLabel = new System.Windows.Forms.Label();
            externalProgFilePathLabel = new System.Windows.Forms.Label();
            externalProgCaptionlabel = new System.Windows.Forms.Label();
            ffmpegFilePathLabel = new System.Windows.Forms.Label();
            ffmpegCaptionlabel = new System.Windows.Forms.Label();
            downloadParamsGroupBox.SuspendLayout();
            ui_downloadLogUIGroupBox.SuspendLayout();
            externalProgGroupBox.SuspendLayout();
            ffmpegGroupBox.SuspendLayout();
            gcGroupBox.SuspendLayout();
            receivedAndWritedPartsGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.attemptRequestCountByPartNUD)).BeginInit();
            this.SuspendLayout();
            // 
            // downloadParamsGroupBox
            //            
            downloadParamsGroupBox.Location = new System.Drawing.Point(13, 7);
            downloadParamsGroupBox.Size = new System.Drawing.Size(261, 172);
            downloadParamsGroupBox.TabIndex = 0;
            downloadParamsGroupBox.TabStop = false;
            downloadParamsGroupBox.Text = "Download params";
            downloadParamsGroupBox.Controls.Add(attemptRequestCountLabel);
            downloadParamsGroupBox.Controls.Add(this.attemptRequestCountByPartNUD);
            downloadParamsGroupBox.Controls.Add(this.only4NotRunLabel1);
            downloadParamsGroupBox.Controls.Add(requestTimeoutByPartLabel);
            downloadParamsGroupBox.Controls.Add(this.requestTimeoutByPartDTP);
            downloadParamsGroupBox.Controls.Add(this.only4NotRunLabel2);
            downloadParamsGroupBox.Controls.Add(this.outputFileExtensionLabel);
            downloadParamsGroupBox.Controls.Add(this.outputFileExtensionTextBox);
            downloadParamsGroupBox.Controls.Add(this.uniqueUrlsOnlyCheckBox);
            downloadParamsGroupBox.Controls.Add(this.ignoreHostHttpHeaderCheckBox);
            // 
            // attemptRequestCountLabel
            // 
            attemptRequestCountLabel.AutoSize = true;
            attemptRequestCountLabel.Location = new System.Drawing.Point(12, 20);
            attemptRequestCountLabel.Size = new System.Drawing.Size(148, 13);
            attemptRequestCountLabel.TabIndex = 0;
            attemptRequestCountLabel.Text = "attempt request count by part:";
            // 
            // requestTimeoutByPartLabel
            //  
            requestTimeoutByPartLabel.AutoSize = true;
            requestTimeoutByPartLabel.Location = new System.Drawing.Point(43, 62);
            requestTimeoutByPartLabel.Size = new System.Drawing.Size(117, 13);
            requestTimeoutByPartLabel.TabIndex = 3;
            requestTimeoutByPartLabel.Text = "request timeout by part:";
            // 
            // attemptRequestCountByPartNUD
            // 
            //this.attemptRequestCountByPartNUD.BorderStyle = System.Windows.Forms.BorderStyle.None;
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
            // ignoreHostHttpHeaderCheckBox
            // 
            this.ignoreHostHttpHeaderCheckBox.AutoSize = true;
            this.ignoreHostHttpHeaderCheckBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ignoreHostHttpHeaderCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.ignoreHostHttpHeaderCheckBox.Location = new System.Drawing.Point(43, 120);
            this.ignoreHostHttpHeaderCheckBox.Size = new System.Drawing.Size(117, 17);
            this.ignoreHostHttpHeaderCheckBox.TabIndex = 5;
            this.ignoreHostHttpHeaderCheckBox.Text = "ignore \"Host\" http-header";
            this.ignoreHostHttpHeaderCheckBox.UseVisualStyleBackColor = true;
            // 
            // outputFileExtensionLabel
            // 
            this.outputFileExtensionLabel.AutoSize = true;
            this.outputFileExtensionLabel.Location = new System.Drawing.Point(20, 142);
            this.outputFileExtensionLabel.Size = new System.Drawing.Size(139, 21);
            this.outputFileExtensionLabel.TabIndex = 5;
            this.outputFileExtensionLabel.Text = "default output file extension:";
            // 
            // outputFileExtensionTextBox
            // 
            this.outputFileExtensionTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            //this.outputFileExtensionTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.outputFileExtensionTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.5F);
            this.outputFileExtensionTextBox.Location = new System.Drawing.Point(167, 142);
            this.outputFileExtensionTextBox.Size = new System.Drawing.Size(89, 26);
            this.outputFileExtensionTextBox.TabIndex = 6;
            this.outputFileExtensionTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.outputFileExtensionTextBox.WordWrap = false;
            this.outputFileExtensionTextBox.PlaceHolderText = "extension";
            this.outputFileExtensionTextBox.DrawClearButton = false;

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
            // ui_downloadLogUIGroupBox
            // 
            ui_downloadLogUIGroupBox.Location = new System.Drawing.Point(13, 185);
            ui_downloadLogUIGroupBox.Size = new System.Drawing.Size(261, 140);
            ui_downloadLogUIGroupBox.TabIndex = 1;
            ui_downloadLogUIGroupBox.TabStop = false;
            ui_downloadLogUIGroupBox.Text = "UI / download log UI";
            ui_downloadLogUIGroupBox.Controls.Add(this.showOnlyRequestRowsWithErrorsCheckBox);
            ui_downloadLogUIGroupBox.Controls.Add(this.showDownloadStatisticsInMainFormTitleCheckBox);
            ui_downloadLogUIGroupBox.Controls.Add(this.showAllDownloadsCompleted_NotificationCheckBox);
            ui_downloadLogUIGroupBox.Controls.Add(this.useDirectorySelectDialogModernCheckBox);
            ui_downloadLogUIGroupBox.Controls.Add(this.testDirectorySelectDialog);
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
            // externalProgGroupBox
            // 
            externalProgGroupBox.Location = new System.Drawing.Point(285, 7);
            externalProgGroupBox.Size = new System.Drawing.Size(261, 125);
            externalProgGroupBox.TabIndex = 2;
            externalProgGroupBox.TabStop = false;
            externalProgGroupBox.Text = "      External program";
            externalProgGroupBox.Controls.Add(externalProgPictureBox);
            externalProgGroupBox.Controls.Add(this.externalProgResetButton);
            externalProgGroupBox.Controls.Add(this.externalProgFilePathButton);
            externalProgGroupBox.Controls.Add(this.externalProgFilePathTextBox);
            externalProgGroupBox.Controls.Add(this.externalProgCaptionTextBox);
            externalProgGroupBox.Controls.Add(this.externalProgApplyByDefaultCheckBox);
            externalProgGroupBox.Controls.Add(externalProgCaptionlabel);
            externalProgGroupBox.Controls.Add(externalProgFilePathLabel);
            //
            // externalProgPictureBox
            // 
            externalProgPictureBox.Image = m3u8.download.manager.Properties.Resources.freemake_16х16;
            externalProgPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            externalProgPictureBox.Location = new System.Drawing.Point(5, 0);
            // 
            // externalProgCaptionlabel
            // 
            externalProgCaptionlabel.AutoSize = true;
            externalProgCaptionlabel.Location = new System.Drawing.Point(16, 19 - 4);
            externalProgCaptionlabel.Size = new System.Drawing.Size(38, 13);
            externalProgCaptionlabel.TabIndex = 0;
            externalProgCaptionlabel.Text = "Name:";
            // 
            // externalProgFilePathLabel
            // 
            externalProgFilePathLabel.AutoSize = true;
            externalProgFilePathLabel.Location = new System.Drawing.Point(16, 55);
            externalProgFilePathLabel.Size = new System.Drawing.Size(32, 13);
            externalProgFilePathLabel.TabIndex = 1;
            externalProgFilePathLabel.Text = "Path:";
            // 
            // externalProgFilePathButton
            // 
            this.externalProgFilePathButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));            
            this.externalProgFilePathButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.externalProgFilePathButton.Location = new System.Drawing.Point(244, 73);
            this.externalProgFilePathButton.Size = new System.Drawing.Size(16, 18);
            this.externalProgFilePathButton.TabIndex = 9;            
            this.externalProgFilePathButton.UseVisualStyleBackColor = true;            
            this.externalProgFilePathButton.Image = new System.Drawing.Bitmap( Properties.Resources.browse, new System.Drawing.Size( 10, 10 ) );
            this.externalProgFilePathButton.Margin = new System.Windows.Forms.Padding(0);
            this.externalProgFilePathButton.Click += new System.EventHandler(this.externalProgFilePathButton_Click);
            this.toolTip.SetToolTip(this.externalProgFilePathButton, "select path");
            // 
            // externalProgFilePathTextBox
            // 
            this.externalProgFilePathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            //this.externalProgFilePathTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.externalProgFilePathTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.5F);
            this.externalProgFilePathTextBox.Location = new System.Drawing.Point(6, 71);
            this.externalProgFilePathTextBox.Size = new System.Drawing.Size(239, 18);
            this.externalProgFilePathTextBox.TabIndex = 8;
            this.externalProgFilePathTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.externalProgFilePathTextBox.WordWrap = false;
            this.externalProgFilePathTextBox.PlaceHolderText = "path to external program";
            this.externalProgFilePathTextBox.DrawClearButton = false;
            this.externalProgFilePathTextBox.TextChanged += new System.EventHandler(this.externalProgFilePathTextBox_TextChanged);
            // 
            // externalProgCaptionTextBox
            // 
            this.externalProgCaptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            //this.externalProgCaptionTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.externalProgCaptionTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.5F);
            this.externalProgCaptionTextBox.Location = new System.Drawing.Point(19, 35 - 4);
            this.externalProgCaptionTextBox.Size = new System.Drawing.Size(226, 18);
            this.externalProgCaptionTextBox.TabIndex = 7;
            this.externalProgCaptionTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.externalProgCaptionTextBox.WordWrap = false;
            this.externalProgCaptionTextBox.PlaceHolderText = "caption of external program";
            this.externalProgCaptionTextBox.DrawClearButton = false;
            this.externalProgCaptionTextBox.TextChanged += new System.EventHandler(this.externalProgCaptionTextBox_TextChanged);
            // 
            // externalProgResetButton
            // 
            this.externalProgResetButton.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.externalProgResetButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.externalProgResetButton.Location = new System.Drawing.Point(241, 0);
            this.externalProgResetButton.Size = new System.Drawing.Size(18, 22);
            this.externalProgResetButton.TabIndex = 10;
            this.externalProgResetButton.UseVisualStyleBackColor = true;
            this.externalProgResetButton.Click += new System.EventHandler( this.externalProgResetButton_Click );
            this.externalProgResetButton.Image = new System.Drawing.Bitmap( Properties.Resources.reset, new System.Drawing.Size(12, 12) );
            this.toolTip.SetToolTip( this.externalProgResetButton, "reset" );
            // 
            // externalProgApplyByDefaultCheckBox
            // 
            this.externalProgApplyByDefaultCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;            
            this.externalProgApplyByDefaultCheckBox.AutoSize = true;
            this.externalProgApplyByDefaultCheckBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.externalProgApplyByDefaultCheckBox.Location = new System.Drawing.Point(6, 101);
            this.externalProgApplyByDefaultCheckBox.Text = "Apply to all new downloads by default";
            //this.externalProgApplyByDefaultCheckBox.Size = new System.Drawing.Size( 245, 18 );
            //this.externalProgApplyByDefaultCheckBox.AutoEllipsis = true;
            // 
            // ffmpegGroupBox
            // 
            ffmpegGroupBox.Location = new System.Drawing.Point(285, 138);
            ffmpegGroupBox.Size = new System.Drawing.Size(261, 125);
            ffmpegGroupBox.TabIndex = 2;
            ffmpegGroupBox.TabStop = false;
            ffmpegGroupBox.Text = "      FFmpeg converter";
            ffmpegGroupBox.Controls.Add(ffmpegPictureBox);
            ffmpegGroupBox.Controls.Add(this.ffmpegResetButton);
            ffmpegGroupBox.Controls.Add(this.ffmpegFilePathButton);
            ffmpegGroupBox.Controls.Add(this.ffmpegFilePathTextBox);
            ffmpegGroupBox.Controls.Add(this.ffmpegCaptionTextBox);
            ffmpegGroupBox.Controls.Add(this.ffmpegApplyByDefaultCheckBox);
            ffmpegGroupBox.Controls.Add(ffmpegCaptionlabel);
            ffmpegGroupBox.Controls.Add(ffmpegFilePathLabel);
            //
            // ffmpegPictureBox
            // 
            ffmpegPictureBox.Image = m3u8.download.manager.Properties.Resources.ffmpeg_16х16;
            ffmpegPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            ffmpegPictureBox.Location = new System.Drawing.Point(5, 0);
            // 
            // ffmpegCaptionlabel
            // 
            ffmpegCaptionlabel.AutoSize = true;
            ffmpegCaptionlabel.Location = new System.Drawing.Point(16, 19 - 4);
            ffmpegCaptionlabel.Size = new System.Drawing.Size(38, 13);
            ffmpegCaptionlabel.TabIndex = 0;
            ffmpegCaptionlabel.Text = "Name:";
            // 
            // ffmpegFilePathLabel
            // 
            ffmpegFilePathLabel.AutoSize = true;
            ffmpegFilePathLabel.Location = new System.Drawing.Point(16, 55);
            ffmpegFilePathLabel.Size = new System.Drawing.Size(32, 13);
            ffmpegFilePathLabel.TabIndex = 1;
            ffmpegFilePathLabel.Text = "Path:";
            // 
            // ffmpegFilePathButton
            // 
            this.ffmpegFilePathButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));            
            this.ffmpegFilePathButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ffmpegFilePathButton.Location = new System.Drawing.Point(244, 73);
            this.ffmpegFilePathButton.Size = new System.Drawing.Size(16, 18);
            this.ffmpegFilePathButton.TabIndex = 9;            
            this.ffmpegFilePathButton.UseVisualStyleBackColor = true;            
            this.ffmpegFilePathButton.Image = new System.Drawing.Bitmap( Properties.Resources.browse, new System.Drawing.Size( 10, 10 ) );
            this.ffmpegFilePathButton.Margin = new System.Windows.Forms.Padding(0);
            this.ffmpegFilePathButton.Click += new System.EventHandler(this.ffmpegFilePathButton_Click);
            this.toolTip.SetToolTip(this.ffmpegFilePathButton, "select path");
            // 
            // ffmpegFilePathTextBox
            // 
            this.ffmpegFilePathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            //this.ffmpegFilePathTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ffmpegFilePathTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.5F);
            this.ffmpegFilePathTextBox.Location = new System.Drawing.Point(6, 71);
            this.ffmpegFilePathTextBox.Size = new System.Drawing.Size(239, 18);
            this.ffmpegFilePathTextBox.TabIndex = 8;
            this.ffmpegFilePathTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.ffmpegFilePathTextBox.WordWrap = false;
            this.ffmpegFilePathTextBox.PlaceHolderText = "path to ffmpeg.exe program";
            this.ffmpegFilePathTextBox.DrawClearButton = false;
            this.ffmpegFilePathTextBox.TextChanged += new System.EventHandler(this.ffmpegFilePathTextBox_TextChanged);
            // 
            // ffmpegCaptionTextBox
            // 
            this.ffmpegCaptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            //this.ffmpegCaptionTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ffmpegCaptionTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.5F);
            this.ffmpegCaptionTextBox.Location = new System.Drawing.Point(19, 35 - 4);
            this.ffmpegCaptionTextBox.Size = new System.Drawing.Size(226, 18);
            this.ffmpegCaptionTextBox.TabIndex = 7;
            this.ffmpegCaptionTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.ffmpegCaptionTextBox.WordWrap = false;
            this.ffmpegCaptionTextBox.PlaceHolderText = "caption of ffmpeg.exe program";
            this.ffmpegCaptionTextBox.DrawClearButton = false;
            this.ffmpegCaptionTextBox.TextChanged += new System.EventHandler(this.ffmpegCaptionTextBox_TextChanged);
            // 
            // ffmpegResetButton
            // 
            this.ffmpegResetButton.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ffmpegResetButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ffmpegResetButton.Location = new System.Drawing.Point(241, 0);
            this.ffmpegResetButton.Size = new System.Drawing.Size(18, 22);
            this.ffmpegResetButton.TabIndex = 10;
            this.ffmpegResetButton.UseVisualStyleBackColor = true;
            this.ffmpegResetButton.Click += new System.EventHandler(this.ffmpegResetButton_Click);
            this.ffmpegResetButton.Image = new System.Drawing.Bitmap( Properties.Resources.reset, new System.Drawing.Size(12, 12) );
            this.toolTip.SetToolTip(this.ffmpegResetButton, "reset");
            // 
            // ffmpegApplyByDefaultCheckBox
            // 
            this.ffmpegApplyByDefaultCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.ffmpegApplyByDefaultCheckBox.AutoSize = true;
            this.ffmpegApplyByDefaultCheckBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ffmpegApplyByDefaultCheckBox.Location = new System.Drawing.Point(6, 101);
            this.ffmpegApplyByDefaultCheckBox.Text = "Apply to all new downloads by default";
            // 
            // gcGroupBox
            // 
            gcGroupBox.Location = new System.Drawing.Point(285, 269);
            gcGroupBox.Size = new System.Drawing.Size(261, 85);
            gcGroupBox.TabIndex = 4;
            gcGroupBox.TabStop = false;
            gcGroupBox.Text = "GC";
            gcGroupBox.Controls.Add(this.currentMemoryLabel);
            gcGroupBox.Controls.Add(this.collectGarbageButton);
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
            // receivedAndWritedPartsGroupBox
            // 
            receivedAndWritedPartsGroupBox.Location = new System.Drawing.Point(13, 331/*285, 360*/);
            receivedAndWritedPartsGroupBox.Size = new System.Drawing.Size(261, 85);
            receivedAndWritedPartsGroupBox.TabIndex = 4;
            receivedAndWritedPartsGroupBox.TabStop = false;
            receivedAndWritedPartsGroupBox.Text = "Stored files info";
            receivedAndWritedPartsGroupBox.Controls.Add(this.receivedAndWritedPartsLabel);
            receivedAndWritedPartsGroupBox.Controls.Add(this.receivedAndWritedPartsClearAllButton);
            // 
            // receivedAndWritedPartsClearAllButton
            // 
            this.receivedAndWritedPartsClearAllButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.receivedAndWritedPartsClearAllButton.Location = new System.Drawing.Point(50, 22);
            this.receivedAndWritedPartsClearAllButton.Size = new System.Drawing.Size(160, 23);
            this.receivedAndWritedPartsClearAllButton.TabIndex = 0;
            this.receivedAndWritedPartsClearAllButton.ForeColor = System.Drawing.Color.Maroon;
            this.receivedAndWritedPartsClearAllButton.Text = "Clear all info about stored files";
            //---this.receivedAndWritedPartsClearAllButton.UseVisualStyleBackColor = true;
            this.receivedAndWritedPartsClearAllButton.Click += new System.EventHandler(this.receivedAndWritedPartsClearAllButton_Click);
            this.toolTip.SetToolTip(this.receivedAndWritedPartsClearAllButton, this.receivedAndWritedPartsClearAllButton.Text);
            // 
            // receivedAndWritedPartsLabel
            // 
            this.receivedAndWritedPartsLabel.AutoEllipsis = true;
            this.receivedAndWritedPartsLabel.BackColor = System.Drawing.Color.White;
            this.receivedAndWritedPartsLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.receivedAndWritedPartsLabel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.receivedAndWritedPartsLabel.ForeColor = System.Drawing.Color.DimGray;
            this.receivedAndWritedPartsLabel.Location = new System.Drawing.Point(34, 52);
            this.receivedAndWritedPartsLabel.Margin = new System.Windows.Forms.Padding(0);
            this.receivedAndWritedPartsLabel.Size = new System.Drawing.Size(189, 18);
            this.receivedAndWritedPartsLabel.TabIndex = 1;
            this.receivedAndWritedPartsLabel.Text = "...";
            this.receivedAndWritedPartsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.receivedAndWritedPartsLabel.Visible = false;
            // 
            // OtherSettingsUC
            // 
            this.AutoScroll = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(560, 406);
            this.Controls.Add(downloadParamsGroupBox);
            this.Controls.Add(ui_downloadLogUIGroupBox);            
            this.Controls.Add(externalProgGroupBox);
            this.Controls.Add(ffmpegGroupBox);
            this.Controls.Add(gcGroupBox);
            this.Controls.Add(receivedAndWritedPartsGroupBox);
            this.Text = "settings";
            downloadParamsGroupBox.ResumeLayout(false);
            downloadParamsGroupBox.PerformLayout();
            ui_downloadLogUIGroupBox.ResumeLayout(false);
            ui_downloadLogUIGroupBox.PerformLayout();
            externalProgGroupBox.ResumeLayout(false);
            externalProgGroupBox.PerformLayout();
            ffmpegGroupBox.ResumeLayout(false);
            ffmpegGroupBox.PerformLayout();
            gcGroupBox.ResumeLayout(false);
            gcGroupBox.PerformLayout();
            receivedAndWritedPartsGroupBox.ResumeLayout(false);
            receivedAndWritedPartsGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.attemptRequestCountByPartNUD)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        #endregion

        private System.Windows.Forms.NumericUpDownEx attemptRequestCountByPartNUD;
        private System.Windows.Forms.BorderDateTimePicker requestTimeoutByPartDTP;
        private System.Windows.Forms.CheckBox showOnlyRequestRowsWithErrorsCheckBox;
        private System.Windows.Forms.CheckBox showDownloadStatisticsInMainFormTitleCheckBox;
        private System.Windows.Forms.CheckBox showAllDownloadsCompleted_NotificationCheckBox;
        private System.Windows.Forms.CheckBox useDirectorySelectDialogModernCheckBox;
        private System.Windows.Forms.Button testDirectorySelectDialog;
        private System.Windows.Forms.CheckBox uniqueUrlsOnlyCheckBox;
        private System.Windows.Forms.CheckBox ignoreHostHttpHeaderCheckBox;
        private System.Windows.Forms.Label only4NotRunLabel1;
        private System.Windows.Forms.Label only4NotRunLabel2;
        private System.Windows.Forms.Label outputFileExtensionLabel;
        private System.Windows.Forms.TextBoxEx outputFileExtensionTextBox;
        private System.Windows.Forms.TextBoxEx externalProgFilePathTextBox;
        private System.Windows.Forms.TextBoxEx externalProgCaptionTextBox;
        private System.Windows.Forms.CheckBox externalProgApplyByDefaultCheckBox;
        private System.Windows.Forms.Button externalProgFilePathButton;
        private System.Windows.Forms.Button externalProgResetButton;
        private System.Windows.Forms.TextBoxEx ffmpegFilePathTextBox;
        private System.Windows.Forms.TextBoxEx ffmpegCaptionTextBox;
        private System.Windows.Forms.CheckBox ffmpegApplyByDefaultCheckBox;
        private System.Windows.Forms.Button ffmpegFilePathButton;
        private System.Windows.Forms.Button ffmpegResetButton;
        private System.Windows.Forms.Button collectGarbageButton;
        private System.Windows.Forms.Label currentMemoryLabel;
        private System.Windows.Forms.Button receivedAndWritedPartsClearAllButton;
        private System.Windows.Forms.Label receivedAndWritedPartsLabel;
        private System.Windows.Forms.ToolTip toolTip;
    }
}
