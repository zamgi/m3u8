namespace m3u8.download.manager.ui
{
    partial class RequestHeadersEditor
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle cs = new System.Windows.Forms.DataGridViewCellStyle();
            this.keyBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.DGV = new System.Windows.Forms.DataGridView_SaveSelectionByMouseDown();
            this.DGV_checkedColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.DGV_keyColumn = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.DGV_valueColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.filterLabel = new System.Windows.Forms.Label();
            this.filterTextBox = new System.Windows.Forms.TextBoxWithBorder();
            this.clearFilterButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.DGV)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.keyBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // DGV
            // 
            this.DGV.AllowUserToResizeRows = false;
            this.DGV.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.DGV.BorderStyle = System.Windows.Forms.BorderStyle.None;
            cs.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            cs.BackColor = System.Drawing.SystemColors.Control;
            cs.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            cs.ForeColor = System.Drawing.SystemColors.WindowText;
            cs.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            cs.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            cs.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.DGV.RowsAdded += new System.Windows.Forms.DataGridViewRowsAddedEventHandler(this.DGV_RowsAdded);
            this.DGV.ColumnHeadersDefaultCellStyle = cs;
            this.DGV.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DGV.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] 
            {
                this.DGV_checkedColumn,
                this.DGV_keyColumn,
                this.DGV_valueColumn
            });            
            this.DGV.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            this.DGV.ScrollDelayInMilliseconds = 10;
            this.DGV.Location = new System.Drawing.Point(3, 23);
            this.DGV.Size = new System.Drawing.Size(319, 389);
            this.DGV.TabIndex = 0;
            this.DGV.Resize += new System.EventHandler(this.DGV_Resize);
            this.DGV.ColumnWidthChanged += new System.Windows.Forms.DataGridViewColumnEventHandler(this.DGV_ColumnWidthChanged);
            this.DGV.RowHeadersWidthChanged += new System.EventHandler(this.DGV_RowHeadersWidthChanged);
            this.DGV.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.DGV_CellValueChanged);
            this.DGV.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DGV_CellContentClick);
            this.DGV.IsNeedSaveSelectionByMouseDown += new System.Windows.Forms.DataGridView_SaveSelectionByMouseDown.IsNeedSaveSelectionByMouseDownHandler(this.DGV_IsNeedSaveSelectionByMouseDown);
            this.DGV.ColumnHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.DGV_ColumnHeaderMouseClick);
            this.DGV.RowsRemoved += new System.Windows.Forms.DataGridViewRowsRemovedEventHandler(this.DGV_RowsRemoved);
            //---this.DGV.CellToolTipTextNeeded += new System.Windows.Forms.DataGridViewCellToolTipTextNeededEventHandler(this.DGV_CellToolTipTextNeeded);
            this.DGV.EditingControlShowing += new System.Windows.Forms.DataGridViewEditingControlShowingEventHandler(this.DGV_EditingControlShowing);
            this.DGV.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.DGV_DataError);
            this.DGV.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DGV_KeyDown);
            // 
            // DGV_checkedColumn
            // 
            this.DGV_checkedColumn.HeaderText = "\u2713";
            this.DGV_checkedColumn.ToolTipText = "check/unckeck all";
            this.DGV_checkedColumn.Width = 25;
            this.DGV_checkedColumn.MinimumWidth = 20;

            // 
            // keyBindingSource
            // 
            this.keyBindingSource.DataSource = typeof(RequestHeader);
            // 
            // DGV_keyColumn
            // 
            this.DGV_keyColumn.HeaderText = "key";
            this.DGV_keyColumn.DataPropertyName = "Name"; //---"FullName"; //---
            this.DGV_keyColumn.DisplayMember    = "DisplayName";
            this.DGV_keyColumn.ValueMember      = "Name"; //---"FullName";
            this.DGV_keyColumn.DataSource = this.keyBindingSource;
            this.DGV_keyColumn.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.DGV_keyColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.DGV_keyColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.DGV_keyColumn.Width = 200;
            this.DGV_keyColumn.DropDownWidth = 750;
            this.DGV_keyColumn.MaxDropDownItems = 25;
            this.DGV_keyColumn.MinimumWidth = 40;
            this.DGV_keyColumn.DefaultCellStyle = new System.Windows.Forms.DataGridViewCellStyle(this.DGV_keyColumn.DefaultCellStyle)
            {
                Font = new System.Drawing.Font( System.Drawing.FontFamily.GenericMonospace, this.DGV.Font.Size )
            };

            // 
            // DGV_valueColumn
            // 
            this.DGV_valueColumn.HeaderText = "value";
            this.DGV_valueColumn.Width = 120;
            this.DGV_valueColumn.MinimumWidth = 40;
            // 
            // filterLabel
            // 
            this.filterLabel.AutoSize = true;
            this.filterLabel.ForeColor = System.Drawing.Color.Silver;
            this.filterLabel.Location = new System.Drawing.Point(12, 3);
            this.filterLabel.Size = new System.Drawing.Size(29, 13);
            this.filterLabel.TabIndex = 1;
            this.filterLabel.Text = "filter:";
            // 
            // filterTextBox
            // 
            this.filterTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            //---this.filterTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.filterTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.5f/*11.5F*/);
            this.filterTextBox.Location = new System.Drawing.Point(45, 1);
            this.filterTextBox.Size = new System.Drawing.Size(224, 18);
            this.filterTextBox.TabIndex = 2;
            this.filterTextBox.WordWrap = false;
            this.filterTextBox.TextChanged += new System.EventHandler(this.filterTextBox_TextChanged);
            // 
            // clearFilterButton
            // 
            this.clearFilterButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.clearFilterButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.clearFilterButton.Location = new System.Drawing.Point(270, 2);
            this.clearFilterButton.Size = new System.Drawing.Size(16, 18);
            this.clearFilterButton.TabIndex = 3;
            this.clearFilterButton.Text = "x";
            this.clearFilterButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.clearFilterButton.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.clearFilterButton.UseCompatibleTextRendering = true;
            this.clearFilterButton.UseVisualStyleBackColor = true;
            this.clearFilterButton.Visible = false;
            this.clearFilterButton.Click += new System.EventHandler(this.clearFilterButton_Click);
            // 
            // RequestHeadersEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.clearFilterButton);
            this.Controls.Add(this.filterTextBox);
            this.Controls.Add(this.filterLabel);
            this.Controls.Add(this.DGV);
            this.Size = new System.Drawing.Size(325, 411);
            ((System.ComponentModel.ISupportInitialize)(this.DGV)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.keyBindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        #endregion

        private System.Windows.Forms.DataGridView_SaveSelectionByMouseDown DGV;
        private System.Windows.Forms.DataGridViewCheckBoxColumn DGV_checkedColumn;
        private System.Windows.Forms.BindingSource keyBindingSource;
        private System.Windows.Forms.DataGridViewComboBoxColumn DGV_keyColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn DGV_valueColumn;
        private System.Windows.Forms.Label filterLabel;
        private System.Windows.Forms.TextBoxWithBorder filterTextBox;
        private System.Windows.Forms.Button clearFilterButton;
    }
}