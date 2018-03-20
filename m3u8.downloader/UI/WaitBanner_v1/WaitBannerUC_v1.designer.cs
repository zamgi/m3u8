namespace m3u8.downloader
{
    partial class WaitBannerUC_v1
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.SpeedLabel = new System.Windows.Forms.Label();
            this.Progress = new System.Windows.Forms.Label();
            this.Elapsed = new System.Windows.Forms.Label();
            this.Caption = new System.Windows.Forms.Label();
            this.cancelButton = new System.Windows.Forms.Button();
            this.indicatorPictureBox = new System.Windows.Forms.PictureBox();
            this.substrate = new System.Windows.Forms.Label();
            this.fuskingTimer = new System.Windows.Forms.Timer(this.components);
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.indicatorPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.SpeedLabel);
            this.panel1.Controls.Add(this.Progress);
            this.panel1.Controls.Add(this.Elapsed);
            this.panel1.Controls.Add(this.Caption);
            this.panel1.Controls.Add(this.cancelButton);
            this.panel1.Controls.Add(this.indicatorPictureBox);
            this.panel1.Controls.Add(this.substrate);
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(4);
            this.panel1.Size = new System.Drawing.Size(255, 110);
            this.panel1.TabIndex = 0;
            // 
            // SpeedLabel
            // 
            this.SpeedLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SpeedLabel.AutoEllipsis = true;
            this.SpeedLabel.BackColor = System.Drawing.Color.Silver;
            this.SpeedLabel.Location = new System.Drawing.Point(10, 79);
            this.SpeedLabel.Name = "SpeedLabel";
            this.SpeedLabel.Size = new System.Drawing.Size(80, 13);
            this.SpeedLabel.TabIndex = 5;
            this.SpeedLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.toolTip.SetToolTip(this.SpeedLabel, "cumulative speed");
            this.SpeedLabel.Visible = false;
            // 
            // Progress
            // 
            this.Progress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Progress.AutoEllipsis = true;
            this.Progress.BackColor = System.Drawing.Color.Silver;
            this.Progress.Location = new System.Drawing.Point(82, 35);
            this.Progress.Name = "Progress";
            this.Progress.Size = new System.Drawing.Size(102, 13);
            this.Progress.TabIndex = 3;
            this.Progress.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Elapsed
            // 
            this.Elapsed.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Elapsed.AutoEllipsis = true;
            this.Elapsed.BackColor = System.Drawing.Color.Silver;
            this.Elapsed.Location = new System.Drawing.Point(82, 53);
            this.Elapsed.Name = "Elapsed";
            this.Elapsed.Size = new System.Drawing.Size(102, 13);
            this.Elapsed.TabIndex = 4;
            this.Elapsed.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Caption
            // 
            this.Caption.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Caption.AutoEllipsis = true;
            this.Caption.BackColor = System.Drawing.Color.Silver;
            this.Caption.Location = new System.Drawing.Point(82, 17);
            this.Caption.Name = "Caption";
            this.Caption.Size = new System.Drawing.Size(102, 13);
            this.Caption.TabIndex = 2;
            this.Caption.Text = "...executing...";
            this.Caption.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.cancelButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cancelButton.Location = new System.Drawing.Point(97, 74);
            this.cancelButton.Name = "cancelButton";
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
            this.indicatorPictureBox.Name = "indicatorPictureBox";
            this.indicatorPictureBox.Size = new System.Drawing.Size(16, 16);
            this.indicatorPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.indicatorPictureBox.TabIndex = 1;
            this.indicatorPictureBox.TabStop = false;
            this.indicatorPictureBox.Visible = false;
            // 
            // substrate
            // 
            this.substrate.AutoEllipsis = true;
            this.substrate.BackColor = System.Drawing.Color.Silver;
            this.substrate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.substrate.Location = new System.Drawing.Point(4, 4);
            this.substrate.Name = "substrate";
            this.substrate.Padding = new System.Windows.Forms.Padding(0, 10, 0, 0);
            this.substrate.Size = new System.Drawing.Size(245, 100);
            this.substrate.TabIndex = 0;
            this.substrate.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // fuskingTimer
            // 
            this.fuskingTimer.Interval = 2500;
            this.fuskingTimer.Tick += new System.EventHandler(this.fuskingTimer_Tick);
            // 
            // WaitBannerUC_v1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.panel1);
            this.Name = "WaitBannerUC_v1";
            this.Size = new System.Drawing.Size(261, 116);
            this.Load += new System.EventHandler(this.WaitBannerUC_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.indicatorPictureBox)).EndInit();
            this.ResumeLayout(false);
        }
        #endregion

        private System.Windows.Forms.Label substrate;
        private System.Windows.Forms.Timer fuskingTimer;
        private System.Windows.Forms.PictureBox indicatorPictureBox;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label Caption;
        private System.Windows.Forms.Label Progress;
        private System.Windows.Forms.Label Elapsed;
        private System.Windows.Forms.Label SpeedLabel;
        private System.Windows.Forms.ToolTip toolTip;
    }
}
