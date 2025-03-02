namespace m3u8.download.manager.ui
{
    partial class FileNameExcludesWordsEditor
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
            System.Windows.Forms.DataGridViewCellStyle cs1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.DGV = new System.Windows.Forms.DataGridViewEx();
            this.DGV_excludesWordsColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.l1 = new System.Windows.Forms.Label();
            this.filterTextBox = new System.Windows.Forms.TextBox();
            this.clearFilterButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.DGV)).BeginInit();
            this.SuspendLayout();
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(156, 408);
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 5;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.cancelButton.Cursor = System.Windows.Forms.Cursors.Hand;
            // 
            // okButton
            // 
            this.okButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(75, 408);
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 4;
            this.okButton.Text = "Ok";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.okButton.Cursor = System.Windows.Forms.Cursors.Hand;
            // 
            // DGV
            // 
            this.DGV.AllowUserToResizeRows = false;
            this.DGV.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.DGV.GridColor = DefaultColors.DGV.GridLinesColor;
            this.DGV.BorderStyle = System.Windows.Forms.BorderStyle.None;
            cs1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            cs1.BackColor = System.Drawing.SystemColors.Control;
            cs1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));            
            cs1.ForeColor          = System.Drawing.SystemColors.WindowText;
            cs1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            cs1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            cs1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.DGV.ColumnHeadersDefaultCellStyle = cs1;
            this.DGV.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;            
            this.DGV.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] 
            {
                this.DGV_excludesWordsColumn
            });
            this.DGV.Location = new System.Drawing.Point(3, 22);
            this.DGV.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            this.DGV.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.DGV.Size = new System.Drawing.Size(302, 384);
            this.DGV.TabIndex = 0;
            this.DGV.Resize += new System.EventHandler(this.DGV_Resize);            
            // 
            // DGV_excludesWordsColumn
            // 
            this.DGV_excludesWordsColumn.HeaderText = "Exclude word";
            this.DGV_excludesWordsColumn.Width = 210;
            // 
            // l1
            // 
            this.l1.AutoSize = true;
            this.l1.Location = new System.Drawing.Point(12, 3);
            this.l1.Size = new System.Drawing.Size(29, 13);
            this.l1.TabIndex = 1;
            this.l1.ForeColor = System.Drawing.Color.Silver; //.DimGray;
            this.l1.Text = "filter:";
            // 
            // filterTextBox
            //             
            this.filterTextBox.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Top;
            this.filterTextBox.Location = new System.Drawing.Point(45, 2);
            this.filterTextBox.Size = new System.Drawing.Size(224, 18);
            this.filterTextBox.TabIndex = 2;
            this.filterTextBox.TextChanged += new System.EventHandler(this.filterTextBox_TextChanged);
            this.filterTextBox.WordWrap = false;
            this.filterTextBox.Font = new System.Drawing.Font( "Microsoft Sans Serif", 11.5F );
            this.filterTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            // 
            // clearFilterButton
            // 
            this.clearFilterButton.Anchor = System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Top;
            this.clearFilterButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.clearFilterButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.clearFilterButton.Visible = false;
            this.clearFilterButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.clearFilterButton.Location = new System.Drawing.Point(270, 2);
            this.clearFilterButton.Size = new System.Drawing.Size(16, 18);
            this.clearFilterButton.TabIndex = 3;
            this.clearFilterButton.Text = "x";
            this.clearFilterButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.clearFilterButton.UseCompatibleTextRendering = true;
            this.clearFilterButton.UseVisualStyleBackColor = true;
            this.clearFilterButton.Click += new System.EventHandler(this.clearFilterButton_Click);
            // 
            // FileNameExcludesWordsEditor
            // 
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(307, 434);
            this.Controls.Add(this.clearFilterButton);
            this.Controls.Add(this.filterTextBox);
            this.Controls.Add(this.l1);
            this.Controls.Add(this.DGV);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.MinimizeBox = false;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "file name excludes words editor";
            ((System.ComponentModel.ISupportInitialize)(this.DGV)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        #endregion

        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.DataGridViewEx DGV;
        private System.Windows.Forms.DataGridViewTextBoxColumn DGV_excludesWordsColumn;
        private System.Windows.Forms.Label l1;
        private System.Windows.Forms.TextBox filterTextBox;
        private System.Windows.Forms.Button clearFilterButton;
    }
}