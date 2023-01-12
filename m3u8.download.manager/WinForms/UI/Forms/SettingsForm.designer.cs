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
            System.Windows.Forms.TabControl tabControl;
            System.Windows.Forms.Panel bottomPanel;
            this.parallelismTabPage = new System.Windows.Forms.TabPage();
            this.parallelismSettingsUC = new m3u8.download.manager.ui.ParallelismSettingsUC();
            this.otherTabPage = new System.Windows.Forms.TabPage();
            this.otherSettingsUC = new m3u8.download.manager.ui.OtherSettingsUC();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            tabControl = new System.Windows.Forms.TabControl();
            bottomPanel = new System.Windows.Forms.Panel();
            tabControl.SuspendLayout();
            this.parallelismTabPage.SuspendLayout();
            this.otherTabPage.SuspendLayout();
            bottomPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            tabControl.Controls.Add(this.parallelismTabPage);
            tabControl.Controls.Add(this.otherTabPage);
            tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            tabControl.Location = new System.Drawing.Point(0, 0);
            tabControl.Size = new System.Drawing.Size(296, 457);
            tabControl.TabIndex = 0;
            tabControl.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            tabControl.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.TabControl_DrawItem);
            // 
            // parallelismTabPage
            // 
            this.parallelismTabPage.Controls.Add(this.parallelismSettingsUC);
            this.parallelismTabPage.Location = new System.Drawing.Point(4, 22);
            this.parallelismTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.parallelismTabPage.Size = new System.Drawing.Size(288, 431);
            this.parallelismTabPage.TabIndex = 0;
            this.parallelismTabPage.Text = "parallelism";
            this.parallelismTabPage.UseVisualStyleBackColor = true;
            this.parallelismTabPage.BackColor = System.Drawing.Color.WhiteSmoke;
            // 
            // parallelismSettingsUC
            // 
            this.parallelismSettingsUC.Dock = System.Windows.Forms.DockStyle.Fill;
            this.parallelismSettingsUC.Location = new System.Drawing.Point(3, 3);
            this.parallelismSettingsUC.Size = new System.Drawing.Size(282, 425);
            this.parallelismSettingsUC.TabIndex = 0;
            // 
            // otherTabPage
            // 
            this.otherTabPage.Controls.Add(this.otherSettingsUC);
            this.otherTabPage.Location = new System.Drawing.Point(4, 22);
            this.otherTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.otherTabPage.Size = new System.Drawing.Size(288, 431);
            this.otherTabPage.TabIndex = 1;
            this.otherTabPage.Text = "other";
            this.otherTabPage.UseVisualStyleBackColor = true;
            this.otherTabPage.BackColor = System.Drawing.Color.WhiteSmoke;
            // 
            // otherSettingsUC
            // 
            this.otherSettingsUC.Dock = System.Windows.Forms.DockStyle.Fill;
            this.otherSettingsUC.Location = new System.Drawing.Point(3, 3);
            this.otherSettingsUC.Size = new System.Drawing.Size(282, 425);
            this.otherSettingsUC.TabIndex = 0;
            // 
            // bottomPanel
            // 
            bottomPanel.Controls.Add(this.cancelButton);
            bottomPanel.Controls.Add(this.okButton);
            bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            bottomPanel.Location = new System.Drawing.Point(0, 457);
            bottomPanel.Size = new System.Drawing.Size(296, 38);
            bottomPanel.TabIndex = 1;
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.cancelButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.cancelButton.Location = new System.Drawing.Point(151, 7);
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // okButton
            // 
            this.okButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.okButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.okButton.Location = new System.Drawing.Point(70, 7);
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 0;
            this.okButton.Text = "Ok";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // SettingsForm2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(300, 555);
            this.Controls.Add(tabControl);
            this.Controls.Add(bottomPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "settings";
            tabControl.ResumeLayout(false);
            this.parallelismTabPage.ResumeLayout(false);
            this.otherTabPage.ResumeLayout(false);
            bottomPanel.ResumeLayout(false);
            this.ResumeLayout(false);
        }
        #endregion

        private m3u8.download.manager.ui.ParallelismSettingsUC parallelismSettingsUC;
        private m3u8.download.manager.ui.OtherSettingsUC otherSettingsUC;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.TabPage parallelismTabPage;
        private System.Windows.Forms.TabPage otherTabPage;
    }
}