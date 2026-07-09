namespace m3u8.download.manager.UI.Forms
{
    partial class LoggerForm
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
            if ( disposing )
            {
                components?.Dispose();
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
            logTextBox = new System.Windows.Forms.TextBox();
            panel = new System.Windows.Forms.Panel();
            clearButton = new System.Windows.Forms.Button();
            splitContainer = new System.Windows.Forms.SplitContainer();
            logTextBox_4_Parts = new System.Windows.Forms.TextBox();
            panel_4_Parts = new System.Windows.Forms.Panel();
            clearButton_4_Parts = new System.Windows.Forms.Button();
            label_processor = new System.Windows.Forms.Label();
            label_4_parts = new System.Windows.Forms.Label();
            panel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize) splitContainer).BeginInit();
            splitContainer.Panel1.SuspendLayout();
            splitContainer.Panel2.SuspendLayout();
            splitContainer.SuspendLayout();
            panel_4_Parts.SuspendLayout();
            SuspendLayout();
            // 
            // logTextBox
            // 
            logTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            logTextBox.Location = new System.Drawing.Point( 55, 0 );
            logTextBox.Multiline = true;
            logTextBox.Name = "logTextBox";
            logTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            logTextBox.Size = new System.Drawing.Size( 341, 153 );
            logTextBox.TabIndex = 1;
            // 
            // panel
            // 
            panel.AutoSize = true;
            panel.Controls.Add( label_processor );
            panel.Controls.Add( clearButton );
            panel.Dock = System.Windows.Forms.DockStyle.Left;
            panel.Location = new System.Drawing.Point( 0, 0 );
            panel.Margin = new System.Windows.Forms.Padding( 0 );
            panel.Name = "panel";
            panel.Size = new System.Drawing.Size( 55, 153 );
            panel.TabIndex = 0;
            // 
            // clearButton
            // 
            clearButton.AutoSize = true;
            clearButton.Location = new System.Drawing.Point( 0, 0 );
            clearButton.Name = "clearButton";
            clearButton.Size = new System.Drawing.Size( 52, 25 );
            clearButton.TabIndex = 0;
            clearButton.Text = "clear";
            clearButton.UseVisualStyleBackColor = true;
            clearButton.Click += clearButton_Click;
            // 
            // splitContainer
            // 
            splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer.Location = new System.Drawing.Point( 0, 0 );
            splitContainer.Name = "splitContainer";
            splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer.Panel1
            // 
            splitContainer.Panel1.Controls.Add( logTextBox );
            splitContainer.Panel1.Controls.Add( panel );
            // 
            // splitContainer.Panel2
            // 
            splitContainer.Panel2.Controls.Add( logTextBox_4_Parts );
            splitContainer.Panel2.Controls.Add( panel_4_Parts );
            splitContainer.Size = new System.Drawing.Size( 396, 306 );
            splitContainer.SplitterDistance = 153;
            splitContainer.TabIndex = 0;
            // 
            // logTextBox_4_Parts
            // 
            logTextBox_4_Parts.Dock = System.Windows.Forms.DockStyle.Fill;
            logTextBox_4_Parts.Location = new System.Drawing.Point( 55, 0 );
            logTextBox_4_Parts.Multiline = true;
            logTextBox_4_Parts.Name = "logTextBox_4_Parts";
            logTextBox_4_Parts.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            logTextBox_4_Parts.Size = new System.Drawing.Size( 341, 149 );
            logTextBox_4_Parts.TabIndex = 1;
            // 
            // panel_4_Parts
            // 
            panel_4_Parts.AutoSize = true;
            panel_4_Parts.Controls.Add( label_4_parts );
            panel_4_Parts.Controls.Add( clearButton_4_Parts );
            panel_4_Parts.Dock = System.Windows.Forms.DockStyle.Left;
            panel_4_Parts.Location = new System.Drawing.Point( 0, 0 );
            panel_4_Parts.Name = "panel_4_Parts";
            panel_4_Parts.Size = new System.Drawing.Size( 55, 149 );
            panel_4_Parts.TabIndex = 0;
            // 
            // clearButton_4_Parts
            // 
            clearButton_4_Parts.AutoSize = true;
            clearButton_4_Parts.Location = new System.Drawing.Point( 0, 0 );
            clearButton_4_Parts.Name = "clearButton_4_Parts";
            clearButton_4_Parts.Size = new System.Drawing.Size( 52, 25 );
            clearButton_4_Parts.TabIndex = 0;
            clearButton_4_Parts.Text = "clear";
            clearButton_4_Parts.UseVisualStyleBackColor = true;
            clearButton_4_Parts.Click += clearButton_4_Parts_Click;
            // 
            // label_processor
            // 
            label_processor.AutoSize = true;
            label_processor.Location = new System.Drawing.Point( 12, 28 );
            label_processor.Name = "label_processor";
            label_processor.Size = new System.Drawing.Size( 34, 30 );
            label_processor.TabIndex = 1;
            label_processor.Text = "proc \r\nnext:";
            // 
            // label_4_parts
            // 
            label_4_parts.AutoSize = true;
            label_4_parts.Location = new System.Drawing.Point( 16, 28 );
            label_4_parts.Name = "label_4_parts";
            label_4_parts.Size = new System.Drawing.Size( 36, 15 );
            label_4_parts.TabIndex = 2;
            label_4_parts.Text = "parts:";
            // 
            // LoggerForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF( 7F, 15F );
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size( 396, 306 );
            Controls.Add( splitContainer );
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            Name = "LoggerForm";
            ShowInTaskbar = false;
            Text = "logger";
            panel.ResumeLayout( false );
            panel.PerformLayout();
            splitContainer.Panel1.ResumeLayout( false );
            splitContainer.Panel1.PerformLayout();
            splitContainer.Panel2.ResumeLayout( false );
            splitContainer.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize) splitContainer).EndInit();
            splitContainer.ResumeLayout( false );
            panel_4_Parts.ResumeLayout( false );
            panel_4_Parts.PerformLayout();
            ResumeLayout( false );
        }
        #endregion

        private System.Windows.Forms.TextBox logTextBox;
        private System.Windows.Forms.Panel panel;
        private System.Windows.Forms.Button clearButton;
        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.TextBox logTextBox_4_Parts;
        private System.Windows.Forms.Panel panel_4_Parts;
        private System.Windows.Forms.Button clearButton_4_Parts;
        private System.Windows.Forms.Label label_processor;
        private System.Windows.Forms.Label label_4_parts;
    }
}