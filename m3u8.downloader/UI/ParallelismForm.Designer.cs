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
            this.numericUpDown = new System.Windows.Forms.NumericUpDown();
            this.infinityCheckBox = new System.Windows.Forms.CheckBox();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.useCrossAppInstanceDegreeOfParallelismCheckBox = new System.Windows.Forms.CheckBox();
            l1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // numericUpDown
            // 
            this.numericUpDown.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.numericUpDown.Location = new System.Drawing.Point(56, 38);
            this.numericUpDown.Minimum = new decimal( new int[] { 1, 0, 0, 0 } );
            this.numericUpDown.Size = new System.Drawing.Size(89, 16);
            this.numericUpDown.TabIndex = 1;
            this.numericUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numericUpDown.Value = new decimal( new int[] { 1, 0, 0, 0 } );
            // 
            // infinityCheckBox
            // 
            this.infinityCheckBox.AutoSize = true;
            this.infinityCheckBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.infinityCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.infinityCheckBox.Location = new System.Drawing.Point(151, 38);
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
            this.okButton.Location = new System.Drawing.Point(58, 130);
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
            this.cancelButton.Location = new System.Drawing.Point(139, 130);
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 3;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // l1
            // 
            l1.AutoSize = true;
            l1.Location = new System.Drawing.Point(53, 9);
            l1.Size = new System.Drawing.Size(128, 13);
            l1.TabIndex = 4;
            l1.Text = "max degree of parallelism:";
            // 
            // useCrossAppInstanceDegreeOfParallelismCheckBox
            // 
            this.useCrossAppInstanceDegreeOfParallelismCheckBox.AutoSize = true;
            this.useCrossAppInstanceDegreeOfParallelismCheckBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.useCrossAppInstanceDegreeOfParallelismCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.useCrossAppInstanceDegreeOfParallelismCheckBox.Location = new System.Drawing.Point(56, 71);
            this.useCrossAppInstanceDegreeOfParallelismCheckBox.Size = new System.Drawing.Size(166, 30);
            this.useCrossAppInstanceDegreeOfParallelismCheckBox.TabIndex = 5;
            this.useCrossAppInstanceDegreeOfParallelismCheckBox.Text = "use cross application instance\r\ndegree of parallelism";
            this.useCrossAppInstanceDegreeOfParallelismCheckBox.UseVisualStyleBackColor = true;
            // 
            // ParallelismForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(272, 165);
            this.Controls.Add(this.useCrossAppInstanceDegreeOfParallelismCheckBox);
            this.Controls.Add(l1);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.infinityCheckBox);
            this.Controls.Add(this.numericUpDown);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "parallelism";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        #endregion

        private System.Windows.Forms.NumericUpDown numericUpDown;
        private System.Windows.Forms.CheckBox infinityCheckBox;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.CheckBox useCrossAppInstanceDegreeOfParallelismCheckBox;
    }
}