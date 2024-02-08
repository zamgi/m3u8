namespace m3u8.download.manager.ui
{
    partial class ColumnsVisibilityEditor
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
            System.Windows.Forms.DataGridViewCellStyle cs = new System.Windows.Forms.DataGridViewCellStyle();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.DGV = new System.Windows.Forms.DataGridView_SaveSelectionByMouseDown();
            this.DGV_columnNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DGV_isVisibleColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.DGV)).BeginInit();
            this.SuspendLayout();
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(156, 410);
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
            this.okButton.Location = new System.Drawing.Point(75, 410);
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 4;
            this.okButton.Text = "Ok";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.okButton.Cursor = System.Windows.Forms.Cursors.Hand;
            // 
            // DGV
            // 
            this.DGV.Location = new System.Drawing.Point(3, 0);
            this.DGV.Size = new System.Drawing.Size(302, 406);
            this.DGV.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            cs.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            cs.BackColor = System.Drawing.SystemColors.Control;
            cs.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            cs.ForeColor = System.Drawing.SystemColors.WindowText;
            cs.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            cs.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            cs.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.DGV.ColumnHeadersDefaultCellStyle = cs;
            this.DGV.AllowUserToAddRows = false;
            this.DGV.AllowUserToDeleteRows = false;
            this.DGV.AllowUserToResizeRows = false;
            this.DGV.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DGV.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] 
            {
                this.DGV_isVisibleColumn,
                this.DGV_columnNameColumn
            });
            this.DGV.RowHeadersWidth = 25;
            this.DGV.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnKeystroke;
            this.DGV.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.DGV.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.DGV.TabIndex = 0;
            this.DGV.Resize += new System.EventHandler(this.DGV_Resize);
            this.DGV.ColumnWidthChanged += new System.Windows.Forms.DataGridViewColumnEventHandler(this.DGV_ColumnWidthChanged);
            this.DGV.RowHeadersWidthChanged += new System.EventHandler(this.DGV_RowHeadersWidthChanged);
            this.DGV.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DGV_CellContentClick);
            this.DGV.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.DGV_CellValueChanged);
            this.DGV.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.DGV_CellPainting);
            this.DGV.IsNeedSaveSelectionByMouseDown += new System.Windows.Forms.DataGridView_SaveSelectionByMouseDown.IsNeedSaveSelectionByMouseDownHandler(this.DGV_IsNeedSaveSelectionByMouseDown);

            // 
            // DGV_isVisibleColumn
            // 
            this.DGV_isVisibleColumn.HeaderText = "Is Visible";
            this.DGV_isVisibleColumn.Width = 50;
            // 
            // DGV_columnNameColumn
            // 
            this.DGV_columnNameColumn.HeaderText = "Column name";
            this.DGV_columnNameColumn.Width = 210;
            this.DGV_columnNameColumn.ReadOnly = true;

            // 
            // ColumnsVisibilityEditor
            // 
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(307, 434);
            this.Controls.Add(this.DGV);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.MinimizeBox = false;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "columns visibility editor";
            ((System.ComponentModel.ISupportInitialize)(this.DGV)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        #endregion

        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.DataGridView_SaveSelectionByMouseDown DGV;
        private System.Windows.Forms.DataGridViewCheckBoxColumn DGV_isVisibleColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn DGV_columnNameColumn;
    }
}