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
            httpCheckBox = new System.Windows.Forms.CheckBox();
            socks5CheckBox = new System.Windows.Forms.CheckBox();
            torServerSocks5CheckBox = new System.Windows.Forms.CheckBox();
            torBrowserSocks5CheckBox = new System.Windows.Forms.CheckBox();
            addressGroupBox = new System.Windows.Forms.GroupBox();
            credentialsGroupBox = new System.Windows.Forms.GroupBox();
            userNameTextBox = new System.Windows.Forms.TextBoxEx();
            //portNumUpDown = new System.Windows.Forms.NumericUpDown();
            portTextBox = new System.Windows.Forms.TextBoxEx();
            addressTextBox = new System.Windows.Forms.TextBoxEx();
            passwordTextBox = new System.Windows.Forms.TextBoxEx();
            editWebProxyGroupBox = new System.Windows.Forms.GroupBox();
            editWebProxyGroupBox.SuspendLayout();
            addressGroupBox.SuspendLayout();
            credentialsGroupBox.SuspendLayout();
            //((System.ComponentModel.ISupportInitialize) portNumUpDown).BeginInit();
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
            torServerSocks5CheckBox.Text = "Tor Server Socks5";
            torServerSocks5CheckBox.UseVisualStyleBackColor = true;
            // 
            // torBrowserSocks5CheckBox
            // 
            torBrowserSocks5CheckBox.AutoSize = true;
            torBrowserSocks5CheckBox.Cursor = System.Windows.Forms.Cursors.Hand;
            torBrowserSocks5CheckBox.Location = new System.Drawing.Point( 30, 35 );
            torBrowserSocks5CheckBox.TabIndex = 0;
            torBrowserSocks5CheckBox.Text = "Tor Browser Socks5";
            torBrowserSocks5CheckBox.UseVisualStyleBackColor = true;
            // 
            // addressGroupBox
            // 
            addressGroupBox.Controls.Add( portTextBox/*portNumUpDown*/ );
            addressGroupBox.Controls.Add( addressTextBox );
            addressGroupBox.Location = new System.Drawing.Point( 205, 20 );
            addressGroupBox.Size = new System.Drawing.Size( 345, 72 );
            addressGroupBox.TabIndex = 1;
            addressGroupBox.TabStop = false;
            addressGroupBox.Text = "Address";
            // 
            // credentialsGroupBox
            // 
            credentialsGroupBox.Controls.Add( passwordTextBox );
            credentialsGroupBox.Controls.Add( userNameTextBox );
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
            //// 
            //// portNumUpDown
            //// 
            //portNumUpDown.Location = new System.Drawing.Point( 265, 27 );
            //portNumUpDown.Size = new System.Drawing.Size( 60, 23 );
            //portNumUpDown.TabIndex = 1;
            // 
            // portTextBox
            // 
            portTextBox.Location = new System.Drawing.Point( 265, 27 );
            portTextBox.Size = new System.Drawing.Size( 60, 23 );
            portTextBox.TabIndex = 1;
            portTextBox.PlaceHolderText = "Port";
            // 
            // addressTextBox
            // 
            addressTextBox.Location = new System.Drawing.Point( 20, 27 );
            addressTextBox.Size = new System.Drawing.Size( 237, 23 );
            addressTextBox.TabIndex = 0;
            addressTextBox.PlaceHolderText = "Hostname";
            // 
            // passwordTextBox
            // 
            passwordTextBox.Location = new System.Drawing.Point( 20, 60 );
            passwordTextBox.Size = new System.Drawing.Size( 237, 23 );
            passwordTextBox.TabIndex = 1;
            passwordTextBox.PlaceHolderText = "Password";
            // 
            // WebProxyUC
            // 
            AutoScaleDimensions = new System.Drawing.SizeF( 7F, 15F );
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add( credentialsGroupBox );
            Controls.Add( addressGroupBox );
            Controls.Add( editWebProxyGroupBox );
            Size = new System.Drawing.Size( 570, 320 );
            editWebProxyGroupBox.ResumeLayout( false );
            editWebProxyGroupBox.PerformLayout();
            addressGroupBox.ResumeLayout( false );
            addressGroupBox.PerformLayout();
            credentialsGroupBox.ResumeLayout( false );
            credentialsGroupBox.PerformLayout();
            //((System.ComponentModel.ISupportInitialize) portNumUpDown).EndInit();
            ResumeLayout( false );
        }
        #endregion

        private System.Windows.Forms.GroupBox editWebProxyGroupBox;
        private System.Windows.Forms.CheckBox torBrowserSocks5CheckBox;
        private System.Windows.Forms.CheckBox torServerSocks5CheckBox;
        private System.Windows.Forms.CheckBox httpCheckBox;
        private System.Windows.Forms.CheckBox socks5CheckBox;
        private System.Windows.Forms.GroupBox addressGroupBox;
        private System.Windows.Forms.TextBoxEx addressTextBox;
        //private System.Windows.Forms.NumericUpDown portNumUpDown;
        private System.Windows.Forms.TextBoxEx portTextBox;
        private System.Windows.Forms.GroupBox credentialsGroupBox;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.TextBoxEx userNameTextBox;
        private System.Windows.Forms.TextBoxEx passwordTextBox;
    }
}
