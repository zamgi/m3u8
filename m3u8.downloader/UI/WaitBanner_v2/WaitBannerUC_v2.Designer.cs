namespace m3u8.downloader
{
    partial class WaitBannerUC_v2
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
            PanelEx p1; //System.Windows.Forms.Panel p1;
            PanelEx p2; //System.Windows.Forms.Panel p2;
            this.cancelButton = new System.Windows.Forms.Button();
            this.indicatorPictureBox = new System.Windows.Forms.PictureBox();
            this.captionLabel = new System.Windows.Forms.Label();
            this.fuskingTimer = new System.Windows.Forms.Timer(this.components);
            p1 = new PanelEx(); // p1 = new System.Windows.Forms.Panel();
            p2 = new PanelEx(); // p2 = new System.Windows.Forms.Panel();
            p2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.indicatorPictureBox)).BeginInit();
            p1.SuspendLayout();
            this.SuspendLayout();
            // 
            // p1
            // 
            p1.Anchor = System.Windows.Forms.AnchorStyles.None;
            p1.BackColor = System.Drawing.Color.White;
            //---p1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            p1.Controls.Add( p2 );
            p1.Location = new System.Drawing.Point( 1, 1 );
            p1.Margin = new System.Windows.Forms.Padding( 0 );
            p1.Size = new System.Drawing.Size( 263, 93 );
            p1.TabIndex = 2;
            // 
            // p2
            // 
            p2.Anchor = System.Windows.Forms.AnchorStyles.None;
            p2.BackColor = System.Drawing.Color.White;
            //---p2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            p2.Controls.Add(this.cancelButton);
            p2.Controls.Add(this.indicatorPictureBox);
            p2.Controls.Add(this.captionLabel);
            p2.Location = new System.Drawing.Point(4, 4);
            p2.Padding = new System.Windows.Forms.Padding(4);
            p2.Size = new System.Drawing.Size(255, 85);
            p2.TabIndex = 0;
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.cancelButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cancelButton.Location = new System.Drawing.Point(97, 49);
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = "cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // indicatorPictureBox
            // 
            this.indicatorPictureBox.BackColor = System.Drawing.Color.Silver;
            this.indicatorPictureBox.Location = new System.Drawing.Point(206, 12);
            this.indicatorPictureBox.Size = new System.Drawing.Size(16, 16);
            this.indicatorPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.indicatorPictureBox.TabIndex = 1;
            this.indicatorPictureBox.TabStop = false;
            this.indicatorPictureBox.Visible = false;
            // 
            // captionLabel
            // 
            this.captionLabel.AutoEllipsis = true;
            this.captionLabel.BackColor = System.Drawing.Color.Silver;
            this.captionLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.captionLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.captionLabel.Location = new System.Drawing.Point(4, 4);
            this.captionLabel.Padding = new System.Windows.Forms.Padding(0, 10, 0, 0);
            this.captionLabel.Size = new System.Drawing.Size(245, 75);
            this.captionLabel.TabIndex = 0;
            this.captionLabel.Text = "...executing...";
            this.captionLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
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
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(p1);
            this.Size = new System.Drawing.Size(265, 95);
            this.Load += new System.EventHandler(this.WaitBannerUC_Load);
            p2.ResumeLayout(false);
            p2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.indicatorPictureBox)).EndInit();
            p1.ResumeLayout(false);
            this.ResumeLayout(false);
        }
        #endregion

        private System.Windows.Forms.Label captionLabel;
        private System.Windows.Forms.Timer fuskingTimer;
        private System.Windows.Forms.PictureBox indicatorPictureBox;
        private System.Windows.Forms.Button cancelButton;        
    }
}
