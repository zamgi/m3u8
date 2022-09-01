namespace m3u8.download.manager.ui
{
    partial class ParallelismSettingsUC
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
            System.Windows.Forms.GroupBox gb3;
            System.Windows.Forms.ToolTip toolTip;
            this.components = new System.ComponentModel.Container();
            this.maxDegreeOfParallelismNUD = new System.Windows.Forms.NumericUpDownEx();
            this.useCrossDownloadInstanceParallelismCheckBox = new System.Windows.Forms.CheckBox();
            this.maxCrossDownloadInstanceNUD = new System.Windows.Forms.NumericUpDownEx();
            this.useMaxCrossDownloadInstanceCheckBox = new System.Windows.Forms.CheckBox();
            this.maxDegreeOfParallelismLabel = new System.Windows.Forms.Label();
            this.maxCrossDownloadInstanceLabel = new System.Windows.Forms.Label();
            this.isUnlimMaxSpeedThresholdCheckBox = new System.Windows.Forms.CheckBox();
            this.maxSpeedThresholdLabel = new System.Windows.Forms.Label();
            this.maxSpeedThresholdNUD = new System.Windows.Forms.NumericUpDownEx();
            gb1 = new System.Windows.Forms.GroupBox();
            gb2 = new System.Windows.Forms.GroupBox();
            gb3 = new System.Windows.Forms.GroupBox();
            toolTip = new System.Windows.Forms.ToolTip(this.components);            
            ((System.ComponentModel.ISupportInitialize)(this.maxDegreeOfParallelismNUD)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.maxCrossDownloadInstanceNUD)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.maxSpeedThresholdNUD)).BeginInit();
            gb1.SuspendLayout();
            gb2.SuspendLayout();
            gb3.SuspendLayout();
            this.SuspendLayout();
            // 
            // l1
            // 
            this.maxDegreeOfParallelismLabel.AutoSize = true;
            this.maxDegreeOfParallelismLabel.Location = new System.Drawing.Point(41, 23);
            this.maxDegreeOfParallelismLabel.Size = new System.Drawing.Size(128, 13);
            this.maxDegreeOfParallelismLabel.TabIndex = 4;
            this.maxDegreeOfParallelismLabel.Text = "max download threads:"; //---"max degree of downloads parallelism:";
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
            // useCrossDownloadInstanceParallelismCheckBox
            // 
            this.useCrossDownloadInstanceParallelismCheckBox.AutoSize = true;
            this.useCrossDownloadInstanceParallelismCheckBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.useCrossDownloadInstanceParallelismCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.useCrossDownloadInstanceParallelismCheckBox.Location = new System.Drawing.Point(44, 68);
            this.useCrossDownloadInstanceParallelismCheckBox.Size = new System.Drawing.Size(166, 30);
            this.useCrossDownloadInstanceParallelismCheckBox.TabIndex = 5;
            this.useCrossDownloadInstanceParallelismCheckBox.Text = "share \"max download threads\"\r\n between all downloads-instance"; //---"use cross downloads-instance\r\ndegree of parallelism";            
            this.useCrossDownloadInstanceParallelismCheckBox.UseVisualStyleBackColor = true;
            toolTip.SetToolTip(this.useCrossDownloadInstanceParallelismCheckBox, "checked - share \"max download threads\" between all downloads-instance\r\n" +
                                                                                 "unchecked - use \"max download threads\" per each downloads-instance" );
            //---this.useCrossDownloadInstanceParallelismCheckBox.CheckedChanged += new System.EventHandler(this.useCrossDownloadInstanceParallelismCheckBox_CheckedChanged);

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
            // isUnlimMaxSpeedThresholdCheckBox
            // 
            this.isUnlimMaxSpeedThresholdCheckBox.AutoSize = true;
            this.isUnlimMaxSpeedThresholdCheckBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.isUnlimMaxSpeedThresholdCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.isUnlimMaxSpeedThresholdCheckBox.Location = new System.Drawing.Point(41, 23);
            this.isUnlimMaxSpeedThresholdCheckBox.Size = new System.Drawing.Size(85, 17);
            this.isUnlimMaxSpeedThresholdCheckBox.TabIndex = 5;
            this.isUnlimMaxSpeedThresholdCheckBox.Text = "Max/Unlimited speed";
            this.isUnlimMaxSpeedThresholdCheckBox.UseVisualStyleBackColor = true;
            this.isUnlimMaxSpeedThresholdCheckBox.CheckedChanged += new System.EventHandler(this.isUnlimMaxSpeedThresholdCheckBox_CheckedChanged);
            // 
            // maxSpeedThresholdLabel
            // 
            this.maxSpeedThresholdLabel.AutoSize = true;
            this.maxSpeedThresholdLabel.Location = new System.Drawing.Point(41, 53);
            this.maxSpeedThresholdLabel.Size = new System.Drawing.Size(149, 26);
            this.maxSpeedThresholdLabel.TabIndex = 4;
            this.maxSpeedThresholdLabel.Text = "max speed threshold (in Mbps):";
            // 
            // maxSpeedThresholdNUD
            // 
            this.maxSpeedThresholdNUD.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.maxSpeedThresholdNUD.Location = new System.Drawing.Point(41, 75);
            this.maxSpeedThresholdNUD.Minimum = new decimal( new int[] { 1, 0, 0, 0 } );
            this.maxSpeedThresholdNUD.Size = new System.Drawing.Size(89, 16);
            this.maxSpeedThresholdNUD.TabIndex = 1;
            this.maxSpeedThresholdNUD.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.maxSpeedThresholdNUD.Value = new decimal( new int[] { 20, 0, 0, 0 } );

            // 
            // gb1
            // 
            gb1.Controls.Add(this.useCrossDownloadInstanceParallelismCheckBox);
            gb1.Controls.Add(this.maxDegreeOfParallelismLabel);
            gb1.Controls.Add(this.maxDegreeOfParallelismNUD);
            gb1.Location = new System.Drawing.Point(14, 9);
            gb1.Size = new System.Drawing.Size(246, 110);
            gb1.TabIndex = 6;
            gb1.TabStop = false;
            gb1.Text = "downloads parallelism"; //---"download threads";
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
            // gb3
            // 
            gb3.Controls.Add(this.isUnlimMaxSpeedThresholdCheckBox);
            gb3.Controls.Add(this.maxSpeedThresholdLabel);
            gb3.Controls.Add(this.maxSpeedThresholdNUD);
            gb3.Location = new System.Drawing.Point( 14, 225 );
            gb3.Size = new System.Drawing.Size( 246, 110 );
            gb3.TabIndex = 8;
            gb3.TabStop = false;
            gb3.Text = "download speed limit";

            // 
            // ParallelismSettingsUC
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(270, 400);
            this.Controls.Add(gb3);
            this.Controls.Add(gb2);
            this.Controls.Add(gb1);
            this.Text = "parallelism";
            ((System.ComponentModel.ISupportInitialize)(this.maxDegreeOfParallelismNUD)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.maxCrossDownloadInstanceNUD)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.maxSpeedThresholdNUD)).EndInit();
            gb1.ResumeLayout(false);
            gb1.PerformLayout();
            gb2.ResumeLayout(false);
            gb2.PerformLayout();
            gb3.ResumeLayout(false);
            gb3.PerformLayout();
            this.ResumeLayout(false);
        }
        #endregion

        private System.Windows.Forms.CheckBox        useCrossDownloadInstanceParallelismCheckBox;
        private System.Windows.Forms.Label           maxDegreeOfParallelismLabel;
        private System.Windows.Forms.NumericUpDownEx maxDegreeOfParallelismNUD;
        
        private System.Windows.Forms.CheckBox        useMaxCrossDownloadInstanceCheckBox;
        private System.Windows.Forms.Label           maxCrossDownloadInstanceLabel;
        private System.Windows.Forms.NumericUpDownEx maxCrossDownloadInstanceNUD;

        private System.Windows.Forms.CheckBox        isUnlimMaxSpeedThresholdCheckBox;
        private System.Windows.Forms.Label           maxSpeedThresholdLabel;
        private System.Windows.Forms.NumericUpDownEx maxSpeedThresholdNUD;
    }
}