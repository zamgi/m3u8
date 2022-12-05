namespace m3u8.download.manager.ui
{
    partial class ChangeLiveStreamMaxFileSizeForm
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
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.liveStreamMaxSizeInMbLabel = new System.Windows.Forms.Label();
            this.liveStreamMaxSizeInMbNumUpDn = new System.Windows.Forms.NumericUpDownEx();
            this.SuspendLayout();
            // 
            // okButton
            // 
            this.okButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(94, 56);
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 2;
            this.okButton.Text = "Ok";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(175, 56);
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 3;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // liveStreamMaxSizeInMbLabel
            // 
            this.liveStreamMaxSizeInMbLabel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.liveStreamMaxSizeInMbLabel.AutoSize = true;
            this.liveStreamMaxSizeInMbLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.liveStreamMaxSizeInMbLabel.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.liveStreamMaxSizeInMbLabel.ForeColor = System.Drawing.Color.DimGray;
            this.liveStreamMaxSizeInMbLabel.Location = new System.Drawing.Point( 50, 15 );
            this.liveStreamMaxSizeInMbLabel.Size = new System.Drawing.Size( 112, 17 );
            this.liveStreamMaxSizeInMbLabel.TabIndex = 7;
            this.liveStreamMaxSizeInMbLabel.Text = "max single output file size in mb:";
            // 
            // liveStreamMaxSizeInMbNumUpDn
            // 
            //this.liveStreamMaxSizeInMbNumUpDn.AutoSize = true;
            this.liveStreamMaxSizeInMbNumUpDn.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.liveStreamMaxSizeInMbNumUpDn.Cursor = System.Windows.Forms.Cursors.Hand;
            this.liveStreamMaxSizeInMbNumUpDn.ForeColor = System.Drawing.Color.DimGray;
            this.liveStreamMaxSizeInMbNumUpDn.Location = new System.Drawing.Point( 210, 15 );
            this.liveStreamMaxSizeInMbNumUpDn.Size = new System.Drawing.Size( 70, 17 );
            this.liveStreamMaxSizeInMbNumUpDn.TabIndex = 7;
            this.liveStreamMaxSizeInMbNumUpDn.ThousandsSeparator = true;
            this.liveStreamMaxSizeInMbNumUpDn.Minimum = 1;
            this.liveStreamMaxSizeInMbNumUpDn.Maximum = int.MaxValue;
            this.liveStreamMaxSizeInMbNumUpDn.Value = 250;
            this.liveStreamMaxSizeInMbNumUpDn.Increment_MouseWheel = 10;
            this.liveStreamMaxSizeInMbNumUpDn.Round2NextTenGroup = true;
            //---this.liveStreamMaxSizeInMbNumUpDn.Increment = 10;
            this.liveStreamMaxSizeInMbNumUpDn.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // ChangeLiveStreamMaxFileSizeForm
            // 
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(345, 91);
            this.Controls.Add(this.liveStreamMaxSizeInMbLabel);
            this.Controls.Add(this.liveStreamMaxSizeInMbNumUpDn);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "change max single output file size in mb for live stream";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        #endregion

        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Label liveStreamMaxSizeInMbLabel;
        private System.Windows.Forms.NumericUpDownEx liveStreamMaxSizeInMbNumUpDn;
    }
}