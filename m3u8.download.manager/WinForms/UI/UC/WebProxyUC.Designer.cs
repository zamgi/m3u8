using System.Drawing;

namespace m3u8.download.manager.ui
{
    partial class WebProxyUC
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

        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.ToolTip toolTip = new System.Windows.Forms.ToolTip();
            httpCheckBox = new System.Windows.Forms.CheckBox();
            socks5CheckBox = new System.Windows.Forms.CheckBox();
            torServerSocks5CheckBox = new System.Windows.Forms.CheckBox();
            torBrowserSocks5CheckBox = new System.Windows.Forms.CheckBox();
            addressGroupBox = new System.Windows.Forms.GroupBox();
            credentialsGroupBox = new System.Windows.Forms.GroupBox();
            userNameTextBox = new System.Windows.Forms.TextBoxEx();
            portTextBox = new System.Windows.Forms.TextBoxEx();
            addressTextBox = new System.Windows.Forms.TextBoxEx();
            passwordTextBox = new System.Windows.Forms.TextBoxEx();
            testConnectionButton = new System.Windows.Forms.ButtonWithFocusCues();
            editWebProxyGroupBox = new System.Windows.Forms.GroupBox();
            editWebProxyGroupBox.SuspendLayout();
            addressGroupBox.SuspendLayout();
            credentialsGroupBox.SuspendLayout();
            SuspendLayout();
            // 
            // editWebProxyGroupBox
            // 
            editWebProxyGroupBox.Controls.Add( httpCheckBox );
            editWebProxyGroupBox.Controls.Add( socks5CheckBox );
            editWebProxyGroupBox.Controls.Add( torServerSocks5CheckBox );
            editWebProxyGroupBox.Controls.Add( torBrowserSocks5CheckBox );
            editWebProxyGroupBox.Location = new System.Drawing.Point( 20, 20 );
            editWebProxyGroupBox.Size = new System.Drawing.Size( 172, 178 );
            editWebProxyGroupBox.TabIndex = 0;
            editWebProxyGroupBox.TabStop = false;
            editWebProxyGroupBox.Text = "Edit web proxy";            
            // 
            // httpCheckBox
            // 
            httpCheckBox.AutoSize = true;
            httpCheckBox.Cursor = System.Windows.Forms.Cursors.Hand;
            httpCheckBox.Location = new System.Drawing.Point( 30, 125 );
            httpCheckBox.TabIndex = 3;
            httpCheckBox.Text = "HTTP";
            httpCheckBox.UseVisualStyleBackColor = true;
            // 
            // socks5CheckBox
            // 
            socks5CheckBox.AutoSize = true;
            socks5CheckBox.Cursor = System.Windows.Forms.Cursors.Hand;
            socks5CheckBox.Location = new System.Drawing.Point( 30, 95 );
            socks5CheckBox.TabIndex = 2;
            socks5CheckBox.Text = "Socks5";
            socks5CheckBox.UseVisualStyleBackColor = true;
            // 
            // torServerSocks5CheckBox
            // 
            torServerSocks5CheckBox.AutoSize = true;
            torServerSocks5CheckBox.Cursor = System.Windows.Forms.Cursors.Hand;
            torServerSocks5CheckBox.Location = new System.Drawing.Point( 30, 65 );
            torServerSocks5CheckBox.TabIndex = 1;
            torServerSocks5CheckBox.Text = "Socks5 (Tor Server)";
            torServerSocks5CheckBox.UseVisualStyleBackColor = true;
            toolTip.SetToolTip( torServerSocks5CheckBox, "use predefined Tor Server Socks5 web proxy" );
            // 
            // torBrowserSocks5CheckBox
            // 
            torBrowserSocks5CheckBox.AutoSize = true;
            torBrowserSocks5CheckBox.Cursor = System.Windows.Forms.Cursors.Hand;
            torBrowserSocks5CheckBox.Location = new System.Drawing.Point( 30, 35 );
            torBrowserSocks5CheckBox.TabIndex = 0;
            torBrowserSocks5CheckBox.Text = "Socks5 (Tor Browser)";
            torBrowserSocks5CheckBox.UseVisualStyleBackColor = true;
            toolTip.SetToolTip( torBrowserSocks5CheckBox, "use predefined Tor Browser Socks5 web proxy" );
            // 
            // addressGroupBox
            // 
            addressGroupBox.Controls.Add(portTextBox);
            addressGroupBox.Controls.Add(addressTextBox);            
            addressGroupBox.Location = new System.Drawing.Point( 205, 20 );
            addressGroupBox.Size = new System.Drawing.Size( 345, 72 );
            addressGroupBox.TabIndex = 1;
            addressGroupBox.TabStop = false;
            addressGroupBox.Text = "Address";
            // 
            // addressTextBox
            // 
            addressTextBox.Location = new System.Drawing.Point( 20, 27 );
            addressTextBox.Size = new System.Drawing.Size( 237, 23 );
            addressTextBox.TabIndex = 0;
            addressTextBox.PlaceHolderText = "Hostname";
            // 
            // portTextBox
            // 
            portTextBox.Location = new System.Drawing.Point( 265, 27 );
            portTextBox.Size = new System.Drawing.Size( 60, 23 );
            portTextBox.TabIndex = 1;
            portTextBox.PlaceHolderText = "Port";
            // 
            // credentialsGroupBox
            // 
            credentialsGroupBox.Controls.Add( passwordTextBox );
            credentialsGroupBox.Controls.Add( userNameTextBox );
            credentialsGroupBox.Controls.Add( testConnectionButton );
            credentialsGroupBox.Location = new System.Drawing.Point( 205, 98 );
            credentialsGroupBox.Size = new System.Drawing.Size( 345, 100 );
            credentialsGroupBox.TabIndex = 2;
            credentialsGroupBox.TabStop = false;
            credentialsGroupBox.Text = "Credentials (optional)";
            // 
            // userNameTextBox
            // 
            userNameTextBox.Location = new System.Drawing.Point( 20, 27 );
            userNameTextBox.Size = new System.Drawing.Size( 237, 23 );
            userNameTextBox.TabIndex = 0;
            userNameTextBox.PlaceHolderText = "Username";
            // 
            // passwordTextBox
            // 
            passwordTextBox.Location = new System.Drawing.Point( 20, 60 );
            passwordTextBox.Size = new System.Drawing.Size( 237, 23 );
            passwordTextBox.TabIndex = 1;
            passwordTextBox.PlaceHolderText = "Password";
            // 
            // testConnectionButton
            // 
            this.testConnectionButton.AutoEllipsis = true;
            this.testConnectionButton.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.testConnectionButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.testConnectionButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.testConnectionButton.Location = new System.Drawing.Point(265, 27);
            this.testConnectionButton.Size = new System.Drawing.Size(70, 21);
            this.testConnectionButton.TabIndex = 3;
            this.testConnectionButton.Text = "test link...";
            this.testConnectionButton.UseVisualStyleBackColor = true;
            this.testConnectionButton.Visible = false;
            this.testConnectionButton.Click += new System.EventHandler(this.testConnectionButton_Click);
            // 
            // WebProxyUC
            // 
            AutoScroll = true;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Size = new System.Drawing.Size(570, 320);
            Controls.Add(credentialsGroupBox);
            Controls.Add(addressGroupBox);
            Controls.Add(editWebProxyGroupBox);
            editWebProxyGroupBox.ResumeLayout( false );
            editWebProxyGroupBox.PerformLayout();
            addressGroupBox.ResumeLayout( false );
            addressGroupBox.PerformLayout();
            credentialsGroupBox.ResumeLayout( false );
            credentialsGroupBox.PerformLayout();
            ResumeLayout( false );
            PerformLayout();
        }
        #endregion

        private System.Windows.Forms.GroupBox editWebProxyGroupBox;
        private System.Windows.Forms.CheckBox torBrowserSocks5CheckBox;
        private System.Windows.Forms.CheckBox torServerSocks5CheckBox;
        private System.Windows.Forms.CheckBox httpCheckBox;
        private System.Windows.Forms.CheckBox socks5CheckBox;
        private System.Windows.Forms.GroupBox addressGroupBox;
        private System.Windows.Forms.TextBoxEx addressTextBox;
        private System.Windows.Forms.TextBoxEx portTextBox;
        private System.Windows.Forms.GroupBox credentialsGroupBox;
        private System.Windows.Forms.TextBoxEx userNameTextBox;
        private System.Windows.Forms.TextBoxEx passwordTextBox;
        private System.Windows.Forms.ButtonWithFocusCues testConnectionButton;
    }
}
