using _DC_ = m3u8.download.manager.controllers.DownloadController;
using _SC_ = m3u8.download.manager.controllers.SettingsPropertyChangeController;

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
        private void InitializeComponent( _DC_ dc, _SC_ sc )
        {
            this.components = new System.ComponentModel.Container();
            this.l1 = new System.Windows.Forms.Label();
            this.l2 = new System.Windows.Forms.Label();
            this.topPanel = new System.Windows.Forms.Panel();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.mainTabPage = new System.Windows.Forms.TabPage();
            this.requestHeadersTabPage = new System.Windows.Forms.TabPage();
            this.requestHeadersEditor = new RequestHeadersEditor( sc );
            this.m3u8FileUrlTextBox = new System.Windows.Forms.TextBox();
            this.outputFileNameTextBox = new TextBoxWithCustomPathPaste();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.outputDirectorySelectButton = new System.Windows.Forms.ButtonWithFocusCues();
            this.outputFileNameSelectButton = new System.Windows.Forms.ButtonWithFocusCues();
            this.outputFileNameClearButton = new System.Windows.Forms.ButtonWithFocusCues();
            this.logPanel = new System.Windows.Forms.Panel();
            this.logUC = new m3u8.download.manager.ui.LogUC( sc );
            this.mainLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.outputDirectoryTextBox = new TextBoxWithCustomPathPaste();
            this.isLiveStreamCheckBox = new System.Windows.Forms.CheckBox();
            this.liveStreamMaxSizeInMbLabel = new System.Windows.Forms.Label();
            this.liveStreamMaxSizeInMbNumUpDn = new System.Windows.Forms.NumericUpDownEx();
            this.patternOutputFileNameLabelCaption = new System.Windows.Forms.Label();
            this.patternOutputFileNameLabel = new System.Windows.Forms.Label();
            this.patternOutputFileNameNumUpDn = new System.Windows.Forms.NumericUpDownEx();
            this.loadM3u8FileContentButton = new System.Windows.Forms.ButtonWithFocusCues();
            this.externalProgApplyByDefaultCheckBox = new System.Windows.Forms.CheckBox();
            this.buttomPanel = new System.Windows.Forms.Panel();
            this.downloadStartButton = new System.Windows.Forms.ButtonWithFocusCues();
            this.downloadLaterButton = new System.Windows.Forms.ButtonWithFocusCues();
            this.statusBarUC = new m3u8.download.manager.ui.StatusBarUC( dc, sc );
            this.topPanel.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.mainTabPage.SuspendLayout();
            this.requestHeadersTabPage.SuspendLayout();
            this.logPanel.SuspendLayout();
            this.mainLayoutPanel.SuspendLayout();
            this.buttomPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // l1
            // 
            this.l1.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.l1.AutoSize = true;
            this.l1.ForeColor = System.Drawing.Color.SteelBlue;
            this.l1.Location = new System.Drawing.Point(6, 2);
            this.l1.Size = new System.Drawing.Size(55, 26);
            this.l1.TabIndex = 0;
            this.l1.Text = "output \r\nfile name :";
            this.l1.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // l2
            // 
            this.l2.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.l2.AutoSize = true;
            this.l2.ForeColor = System.Drawing.Color.SteelBlue;
            this.l2.Location = new System.Drawing.Point(6, 62);
            this.l2.Size = new System.Drawing.Size(55, 26);
            this.l2.TabIndex = 4;
            this.l2.Text = "output \r\ndirectory :";
            this.l2.TextAlign = System.Drawing.ContentAlignment.TopRight;

            // 
            // topPanel
            // 
            this.topPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.topPanel.Controls.Add(this.m3u8FileUrlTextBox);
            this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.topPanel.Location = new System.Drawing.Point(0, 0);
            this.topPanel.Size = new System.Drawing.Size(803, 81);
            this.topPanel.TabIndex = 0;
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.mainTabPage);
            this.tabControl.Controls.Add(this.requestHeadersTabPage);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Size = new System.Drawing.Size(803, 81);
            this.tabControl.TabIndex = 0;
            //tabControl.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            //tabControl.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.TabControl_DrawItem);
            //tabControl.Selected += new System.Windows.Forms.TabControlEventHandler(this.TabControl_Selected);
            // 
            // mainTabPage
            // 
            this.mainTabPage.Controls.Add(this.logPanel);
            this.mainTabPage.Controls.Add(this.buttomPanel);
            this.mainTabPage.Controls.Add(this.mainLayoutPanel);
            this.mainTabPage.Controls.Add(this.topPanel);
            this.mainTabPage.Location = new System.Drawing.Point(4, 22);
            this.mainTabPage.Size = new System.Drawing.Size(288, 431);
            this.mainTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.mainTabPage.TabIndex = 0;
            this.mainTabPage.Text = ".m3u8 file url:";
            this.mainTabPage.UseVisualStyleBackColor = true;
            this.mainTabPage.BackColor = System.Drawing.Color.WhiteSmoke;
            // 
            // requestHeadersTabPage
            // 
            this.requestHeadersTabPage.Controls.Add(this.requestHeadersEditor);
            this.requestHeadersTabPage.Location = new System.Drawing.Point(4, 22);            
            this.requestHeadersTabPage.Size = new System.Drawing.Size(288, 431);
            this.requestHeadersTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.requestHeadersTabPage.TabIndex = 1;
            this.requestHeadersTabPage.Text = "request headers";
            this.requestHeadersTabPage.UseVisualStyleBackColor = true;
            this.requestHeadersTabPage.BackColor = System.Drawing.Color.WhiteSmoke;
            // 
            // requestHeadersEditor
            // 
            this.requestHeadersEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            //this.requestHeadersEditor.Location = new System.Drawing.Point(0, 0);
            //this.requestHeadersEditor.Size = new System.Drawing.Size(800, 409);
            this.requestHeadersEditor.TabIndex = 0;
            this.requestHeadersEditor.OnRequestHeadersCountChanged += new RequestHeadersEditor.RequestHeadersCountChangedEventHandler(this.requestHeadersEditor_OnRequestHeadersCountChanged);
            // 
            // m3u8FileUrlTextBox
            // 
            //this.m3u8FileUrlTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.m3u8FileUrlTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m3u8FileUrlTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.m3u8FileUrlTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.5F);
            this.m3u8FileUrlTextBox.Location = new System.Drawing.Point(0, 0);
            this.m3u8FileUrlTextBox.Multiline = true;
            this.m3u8FileUrlTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.m3u8FileUrlTextBox.Size = new System.Drawing.Size(790, 54);
            this.m3u8FileUrlTextBox.TabIndex = 1;
            this.m3u8FileUrlTextBox.TextChanged += new System.EventHandler(this.m3u8FileUrlTextBox_TextChanged);
            // 
            // outputFileNameTextBox
            // 
            this.outputFileNameTextBox.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            //---this.outputFileNameTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle/*None*/;
            //this.outputFileNameTextBox.BorderColor = System.Drawing.Color.Silver;
            this.outputFileNameTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.5F);
            this.outputFileNameTextBox.Location = new System.Drawing.Point(67, 6);
            this.outputFileNameTextBox.Size = new System.Drawing.Size(539, 18);
            this.outputFileNameTextBox.TabIndex = 1;
            this.outputFileNameTextBox.WordWrap = false;
            this.outputFileNameTextBox.TextChanged += new System.EventHandler(this.outputFileNameTextBox_TextChanged);
            // 
            // outputDirectoryTextBox
            // 
            this.outputDirectoryTextBox.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.outputDirectoryTextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(235)))), ((int)(((byte)(235)))));
            //---this.outputDirectoryTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle/*None*/;
            //this.outputDirectoryTextBox.BorderColor = System.Drawing.Color.Silver;
            this.outputDirectoryTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.5F);
            this.outputDirectoryTextBox.Location = new System.Drawing.Point(67, 66);
            this.outputDirectoryTextBox.Size = new System.Drawing.Size(539, 18);
            this.outputDirectoryTextBox.TabIndex = 5;
            this.outputDirectoryTextBox.WordWrap = false;
            // 
            // outputDirectorySelectButton
            // 
            this.outputDirectorySelectButton.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.outputDirectorySelectButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.outputDirectorySelectButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.outputDirectorySelectButton.Location = new System.Drawing.Point(641, 68);
            this.outputDirectorySelectButton.Size = new System.Drawing.Size(23, 23);
            this.outputDirectorySelectButton.TabIndex = 6;
            this.outputDirectorySelectButton.Text = "≡";
            this.outputDirectorySelectButton.TextAlign = System.Drawing.ContentAlignment.BottomCenter;            
            this.outputDirectorySelectButton.UseCompatibleTextRendering = true;
            this.outputDirectorySelectButton.UseVisualStyleBackColor = true;
            this.outputDirectorySelectButton.Click += new System.EventHandler(this.outputDirectorySelectButton_Click);
            this.toolTip.SetToolTip(this.outputDirectorySelectButton, "select \'output directory\'");
            // 
            // outputFileNameSelectButton
            // 
            this.outputFileNameSelectButton.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.outputFileNameSelectButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.outputFileNameSelectButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.outputFileNameSelectButton.Location = new System.Drawing.Point(641, 3);
            this.outputFileNameSelectButton.Size = new System.Drawing.Size(23, 23);
            this.outputFileNameSelectButton.TabIndex = 3;
            this.outputFileNameSelectButton.Text = "≡";
            this.outputFileNameSelectButton.TextAlign = System.Drawing.ContentAlignment.BottomCenter;            
            this.outputFileNameSelectButton.UseCompatibleTextRendering = true;
            this.outputFileNameSelectButton.UseVisualStyleBackColor = true;
            this.outputFileNameSelectButton.Click += new System.EventHandler(this.outputFileNameSelectButton_Click);
            this.toolTip.SetToolTip(this.outputFileNameSelectButton, "select \'output file name\'");
            // 
            // outputFileNameClearButton
            // 
            this.outputFileNameClearButton.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.outputFileNameClearButton.Cursor = System.Windows.Forms.Cursors.Hand;            
            this.outputFileNameClearButton.Location = new System.Drawing.Point(612, 3);
            this.outputFileNameClearButton.Size = new System.Drawing.Size(23, 23);
            this.outputFileNameClearButton.TabIndex = 2;
            this.outputFileNameClearButton.Text = "X";
            this.outputFileNameClearButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.outputFileNameClearButton.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.outputFileNameClearButton.UseCompatibleTextRendering = true;
            this.outputFileNameClearButton.UseVisualStyleBackColor = true;
            this.outputFileNameClearButton.Click += new System.EventHandler(this.outputFileNameClearButton_Click);
            this.toolTip.SetToolTip(this.outputFileNameClearButton, "clear \'output file name\'");
            // 
            // logPanel
            // 
            this.logPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.logPanel.Controls.Add(this.logUC);
            this.logPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.logPanel.Location = new System.Drawing.Point(0, 171);
            this.logPanel.Size = new System.Drawing.Size(803, 0);
            this.logPanel.TabIndex = 2;
            // 
            // logUC
            // 
            this.logUC.Dock = System.Windows.Forms.DockStyle.Fill;
            this.logUC.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.5F);
            this.logUC.Location = new System.Drawing.Point(0, 0);
            this.logUC.ShowOnlyRequestRowsWithErrors = false;
            this.logUC.ScrollToLastRow = true;
            this.logUC.ShowResponseColumn = true;
            this.logUC.AllowDrawDownloadButtonForM3u8Urls = true;
            this.logUC.DownloadAdditionalM3u8Url += new System.Action<System.Uri>(this.logUC_DownloadAdditionalM3u8Url);
            this.logUC.AllowDownloadAdditionalM3u8Url = new System.Func<string, bool>(this.logUC_AllowDownloadAdditionalM3u8Url);
            this.logUC.Size = new System.Drawing.Size(803, 0);
            this.logUC.TabIndex = 0;
            // 
            // mainLayoutPanel
            // 
            this.mainLayoutPanel.ColumnCount = 7;
            this.mainLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.mainLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute/*System.Windows.Forms.SizeType.Absolute, 29F*/));
            this.mainLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.mainLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            this.mainLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            this.mainLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.mainLayoutPanel.Controls.Add(this.l2, 0, 2);
            this.mainLayoutPanel.Controls.Add(this.outputDirectoryTextBox, 1, 2);
            this.mainLayoutPanel.SetColumnSpan(this.outputDirectoryTextBox, 3);
            this.mainLayoutPanel.Controls.Add(this.outputDirectorySelectButton, 5, 2);
            this.mainLayoutPanel.Controls.Add(this.externalProgApplyByDefaultCheckBox, 1, 3);
            this.mainLayoutPanel.SetColumnSpan(this.externalProgApplyByDefaultCheckBox, 3);
            this.mainLayoutPanel.Controls.Add(this.l1, 0, 0);
            this.mainLayoutPanel.Controls.Add(this.outputFileNameSelectButton, 5, 0);
            this.mainLayoutPanel.Controls.Add(this.outputFileNameTextBox, 1, 0);
            this.mainLayoutPanel.SetColumnSpan(this.outputFileNameTextBox, 3);            
            this.mainLayoutPanel.Controls.Add(this.outputFileNameClearButton, 4, 0);

            this.mainLayoutPanel.Controls.Add(this.patternOutputFileNameLabelCaption, 0, 1);
            this.mainLayoutPanel.Controls.Add(this.patternOutputFileNameLabel, 1, 1);
            this.mainLayoutPanel.Controls.Add(this.patternOutputFileNameNumUpDn, 2, 1);

            this.mainLayoutPanel.Controls.Add(this.liveStreamMaxSizeInMbLabel, 3, 1);
            //this.mainLayoutPanel.Controls.Add(this.liveStreamMaxSizeInMbLabel, 1, 1);
            this.mainLayoutPanel.SetColumnSpan(this.liveStreamMaxSizeInMbLabel, 3);
            this.mainLayoutPanel.Controls.Add(this.liveStreamMaxSizeInMbNumUpDn, 6, 1);
            this.mainLayoutPanel.Controls.Add(this.isLiveStreamCheckBox, 6, 0);

            this.mainLayoutPanel.Controls.Add(this.loadM3u8FileContentButton, 6, 2);
            this.mainLayoutPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.mainLayoutPanel.Location = new System.Drawing.Point(0, 81);
            this.mainLayoutPanel.Padding = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.mainLayoutPanel.RowCount = 4;
            this.mainLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.mainLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize, 30F));
            this.mainLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.mainLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.mainLayoutPanel.Size = new System.Drawing.Size(803, 60 + 20);
            this.mainLayoutPanel.TabIndex = 1;
            // 
            // liveStreamMaxSizeInMbLabel
            // 
            this.liveStreamMaxSizeInMbLabel.AutoSize = true;
            this.liveStreamMaxSizeInMbLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;// | System.Windows.Forms.AnchorStyles.Top;            
            this.liveStreamMaxSizeInMbLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.liveStreamMaxSizeInMbLabel.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.liveStreamMaxSizeInMbLabel.ForeColor = System.Drawing.Color.DimGray;
            //this.liveStreamMaxSizeInMbLabel.Location = new System.Drawing.Point(670, 6);
            //this.liveStreamMaxSizeInMbLabel.Size = new System.Drawing.Size(112, 17);            
            this.liveStreamMaxSizeInMbLabel.MinimumSize = new System.Drawing.Size(100, 17);
            this.liveStreamMaxSizeInMbLabel.AutoEllipsis = true;
            this.liveStreamMaxSizeInMbLabel.TabIndex = 7;
            this.liveStreamMaxSizeInMbLabel.Text = "max single output file size in mb:";
            this.liveStreamMaxSizeInMbLabel.Visible = false;
            // 
            // liveStreamMaxSizeInMbNumUpDn
            // 
            //this.liveStreamMaxSizeInMbNumUpDn.AutoSize = true;
            this.liveStreamMaxSizeInMbNumUpDn.Anchor = System.Windows.Forms.AnchorStyles.Left;// | System.Windows.Forms.AnchorStyles.Top;            
            this.liveStreamMaxSizeInMbNumUpDn.Cursor = System.Windows.Forms.Cursors.Hand;
            this.liveStreamMaxSizeInMbNumUpDn.ForeColor = System.Drawing.Color.DimGray;
            //this.liveStreamMaxSizeInMbNumUpDn.Location = new System.Drawing.Point(670, 6);
            this.liveStreamMaxSizeInMbNumUpDn.Size = new System.Drawing.Size(70, 17);
            this.liveStreamMaxSizeInMbNumUpDn.TabIndex = 7;
            this.liveStreamMaxSizeInMbNumUpDn.ThousandsSeparator = true;
            this.liveStreamMaxSizeInMbNumUpDn.Minimum = 1;
            this.liveStreamMaxSizeInMbNumUpDn.Maximum = int.MaxValue;
            this.liveStreamMaxSizeInMbNumUpDn.Value = 250;
            //---this.liveStreamMaxSizeInMbNumUpDn.Increment = 10;
            this.liveStreamMaxSizeInMbNumUpDn.Set_Increment_MouseWheel( 10 );
            this.liveStreamMaxSizeInMbNumUpDn.Round2NextTenGroup = true;
            this.liveStreamMaxSizeInMbNumUpDn.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.toolTip.SetToolTip(this.liveStreamMaxSizeInMbNumUpDn, "max single output file size in mb for live stream" );
            this.liveStreamMaxSizeInMbNumUpDn.Visible = false;
            // 
            // isLiveStreamCheckBox
            // 
            this.isLiveStreamCheckBox.AutoSize = true;
            this.isLiveStreamCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Left;            
            this.isLiveStreamCheckBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.isLiveStreamCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.isLiveStreamCheckBox.ForeColor = System.Drawing.Color.Silver;
            //this.isLiveStreamCheckBox.Location = new System.Drawing.Point(670, 6);
            //this.isLiveStreamCheckBox.Size = new System.Drawing.Size(112, 17);
            this.isLiveStreamCheckBox.TabIndex = 7;
            this.isLiveStreamCheckBox.Text = "this is a live stream";
            this.isLiveStreamCheckBox.UseVisualStyleBackColor = true;
            this.isLiveStreamCheckBox.Click += new System.EventHandler(this.isLiveStreamCheckBox_Click);

            // 
            // patternOutputFileNameLabelCaption
            // 
            this.patternOutputFileNameLabelCaption.AutoSize = true;
            this.patternOutputFileNameLabelCaption.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.patternOutputFileNameLabelCaption.ForeColor = System.Drawing.Color.DarkBlue;
            //this.patternOutputFileNameLabelCaption.Location = new System.Drawing.Point(6, 2);
            //this.patternOutputFileNameLabelCaption.Size = new System.Drawing.Size(55, 26);
            this.patternOutputFileNameLabelCaption.TabIndex = 0;
            this.patternOutputFileNameLabelCaption.Text = "pattern :";
            this.patternOutputFileNameLabelCaption.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.patternOutputFileNameLabelCaption.Visible = false;
            // 
            // patternOutputFileNameLabel
            // 
            this.patternOutputFileNameLabel.AutoSize = true;
            this.patternOutputFileNameLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;// | System.Windows.Forms.AnchorStyles.Top;            
            this.patternOutputFileNameLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.patternOutputFileNameLabel.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.patternOutputFileNameLabel.ForeColor = System.Drawing.Color.DimGray;
            //this.patternOutputFileNameLabel.Location = new System.Drawing.Point(670, 6);
            //this.patternOutputFileNameLabel.Size = new System.Drawing.Size(112, 17);
            this.patternOutputFileNameLabel.MaximumSize = new System.Drawing.Size(350, 17);
            this.patternOutputFileNameLabel.AutoEllipsis = true;
            this.patternOutputFileNameLabel.TabIndex = 7;
            this.patternOutputFileNameLabel.Text = "...";
            this.patternOutputFileNameLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.patternOutputFileNameLabel.Visible = false;
            this.patternOutputFileNameLabel.VisibleChanged += new System.EventHandler(this.patternOutputFileNameLabel_VisibleChanged);
            // 
            // patternOutputFileNameNumUpDn
            // 
            //this.patternOutputFileNameNumUpDn.AutoSize = true;
            this.patternOutputFileNameNumUpDn.Anchor = System.Windows.Forms.AnchorStyles.Left;// | System.Windows.Forms.AnchorStyles.Top;            
            this.patternOutputFileNameNumUpDn.Cursor = System.Windows.Forms.Cursors.Hand;
            this.patternOutputFileNameNumUpDn.ForeColor = System.Drawing.Color.DimGray;
            //this.patternOutputFileNameNumUpDn.Location = new System.Drawing.Point(670, 6);
            this.patternOutputFileNameNumUpDn.Size = new System.Drawing.Size(70, 17);            
            this.patternOutputFileNameNumUpDn.TabIndex = 7;
            this.patternOutputFileNameNumUpDn.ThousandsSeparator = true;
            this.patternOutputFileNameNumUpDn.Minimum = 1;
            this.patternOutputFileNameNumUpDn.Maximum = int.MaxValue;
            this.patternOutputFileNameNumUpDn.Value = 1;
            this.patternOutputFileNameNumUpDn.Set_Increment_MouseWheel( 1 );
            this.patternOutputFileNameNumUpDn.Round2NextTenGroup = false;
            this.patternOutputFileNameNumUpDn.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;            
            this.patternOutputFileNameNumUpDn.Visible = false;
            this.patternOutputFileNameNumUpDn.ValueChanged += new System.EventHandler(this.patternOutputFileNameNumUpDn_ValueChanged);
            this.toolTip.SetToolTip(this.patternOutputFileNameNumUpDn, "current pattern output file name count-number" );

            // 
            // loadM3u8FileContentButton
            // 
            this.loadM3u8FileContentButton.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.loadM3u8FileContentButton.AutoSize = true;
            this.loadM3u8FileContentButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.loadM3u8FileContentButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.loadM3u8FileContentButton.ForeColor = System.Drawing.Color.FromArgb(70, 70, 70);
            this.loadM3u8FileContentButton.Location = new System.Drawing.Point(670, 63);
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
            this.buttomPanel.Location = new System.Drawing.Point(0, 150);
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
            this.downloadStartButton.ForeColor = System.Drawing.Color.FromArgb(70, 70, 70);
            this.downloadStartButton.Location = new System.Drawing.Point(233, 5);
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
            this.downloadLaterButton.ForeColor = System.Drawing.Color.FromArgb(70, 70, 70);
            this.downloadLaterButton.Location = new System.Drawing.Point(413, 5);
            this.downloadLaterButton.Size = new System.Drawing.Size(159, 23);
            this.downloadLaterButton.TabIndex = 1;
            this.downloadLaterButton.Text = "Download later";
            this.downloadLaterButton.UseVisualStyleBackColor = false;
            // 
            // externalProgApplyByDefaultCheckBox
            // 
            this.externalProgApplyByDefaultCheckBox.AutoSize = true;
            this.externalProgApplyByDefaultCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Left;            
            this.externalProgApplyByDefaultCheckBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.externalProgApplyByDefaultCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.externalProgApplyByDefaultCheckBox.ForeColor = System.Drawing.Color.Silver; //System.Drawing.Color.FromArgb(70, 70, 70); // 
            //this.externalProgApplyByDefaultCheckBox.Location = new System.Drawing.Point(670, 6);
            //this.externalProgApplyByDefaultCheckBox.Size = new System.Drawing.Size(112, 17);
            this.externalProgApplyByDefaultCheckBox.TabIndex = 20;
            this.externalProgApplyByDefaultCheckBox.Text = "External program - Apply to all new downloads by default";
            this.externalProgApplyByDefaultCheckBox.UseVisualStyleBackColor = true;
            this.externalProgApplyByDefaultCheckBox.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.externalProgApplyByDefaultCheckBox.Click += new System.EventHandler(this.externalProgApplyByDefaultCheckBox_Click);
            // 
            // statusBarUC
            // 
            this.statusBarUC.AutoSize = true;
            this.statusBarUC.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.statusBarUC.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.statusBarUC.IsVisibleExcludesWordsLabel = true;
            this.statusBarUC.IsVisibleParallelismLabel = true;
            this.statusBarUC.IsVisibleSettingsLabel = true;
            this.statusBarUC.Location = new System.Drawing.Point(0, 186);
            this.statusBarUC.Margin = new System.Windows.Forms.Padding(0);
            this.statusBarUC.Size = new System.Drawing.Size(803, 39);
            this.statusBarUC.TabIndex = 4;
            // 
            // AddNewDownloadForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(803, 270);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.statusBarUC);
            this.Icon = global::m3u8.download.manager.Properties.Resources.m3u8_32x36;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "add new download";
            this.tabControl.ResumeLayout(false);
            this.mainTabPage.ResumeLayout(false);
            this.requestHeadersTabPage.ResumeLayout(false);
            this.topPanel.ResumeLayout(false);
            this.logPanel.ResumeLayout(false);
            this.mainLayoutPanel.ResumeLayout(false);
            this.buttomPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage mainTabPage;
        private System.Windows.Forms.TabPage requestHeadersTabPage;
        private m3u8.download.manager.ui.RequestHeadersEditor requestHeadersEditor;
        private System.Windows.Forms.Panel topPanel;
        private System.Windows.Forms.TextBox m3u8FileUrlTextBox;
        private m3u8.download.manager.ui.TextBoxWithCustomPathPaste outputFileNameTextBox;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.ButtonWithFocusCues outputFileNameClearButton;
        private System.Windows.Forms.ButtonWithFocusCues loadM3u8FileContentButton;
        private System.Windows.Forms.CheckBox isLiveStreamCheckBox;
        private System.Windows.Forms.Label liveStreamMaxSizeInMbLabel;
        private System.Windows.Forms.NumericUpDownEx liveStreamMaxSizeInMbNumUpDn;
        private System.Windows.Forms.Label patternOutputFileNameLabelCaption;
        private System.Windows.Forms.Label patternOutputFileNameLabel;
        private System.Windows.Forms.NumericUpDownEx patternOutputFileNameNumUpDn;
        private System.Windows.Forms.Panel logPanel;
        private System.Windows.Forms.TableLayoutPanel mainLayoutPanel;
        private System.Windows.Forms.ButtonWithFocusCues outputFileNameSelectButton;
        private System.Windows.Forms.Panel buttomPanel;
        private System.Windows.Forms.ButtonWithFocusCues downloadStartButton;
        private System.Windows.Forms.ButtonWithFocusCues downloadLaterButton;
        private System.Windows.Forms.ButtonWithFocusCues outputDirectorySelectButton;
        private m3u8.download.manager.ui.TextBoxWithCustomPathPaste outputDirectoryTextBox;
        private System.Windows.Forms.CheckBox externalProgApplyByDefaultCheckBox;
        private LogUC logUC;
        private StatusBarUC statusBarUC;
        private System.Windows.Forms.Label l1;
        private System.Windows.Forms.Label l2;
    }
}