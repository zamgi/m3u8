namespace m3u8.download.manager.ui
{
    partial class MoreSettingsUC
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
            this.gb1 = new System.Windows.Forms.GroupBox();
            this.collectGarbageButton = new System.Windows.Forms.Button();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.currentMemoryLabel = new System.Windows.Forms.Label();
            this.gb1.SuspendLayout();
            this.SuspendLayout();
            // 
            // gb1
            // 
            this.gb1.Controls.Add(this.currentMemoryLabel);
            this.gb1.Controls.Add(this.collectGarbageButton);
            this.gb1.Location = new System.Drawing.Point(13, 7);
            this.gb1.Size = new System.Drawing.Size(261, 76);
            this.gb1.TabIndex = 0;
            this.gb1.TabStop = false;
            this.gb1.Text = "GC";
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
            // MoreSettingsUC
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gb1);
            this.Size = new System.Drawing.Size(286, 470);
            this.gb1.ResumeLayout(false);
            this.ResumeLayout(false);
        }
        #endregion

        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.GroupBox gb1;
        private System.Windows.Forms.Button collectGarbageButton;
        private System.Windows.Forms.Label currentMemoryLabel;
    }
}