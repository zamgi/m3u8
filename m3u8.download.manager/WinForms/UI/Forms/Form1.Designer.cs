namespace m3u8.download.manager.ui
{
    partial class Form1
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
            this.setRequestHeadersButton = new System.Windows.Forms.Button();
            this.getRequestHeadersButton = new System.Windows.Forms.Button();
            this.requestHeadersEditor1 = new m3u8.download.manager.ui.RequestHeadersEditor();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // setRequestHeadersButton
            // 
            this.setRequestHeadersButton.Location = new System.Drawing.Point(115, 10);
            this.setRequestHeadersButton.Name = "setRequestHeadersButton";
            this.setRequestHeadersButton.Size = new System.Drawing.Size(164, 23);
            this.setRequestHeadersButton.TabIndex = 1;
            this.setRequestHeadersButton.Text = "set request-headers";
            this.setRequestHeadersButton.UseVisualStyleBackColor = true;
            this.setRequestHeadersButton.Click += new System.EventHandler(this.setRequestHeadersButton_Click);
            // 
            // getRequestHeadersButton
            // 
            this.getRequestHeadersButton.Location = new System.Drawing.Point(314, 10);
            this.getRequestHeadersButton.Name = "getRequestHeadersButton";
            this.getRequestHeadersButton.Size = new System.Drawing.Size(164, 23);
            this.getRequestHeadersButton.TabIndex = 2;
            this.getRequestHeadersButton.Text = "get request-headers";
            this.getRequestHeadersButton.UseVisualStyleBackColor = true;
            this.getRequestHeadersButton.Click += new System.EventHandler(this.getRequestHeadersButton_Click);
            // 
            // requestHeadersEditor1
            // 
            this.requestHeadersEditor1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.requestHeadersEditor1.Location = new System.Drawing.Point(0, 0);
            this.requestHeadersEditor1.Name = "requestHeadersEditor1";
            this.requestHeadersEditor1.Size = new System.Drawing.Size(800, 409);
            this.requestHeadersEditor1.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.setRequestHeadersButton);
            this.panel1.Controls.Add(this.getRequestHeadersButton);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 409);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(800, 41);
            this.panel1.TabIndex = 3;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.requestHeadersEditor1);
            this.Controls.Add(this.panel1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
        }
        #endregion

        private ui.RequestHeadersEditor requestHeadersEditor1;
        private System.Windows.Forms.Button setRequestHeadersButton;
        private System.Windows.Forms.Button getRequestHeadersButton;
        private System.Windows.Forms.Panel panel1;
    }
}