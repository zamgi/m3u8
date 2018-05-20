namespace m3u8.downloader
{
    partial class WaitBannerUC
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
            this.components = new System.ComponentModel.Container();
            this.p1 = new System.Windows.Forms.Panel();
            this.speedLabel = new System.Windows.Forms.Label();
            this.progressLabel = new System.Windows.Forms.Label();
            this.elapsedLabel = new System.Windows.Forms.Label();
            this.captionLabel = new System.Windows.Forms.Label();
            this.cancelButton = new System.Windows.Forms.Button();
            this.indicatorPictureBox = new System.Windows.Forms.PictureBox();
            this.substrateLabel = new System.Windows.Forms.Label();
            this.fuskingTimer = new System.Windows.Forms.Timer(this.components);
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.p1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.indicatorPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.p1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.p1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.p1.Controls.Add(this.speedLabel);
            this.p1.Controls.Add(this.progressLabel);
            this.p1.Controls.Add(this.elapsedLabel);
            this.p1.Controls.Add(this.captionLabel);
            this.p1.Controls.Add(this.cancelButton);
            this.p1.Controls.Add(this.indicatorPictureBox);
            this.p1.Controls.Add(this.substrateLabel);
            this.p1.Location = new System.Drawing.Point(3, 3);
            this.p1.Padding = new System.Windows.Forms.Padding(4);
            this.p1.Size = new System.Drawing.Size(255, 110);
            this.p1.TabIndex = 0;
            // 
            // SpeedLabel
            // 
            this.speedLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.speedLabel.AutoEllipsis = true;
            this.speedLabel.BackColor = System.Drawing.Color.Silver;
            this.speedLabel.Location = new System.Drawing.Point(10, 79);
            this.speedLabel.Size = new System.Drawing.Size(80, 13);
            this.speedLabel.TabIndex = 5;
            this.speedLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.toolTip.SetToolTip(this.speedLabel, "cumulative speed");
            this.speedLabel.Visible = false;
            // 
            // Progress
            // 
            this.progressLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressLabel.AutoEllipsis = true;
            this.progressLabel.BackColor = System.Drawing.Color.Silver;
            this.progressLabel.Location = new System.Drawing.Point(82, 35);
            this.progressLabel.Size = new System.Drawing.Size(102, 13);
            this.progressLabel.TabIndex = 3;
            this.progressLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Elapsed
            // 
            this.elapsedLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.elapsedLabel.AutoEllipsis = true;
            this.elapsedLabel.BackColor = System.Drawing.Color.Silver;
            this.elapsedLabel.Location = new System.Drawing.Point(82, 53);
            this.elapsedLabel.Size = new System.Drawing.Size(102, 13);
            this.elapsedLabel.TabIndex = 4;
            this.elapsedLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Caption
            // 
            this.captionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.captionLabel.AutoEllipsis = true;
            this.captionLabel.BackColor = System.Drawing.Color.Silver;
            this.captionLabel.Location = new System.Drawing.Point(82, 17);
            this.captionLabel.Size = new System.Drawing.Size(102, 13);
            this.captionLabel.TabIndex = 2;
            this.captionLabel.Text = "...executing...";
            this.captionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.cancelButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.cancelButton.Location = new System.Drawing.Point(97, 74);
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = "cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // indicatorPictureBox
            // 
            this.indicatorPictureBox.BackColor = System.Drawing.Color.Silver;
            this.indicatorPictureBox.Location = new System.Drawing.Point(206, 43);
            this.indicatorPictureBox.Size = new System.Drawing.Size(16, 16);
            this.indicatorPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.indicatorPictureBox.TabIndex = 1;
            this.indicatorPictureBox.TabStop = false;
            this.indicatorPictureBox.Visible = false;
            // 
            // substrate
            // 
            this.substrateLabel.AutoEllipsis = true;
            this.substrateLabel.BackColor = System.Drawing.Color.Silver;
            this.substrateLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.substrateLabel.Location = new System.Drawing.Point(4, 4);
            this.substrateLabel.Padding = new System.Windows.Forms.Padding(0, 10, 0, 0);
            this.substrateLabel.Size = new System.Drawing.Size(245, 100);
            this.substrateLabel.TabIndex = 0;
            this.substrateLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // fuskingTimer
            // 
            this.fuskingTimer.Interval = 2500;
            this.fuskingTimer.Tick += new System.EventHandler(this.fuskingTimer_Tick);
            // 
            // WaitBannerUC
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.p1);
            this.Size = new System.Drawing.Size(261, 116);
            this.Load += new System.EventHandler(this.WaitBannerUC_Load);
            this.p1.ResumeLayout(false);
            this.p1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.indicatorPictureBox)).EndInit();
            this.ResumeLayout(false);
        }
        #endregion

        private System.Windows.Forms.Label substrateLabel;
        private System.Windows.Forms.Timer fuskingTimer;
        private System.Windows.Forms.PictureBox indicatorPictureBox;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Panel p1;
        private System.Windows.Forms.Label captionLabel;
        private System.Windows.Forms.Label progressLabel;
        private System.Windows.Forms.Label elapsedLabel;
        private System.Windows.Forms.Label speedLabel;
        private System.Windows.Forms.ToolTip toolTip;
    }
}
