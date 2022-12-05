namespace m3u8.download.manager.ui
{
    partial class SpeedThresholdUC
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
            this.speedThresholdNumericUpDownEx = new System.Windows.Forms.NumericUpDownEx();
            this.l1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.speedThresholdNumericUpDownEx)).BeginInit();
            this.SuspendLayout();
            // 
            // l1
            // 
            this.l1.AutoSize = true;
            this.l1.Location = new System.Drawing.Point(61, 3);
            this.l1.Size = new System.Drawing.Size(33, 13);
            this.l1.TabIndex = 1;
            this.l1.Text = "Mbps";
            this.l1.Click += new System.EventHandler(this.UC_Click);
            this.l1.MouseEnter += new System.EventHandler(this.L1_MouseEnter);
            this.l1.MouseLeave += new System.EventHandler(this.L1_MouseLeave);
            // 
            // speedThresholdNumericUpDownEx
            // 
            this.speedThresholdNumericUpDownEx.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.speedThresholdNumericUpDownEx.Location = new System.Drawing.Point(3, 3);
            this.speedThresholdNumericUpDownEx.Size = new System.Drawing.Size( 55, 16 );
            this.speedThresholdNumericUpDownEx.TabIndex = 0;
            this.speedThresholdNumericUpDownEx.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.speedThresholdNumericUpDownEx.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SpeedThresholdNumericUpDownEx_KeyDown);
            this.speedThresholdNumericUpDownEx.GotFocus += new System.EventHandler(this.SpeedThresholdNumericUpDownEx_GotFocus);
            this.speedThresholdNumericUpDownEx.Minimum = 1;
            this.speedThresholdNumericUpDownEx.Maximum = 1_000_000;
            this.speedThresholdNumericUpDownEx.Value = 1;
            //---this.speedThresholdNumericUpDownEx.ReadOnly = true;

            // 
            // SpeedThresholdUC
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            //this.AutoSize = true;
            //this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.TabStop = true;
            this.Click += new System.EventHandler(this.UC_Click);
            this.Controls.Add(this.l1);
            this.Controls.Add(this.speedThresholdNumericUpDownEx);
            this.Size = new System.Drawing.Size(97, 22);
            ((System.ComponentModel.ISupportInitialize)(this.speedThresholdNumericUpDownEx)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        #endregion

        private System.Windows.Forms.Label l1;
        //private System.Windows.Forms.NumericUpDownEx_Transparent speedThresholdNumericUpDownEx;
        private System.Windows.Forms.NumericUpDownEx speedThresholdNumericUpDownEx;
    }
}