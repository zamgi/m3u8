namespace m3u8.download.manager.ui
{
    partial class ParallelismForm
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
            System.Windows.Forms.GroupBox gb1;
            System.Windows.Forms.GroupBox gb2;
            this.maxDegreeOfParallelismNUD = new System.Windows.Forms.NumericUpDownEx();
            //---this.infinityMaxDegreeOfParallelismCheckBox = new System.Windows.Forms.CheckBox();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.useCrossDownloadInstanceParallelismCheckBox = new System.Windows.Forms.CheckBox();
            this.maxCrossDownloadInstanceNUD = new System.Windows.Forms.NumericUpDownEx();
            this.useMaxCrossDownloadInstanceCheckBox = new System.Windows.Forms.CheckBox();
            this.maxDegreeOfParallelismLabel = new System.Windows.Forms.Label();
            gb1 = new System.Windows.Forms.GroupBox();
            gb2 = new System.Windows.Forms.GroupBox();
            this.maxCrossDownloadInstanceLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.maxDegreeOfParallelismNUD)).BeginInit();
            gb1.SuspendLayout();
            gb2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.maxCrossDownloadInstanceNUD)).BeginInit();
            this.SuspendLayout();
            // 
            // l1
            // 
            this.maxDegreeOfParallelismLabel.AutoSize = true;
            this.maxDegreeOfParallelismLabel.Location = new System.Drawing.Point(41, 23);
            this.maxDegreeOfParallelismLabel.Size = new System.Drawing.Size(128, 13);
            this.maxDegreeOfParallelismLabel.TabIndex = 4;
            this.maxDegreeOfParallelismLabel.Text = "max degree of downloads parallelism:";
            // 
            // maxDegreeOfParallelismNUD
            // 
            this.maxDegreeOfParallelismNUD.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.maxDegreeOfParallelismNUD.Location = new System.Drawing.Point(44, 42);
            this.maxDegreeOfParallelismNUD.Minimum = new decimal( new int[] { 1, 0, 0, 0 } );
            this.maxDegreeOfParallelismNUD.Size = new System.Drawing.Size(89, 16);
            this.maxDegreeOfParallelismNUD.TabIndex = 1;
            this.maxDegreeOfParallelismNUD.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.maxDegreeOfParallelismNUD.Value = new decimal( new int[] { 1, 0, 0, 0 } );
            // 
            // infinityCheckBox
            // 
            //---this.infinityMaxDegreeOfParallelismCheckBox.AutoSize = true;
            //---this.infinityMaxDegreeOfParallelismCheckBox.Cursor = System.Windows.Forms.Cursors.Hand;
            //---this.infinityMaxDegreeOfParallelismCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            //---this.infinityMaxDegreeOfParallelismCheckBox.Location = new System.Drawing.Point(139, 42);
            //---this.infinityMaxDegreeOfParallelismCheckBox.Size = new System.Drawing.Size(54, 17);
            //---this.infinityMaxDegreeOfParallelismCheckBox.TabIndex = 0;
            //---this.infinityMaxDegreeOfParallelismCheckBox.Text = "Infinity";
            //---this.infinityMaxDegreeOfParallelismCheckBox.UseVisualStyleBackColor = true;
            //---this.infinityMaxDegreeOfParallelismCheckBox.CheckedChanged += new System.EventHandler(this.infinityCheckBox_CheckedChanged);
            // 
            // okButton
            // 
            this.okButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.okButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.okButton.Location = new System.Drawing.Point(58, 238);
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 2;
            this.okButton.Text = "Ok";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.cancelButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.cancelButton.Location = new System.Drawing.Point(139, 238);
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 3;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // useCrossDownloadInstanceParallelismCheckBox
            // 
            this.useCrossDownloadInstanceParallelismCheckBox.AutoSize = true;
            this.useCrossDownloadInstanceParallelismCheckBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.useCrossDownloadInstanceParallelismCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.useCrossDownloadInstanceParallelismCheckBox.Location = new System.Drawing.Point(44, 68);
            this.useCrossDownloadInstanceParallelismCheckBox.Size = new System.Drawing.Size(166, 30);
            this.useCrossDownloadInstanceParallelismCheckBox.TabIndex = 5;
            this.useCrossDownloadInstanceParallelismCheckBox.Text = "use cross downloads-instance\r\ndegree of parallelism";
            this.useCrossDownloadInstanceParallelismCheckBox.UseVisualStyleBackColor = true;
            // 
            // gb1
            // 
            gb1.Controls.Add(this.useCrossDownloadInstanceParallelismCheckBox);
            gb1.Controls.Add(this.maxDegreeOfParallelismLabel);
            gb1.Controls.Add(this.maxDegreeOfParallelismNUD);
            //---gb1.Controls.Add(this.infinityMaxDegreeOfParallelismCheckBox);
            gb1.Location = new System.Drawing.Point(14, 9);
            gb1.Size = new System.Drawing.Size(246, 110);
            gb1.TabIndex = 6;
            gb1.TabStop = false;
            gb1.Text = "download threads";
            // 
            // gb2
            // 
            gb2.Controls.Add(this.useMaxCrossDownloadInstanceCheckBox);
            gb2.Controls.Add(this.maxCrossDownloadInstanceLabel);
            gb2.Controls.Add(this.maxCrossDownloadInstanceNUD);
            gb2.Location = new System.Drawing.Point(14, 125);
            gb2.Size = new System.Drawing.Size(246, 92);
            gb2.TabIndex = 7;
            gb2.TabStop = false;
            // 
            // maxCrossDownloadInstanceLabel
            // 
            this.maxCrossDownloadInstanceLabel.AutoSize = true;
            this.maxCrossDownloadInstanceLabel.Location = new System.Drawing.Point(41, 23);
            this.maxCrossDownloadInstanceLabel.Size = new System.Drawing.Size(149, 26);
            this.maxCrossDownloadInstanceLabel.TabIndex = 4;
            this.maxCrossDownloadInstanceLabel.Text = "max count of downloads-instance for\r\nsimultaneously downloading data:";
            // 
            // maxCrossDownloadInstanceNUD
            // 
            this.maxCrossDownloadInstanceNUD.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.maxCrossDownloadInstanceNUD.Location = new System.Drawing.Point(44, 55);
            this.maxCrossDownloadInstanceNUD.Minimum = new decimal( new int[] { 1, 0, 0, 0 } );
            this.maxCrossDownloadInstanceNUD.Size = new System.Drawing.Size(89, 16);
            this.maxCrossDownloadInstanceNUD.TabIndex = 1;
            this.maxCrossDownloadInstanceNUD.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.maxCrossDownloadInstanceNUD.Value = new decimal( new int[] { 1, 0, 0, 0 } );
            // 
            // useMaxCrossDownloadInstanceCheckBox
            // 
            this.useMaxCrossDownloadInstanceCheckBox.AutoSize = true;
            this.useMaxCrossDownloadInstanceCheckBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.useMaxCrossDownloadInstanceCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.useMaxCrossDownloadInstanceCheckBox.Location = new System.Drawing.Point(16, 0);
            this.useMaxCrossDownloadInstanceCheckBox.Size = new System.Drawing.Size(85, 17);
            this.useMaxCrossDownloadInstanceCheckBox.TabIndex = 5;
            this.useMaxCrossDownloadInstanceCheckBox.Text = "use downloads-instance";
            this.useMaxCrossDownloadInstanceCheckBox.UseVisualStyleBackColor = true;
            this.useMaxCrossDownloadInstanceCheckBox.CheckedChanged += new System.EventHandler(this.useMaxCrossDownloadInstanceCheckBox_CheckedChanged);
            // 
            // ParallelismForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(272, 273);
            this.Controls.Add(gb2);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(gb1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "parallelism";
            ((System.ComponentModel.ISupportInitialize)(this.maxDegreeOfParallelismNUD)).EndInit();
            gb1.ResumeLayout(false);
            gb1.PerformLayout();
            gb2.ResumeLayout(false);
            gb2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.maxCrossDownloadInstanceNUD)).EndInit();
            this.ResumeLayout(false);
        }
        #endregion

        private System.Windows.Forms.NumericUpDownEx maxDegreeOfParallelismNUD;
        private System.Windows.Forms.Label maxDegreeOfParallelismLabel;
        //---private System.Windows.Forms.CheckBox infinityMaxDegreeOfParallelismCheckBox;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.CheckBox useCrossDownloadInstanceParallelismCheckBox;
        private System.Windows.Forms.Label maxCrossDownloadInstanceLabel;
        private System.Windows.Forms.NumericUpDownEx maxCrossDownloadInstanceNUD;
        private System.Windows.Forms.CheckBox useMaxCrossDownloadInstanceCheckBox;
    }
}