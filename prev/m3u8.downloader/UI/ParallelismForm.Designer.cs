namespace m3u8.downloader
{
    partial class ParallelismForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose( bool disposing )
        {
            if ( disposing && (components != null) )
            {
                components.Dispose();
            }
            base.Dispose( disposing );
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.Label l1;
            System.Windows.Forms.GroupBox gb1;
            System.Windows.Forms.GroupBox gb2;
            this.maxDegreeOfParallelismNUD = new System.Windows.Forms.NumericUpDown();
            this.infinityCheckBox = new System.Windows.Forms.CheckBox();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.useCrossAppInstanceDegreeOfParallelismCheckBox = new System.Windows.Forms.CheckBox();
            this.maxDownloadAppInstanceNUD = new System.Windows.Forms.NumericUpDown();
            this.maxDownloadAppInstanceCheckBox = new System.Windows.Forms.CheckBox();
            l1 = new System.Windows.Forms.Label();
            gb1 = new System.Windows.Forms.GroupBox();
            gb2 = new System.Windows.Forms.GroupBox();
            this.maxDownloadAppInstanceLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.maxDegreeOfParallelismNUD)).BeginInit();
            gb1.SuspendLayout();
            gb2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.maxDownloadAppInstanceNUD)).BeginInit();
            this.SuspendLayout();
            // 
            // l1
            // 
            l1.AutoSize = true;
            l1.Location = new System.Drawing.Point(41, 23);
            l1.Size = new System.Drawing.Size(128, 13);
            l1.TabIndex = 4;
            l1.Text = "max degree of parallelism:";
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
            this.infinityCheckBox.AutoSize = true;
            this.infinityCheckBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.infinityCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.infinityCheckBox.Location = new System.Drawing.Point(139, 42);
            this.infinityCheckBox.Size = new System.Drawing.Size(54, 17);
            this.infinityCheckBox.TabIndex = 0;
            this.infinityCheckBox.Text = "Infinity";
            this.infinityCheckBox.UseVisualStyleBackColor = true;
            this.infinityCheckBox.CheckedChanged += new System.EventHandler(this.infinityCheckBox_CheckedChanged);
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
            // useCrossAppInstanceDegreeOfParallelismCheckBox
            // 
            this.useCrossAppInstanceDegreeOfParallelismCheckBox.AutoSize = true;
            this.useCrossAppInstanceDegreeOfParallelismCheckBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.useCrossAppInstanceDegreeOfParallelismCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.useCrossAppInstanceDegreeOfParallelismCheckBox.Location = new System.Drawing.Point(44, 68);
            this.useCrossAppInstanceDegreeOfParallelismCheckBox.Size = new System.Drawing.Size(166, 30);
            this.useCrossAppInstanceDegreeOfParallelismCheckBox.TabIndex = 5;
            this.useCrossAppInstanceDegreeOfParallelismCheckBox.Text = "use cross app-instance\r\ndegree of parallelism";
            this.useCrossAppInstanceDegreeOfParallelismCheckBox.UseVisualStyleBackColor = true;
            // 
            // gb1
            // 
            gb1.Controls.Add(this.useCrossAppInstanceDegreeOfParallelismCheckBox);
            gb1.Controls.Add(l1);
            gb1.Controls.Add(this.maxDegreeOfParallelismNUD);
            gb1.Controls.Add(this.infinityCheckBox);
            gb1.Location = new System.Drawing.Point(14, 9);
            gb1.Size = new System.Drawing.Size(246, 110);
            gb1.TabIndex = 6;
            gb1.TabStop = false;
            gb1.Text = "download threads";
            // 
            // gb2
            // 
            gb2.Controls.Add(this.maxDownloadAppInstanceCheckBox);
            gb2.Controls.Add(this.maxDownloadAppInstanceLabel);
            gb2.Controls.Add(this.maxDownloadAppInstanceNUD);
            gb2.Location = new System.Drawing.Point(14, 125);
            gb2.Size = new System.Drawing.Size(246, 92);
            gb2.TabIndex = 7;
            gb2.TabStop = false;
            // 
            // maxDownloadAppInstanceLabel
            // 
            this.maxDownloadAppInstanceLabel.AutoSize = true;
            this.maxDownloadAppInstanceLabel.Location = new System.Drawing.Point(41, 23);
            this.maxDownloadAppInstanceLabel.Size = new System.Drawing.Size(149, 26);
            this.maxDownloadAppInstanceLabel.TabIndex = 4;
            this.maxDownloadAppInstanceLabel.Text = "max count of app-instance for\r\nsimultaneously downloading data:";
            // 
            // maxDownloadAppInstanceNUD
            // 
            this.maxDownloadAppInstanceNUD.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.maxDownloadAppInstanceNUD.Location = new System.Drawing.Point(44, 55);
            this.maxDownloadAppInstanceNUD.Minimum = new decimal( new int[] { 1, 0, 0, 0 } );
            this.maxDownloadAppInstanceNUD.Size = new System.Drawing.Size(89, 16);
            this.maxDownloadAppInstanceNUD.TabIndex = 1;
            this.maxDownloadAppInstanceNUD.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.maxDownloadAppInstanceNUD.Value = new decimal( new int[] { 1, 0, 0, 0 } );
            // 
            // maxDownloadAppInstanceCheckBox
            // 
            this.maxDownloadAppInstanceCheckBox.AutoSize = true;
            this.maxDownloadAppInstanceCheckBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.maxDownloadAppInstanceCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.maxDownloadAppInstanceCheckBox.Location = new System.Drawing.Point(16, 0);
            this.maxDownloadAppInstanceCheckBox.Size = new System.Drawing.Size(85, 17);
            this.maxDownloadAppInstanceCheckBox.TabIndex = 5;
            this.maxDownloadAppInstanceCheckBox.Text = "use app-instance";
            this.maxDownloadAppInstanceCheckBox.UseVisualStyleBackColor = true;
            this.maxDownloadAppInstanceCheckBox.CheckedChanged += new System.EventHandler(this.maxDownloadAppInstanceCheckBox_CheckedChanged);
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
            ((System.ComponentModel.ISupportInitialize)(this.maxDownloadAppInstanceNUD)).EndInit();
            this.ResumeLayout(false);
        }
        #endregion

        private System.Windows.Forms.NumericUpDown maxDegreeOfParallelismNUD;
        private System.Windows.Forms.CheckBox infinityCheckBox;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.CheckBox useCrossAppInstanceDegreeOfParallelismCheckBox;
        private System.Windows.Forms.Label maxDownloadAppInstanceLabel;
        private System.Windows.Forms.NumericUpDown maxDownloadAppInstanceNUD;
        private System.Windows.Forms.CheckBox maxDownloadAppInstanceCheckBox;
    }
}