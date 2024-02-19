namespace m3u8.download.manager.ui
{
    partial class NumericUpDownUC
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
            //this.speedThresholdNumericUpDownEx = new System.Windows.Forms.NumericUpDownEx_Transparent();
            this.numericUpDownEx = new System.Windows.Forms.NumericUpDownEx();
            this.captionLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownEx)).BeginInit();
            this.SuspendLayout();
            // 
            // captionLabel
            // 
            this.captionLabel.AutoSize = true;
            this.captionLabel.Location = new System.Drawing.Point(61, 3);
            this.captionLabel.Size = new System.Drawing.Size(33, 13);
            this.captionLabel.TabIndex = 1;
            //---this.captionLabel.Text = "Mbps";
            //this.captionLabel.BackColor = System.Drawing.Color.Transparent;
            this.captionLabel.Click += new System.EventHandler(this.UC_Click);
            // 
            // numericUpDownEx
            // 
            this.numericUpDownEx.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.numericUpDownEx.Location = new System.Drawing.Point(3, 3);
            this.numericUpDownEx.Size = new System.Drawing.Size( 55, 16 );
            this.numericUpDownEx.TabIndex = 0;
            this.numericUpDownEx.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numericUpDownEx.KeyDown += new System.Windows.Forms.KeyEventHandler(this.numericUpDownEx_KeyDown);
            this.numericUpDownEx.GotFocus += new System.EventHandler(this.numericUpDownEx_GotFocus);
            this.numericUpDownEx.Minimum = 1;
            this.numericUpDownEx.Maximum = 1_000_000;
            this.numericUpDownEx.Value = 1;
            //---this.numericUpDownEx.ReadOnly = true;

            // 
            // NumericUpDownUC
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            //this.AutoSize = true;
            //this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.TabStop = true;
            this.Click += new System.EventHandler(this.UC_Click);
            this.Controls.Add(this.captionLabel);
            this.Controls.Add(this.numericUpDownEx);
            this.Size = new System.Drawing.Size(97, 22);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownEx)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        #endregion

        private System.Windows.Forms.Label captionLabel;
        //private System.Windows.Forms.NumericUpDownEx_Transparent numericUpDownEx;
        private System.Windows.Forms.NumericUpDownEx numericUpDownEx;
    }
}