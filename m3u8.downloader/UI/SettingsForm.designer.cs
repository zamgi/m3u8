namespace m3u8.downloader
{
    partial class SettingsForm
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
            this.attemptRequestCountByPartNUD = new System.Windows.Forms.NumericUpDown();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.l1 = new System.Windows.Forms.Label();
            this.l2 = new System.Windows.Forms.Label();
            this.requestTimeoutByPartDTP = new System.Windows.Forms.DateTimePicker();
            this.gb1 = new System.Windows.Forms.GroupBox();
            this.showOnlyRequestRowsWithErrorsCheckBox = new System.Windows.Forms.CheckBox();
            this.storeMainFormPositionCheckBox = new System.Windows.Forms.CheckBox();
            this.logUIGridViewCheckBox = new System.Windows.Forms.CheckBox();
            this.logUITextBoxCheckBox = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.attemptRequestCountByPartNUD)).BeginInit();
            this.gb1.SuspendLayout();
            this.SuspendLayout();
            // 
            // attemptRequestCountByPartNUD
            // 
            this.attemptRequestCountByPartNUD.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.attemptRequestCountByPartNUD.Location = new System.Drawing.Point(167, 10);
            this.attemptRequestCountByPartNUD.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.attemptRequestCountByPartNUD.Size = new System.Drawing.Size(89, 16);
            this.attemptRequestCountByPartNUD.TabIndex = 1;
            this.attemptRequestCountByPartNUD.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.attemptRequestCountByPartNUD.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // okButton
            // 
            this.okButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.okButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.okButton.Location = new System.Drawing.Point(65, 235);
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 6;
            this.okButton.Text = "Ok";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.cancelButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.cancelButton.Location = new System.Drawing.Point(146, 235);
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 7;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // l1
            // 
            this.l1.AutoSize = true;
            this.l1.Location = new System.Drawing.Point(12, 9);
            this.l1.Size = new System.Drawing.Size(148, 13);
            this.l1.TabIndex = 0;
            this.l1.Text = "attempt request count by part:";
            // 
            // l2
            // 
            this.l2.AutoSize = true;
            this.l2.Location = new System.Drawing.Point(43, 42);
            this.l2.Size = new System.Drawing.Size(117, 13);
            this.l2.TabIndex = 3;
            this.l2.Text = "request timeout by part:";
            // 
            // requestTimeoutByPartDTP
            // 
            this.requestTimeoutByPartDTP.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.requestTimeoutByPartDTP.Location = new System.Drawing.Point(167, 36);
            this.requestTimeoutByPartDTP.ShowUpDown = true;
            this.requestTimeoutByPartDTP.Size = new System.Drawing.Size(91, 20);
            this.requestTimeoutByPartDTP.TabIndex = 2;
            // 
            // gb1
            // 
            this.gb1.Controls.Add(this.showOnlyRequestRowsWithErrorsCheckBox);
            this.gb1.Controls.Add(this.logUIGridViewCheckBox);
            this.gb1.Controls.Add(this.logUITextBoxCheckBox);
            this.gb1.Location = new System.Drawing.Point(13, 110);
            this.gb1.Size = new System.Drawing.Size(261, 116);
            this.gb1.TabIndex = 5;
            this.gb1.TabStop = false;
            this.gb1.Text = "download log UI type";
            // 
            // showOnlyRequestRowsWithErrorsCheckBox
            // 
            this.showOnlyRequestRowsWithErrorsCheckBox.AutoSize = true;
            this.showOnlyRequestRowsWithErrorsCheckBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.showOnlyRequestRowsWithErrorsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.showOnlyRequestRowsWithErrorsCheckBox.Location = new System.Drawing.Point(50, 77);
            this.showOnlyRequestRowsWithErrorsCheckBox.Size = new System.Drawing.Size(185, 17);
            this.showOnlyRequestRowsWithErrorsCheckBox.TabIndex = 2;
            this.showOnlyRequestRowsWithErrorsCheckBox.Text = "show only request rows with errors";
            this.showOnlyRequestRowsWithErrorsCheckBox.UseVisualStyleBackColor = true;
            // 
            // storeMainFormPositionCheckBox
            // 
            this.storeMainFormPositionCheckBox.AutoSize = true;
            this.storeMainFormPositionCheckBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.storeMainFormPositionCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.storeMainFormPositionCheckBox.Location = new System.Drawing.Point( 50, 77 );
            this.storeMainFormPositionCheckBox.Size = new System.Drawing.Size( 185, 17 );
            this.storeMainFormPositionCheckBox.TabIndex = 4;
            this.storeMainFormPositionCheckBox.Text = "store main window position && size";
            this.storeMainFormPositionCheckBox.UseVisualStyleBackColor = true;
            // 
            // logUIGridViewCheckBox
            // 
            this.logUIGridViewCheckBox.AutoSize = true;
            this.logUIGridViewCheckBox.Checked = true;
            this.logUIGridViewCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.logUIGridViewCheckBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.logUIGridViewCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.logUIGridViewCheckBox.Location = new System.Drawing.Point(33, 50);
            this.logUIGridViewCheckBox.Size = new System.Drawing.Size(169, 17);
            this.logUIGridViewCheckBox.TabIndex = 1;
            this.logUIGridViewCheckBox.Text = "grid-view download log UI type";
            this.logUIGridViewCheckBox.UseVisualStyleBackColor = true;
            // 
            // logUITextBoxCheckBox
            // 
            this.logUITextBoxCheckBox.AutoSize = true;
            this.logUITextBoxCheckBox.Cursor = System.Windows.Forms.Cursors.Hand;
            this.logUITextBoxCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.logUITextBoxCheckBox.Location = new System.Drawing.Point(33, 23);
            this.logUITextBoxCheckBox.Size = new System.Drawing.Size(164, 17);
            this.logUITextBoxCheckBox.TabIndex = 0;
            this.logUITextBoxCheckBox.Text = "text-box download log UI type";
            this.logUITextBoxCheckBox.UseVisualStyleBackColor = true;
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(286, 270);
            this.Controls.Add(this.gb1);
            this.Controls.Add(this.requestTimeoutByPartDTP);
            this.Controls.Add(this.l2);
            this.Controls.Add(this.l1);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.storeMainFormPositionCheckBox);
            this.Controls.Add(this.attemptRequestCountByPartNUD);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "settings";
            ((System.ComponentModel.ISupportInitialize)(this.attemptRequestCountByPartNUD)).EndInit();
            this.gb1.ResumeLayout(false);
            this.gb1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        #endregion

        private System.Windows.Forms.Label l1;
        private System.Windows.Forms.Label l2;
        private System.Windows.Forms.NumericUpDown attemptRequestCountByPartNUD;
        private System.Windows.Forms.DateTimePicker requestTimeoutByPartDTP;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.GroupBox gb1;
        private System.Windows.Forms.CheckBox logUIGridViewCheckBox;
        private System.Windows.Forms.CheckBox logUITextBoxCheckBox;
        private System.Windows.Forms.CheckBox showOnlyRequestRowsWithErrorsCheckBox;
        private System.Windows.Forms.CheckBox storeMainFormPositionCheckBox;
    }
}