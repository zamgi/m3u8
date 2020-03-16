namespace m3u8.download.manager.ui
{
    partial class ChangeOutputFileForm
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
            this.outputFileNameTextBox = new System.Windows.Forms.TextBox();
            this.outputFileNameClearButton = new System.Windows.Forms.ButtonWithFocusCues();
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
            // outputFileNameTextBox
            // 
            this.outputFileNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.outputFileNameTextBox.Location = new System.Drawing.Point(12, 12);
            this.outputFileNameTextBox.Size = new System.Drawing.Size(310, 18);
            this.outputFileNameTextBox.TabIndex = 0;
            this.outputFileNameTextBox.Font = new System.Drawing.Font( "Microsoft Sans Serif", 11.5F );
            this.outputFileNameTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.outputFileNameTextBox.WordWrap = false;
            // 
            // outputFileNameClearButton
            // 
            this.outputFileNameClearButton.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.outputFileNameClearButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.outputFileNameClearButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.outputFileNameClearButton.Location = new System.Drawing.Point(325, 12);
            this.outputFileNameClearButton.Size = new System.Drawing.Size(19, 18);
            this.outputFileNameClearButton.TabIndex = 1;
            this.outputFileNameClearButton.Text = "X";
            this.outputFileNameClearButton.UseVisualStyleBackColor = true;
            this.outputFileNameClearButton.Click += new System.EventHandler(this.outputFileNameClearButton_Click);
            // 
            // ChangeOutputFileForm
            // 
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(345, 91);
            this.Controls.Add(this.outputFileNameClearButton);
            this.Controls.Add(this.outputFileNameTextBox);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "change output file-name";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        #endregion

        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.TextBox outputFileNameTextBox;
        private System.Windows.Forms.ButtonWithFocusCues outputFileNameClearButton;
    }
}