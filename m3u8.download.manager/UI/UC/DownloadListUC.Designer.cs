namespace m3u8.download.manager.ui
{
    partial class DownloadListUC
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle cs1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle cs2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.DGV = new System.Windows.Forms.DataGridView();
            this.DGV_urlColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DGV_outputFileNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DGV_outputDirectoryColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DGV_statusColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DGV_downloadInfoColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.DGV)).BeginInit();
            this.SuspendLayout();
            // 
            // DGV
            // 
            this.DGV.AllowUserToAddRows = false;
            this.DGV.AllowUserToDeleteRows = false;
            this.DGV.BackgroundColor = System.Drawing.Color.White;
            cs1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            cs1.BackColor = System.Drawing.SystemColors.Control;
            cs1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            cs1.ForeColor = System.Drawing.SystemColors.WindowText;
            cs1.SelectionBackColor = System.Drawing.SystemColors.Control;
            cs1.SelectionForeColor = System.Drawing.SystemColors.WindowText;
            cs1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.DGV.ColumnHeadersDefaultCellStyle = cs1;
            this.DGV.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DGV.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                this.DGV_urlColumn,
                this.DGV_outputFileNameColumn,
                this.DGV_outputDirectoryColumn,
                this.DGV_statusColumn,
                this.DGV_downloadInfoColumn
            });
            cs2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            cs2.BackColor = System.Drawing.SystemColors.Window;
            cs2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            cs2.ForeColor = System.Drawing.SystemColors.ControlText;
            cs2.SelectionBackColor = System.Drawing.Color.CadetBlue;
            cs2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            cs2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.DGV.DefaultCellStyle = cs2;
            this.DGV.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DGV.Location = new System.Drawing.Point(0, 0);
            this.DGV.MultiSelect = false;
            this.DGV.Name = "DGV";
            this.DGV.ReadOnly = true;
            this.DGV.RowHeadersWidth = 25;
            this.DGV.RowTemplate.Height = 27;
            this.DGV.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.DGV.Size = new System.Drawing.Size(948, 201);
            this.DGV.TabIndex = 1;
            this.DGV.VirtualMode = true;
            this.DGV.CellValueNeeded += new System.Windows.Forms.DataGridViewCellValueEventHandler(this.DGV_CellValueNeeded);
            this.DGV.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.DGV_CellFormatting);
            this.DGV.SelectionChanged += new System.EventHandler(this.DGV_SelectionChanged);
            this.DGV.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.DGV_CellPainting);
            this.DGV.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DGV_CellClick);
            this.DGV.CellMouseEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.DGV_CellMouseEnter);
            this.DGV.CellMouseLeave += new System.Windows.Forms.DataGridViewCellEventHandler(this.DGV_CellMouseLeave);
            this.DGV.ColumnHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.DGV_ColumnHeaderMouseClick);
            this.DGV.MouseClick += new System.Windows.Forms.MouseEventHandler(this.DGV_MouseClick);
            this.DGV.MouseMove +=new System.Windows.Forms.MouseEventHandler(this.DGV_MouseMove);
            // 
            // DGV_urlColumn
            // 
            this.DGV_urlColumn.FillWeight = 300F;
            this.DGV_urlColumn.HeaderText = "Url";
            this.DGV_urlColumn.Name = "DGV_urlColumn";
            this.DGV_urlColumn.ReadOnly = true;
            this.DGV_urlColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            this.DGV_urlColumn.Width = 300;
            // 
            // DGV_outputFileNameColumn
            // 
            this.DGV_outputFileNameColumn.FillWeight = 200F;
            this.DGV_outputFileNameColumn.HeaderText = "Output file name";
            this.DGV_outputFileNameColumn.Name = "DGV_outputFileNameColumn";
            this.DGV_outputFileNameColumn.ReadOnly = true;
            this.DGV_outputFileNameColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            this.DGV_outputFileNameColumn.Width = 200;
            // 
            // DGV_outputDirectoryColumn
            // 
            this.DGV_outputDirectoryColumn.HeaderText = "Output directory";
            this.DGV_outputDirectoryColumn.Name = "DGV_outputDirectoryColumn";
            this.DGV_outputDirectoryColumn.ReadOnly = true;
            this.DGV_outputDirectoryColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            // 
            // DGV_statusColumn
            // 
            this.DGV_statusColumn.HeaderText = "Status";
            this.DGV_statusColumn.Name = "DGV_statusColumn";
            this.DGV_statusColumn.ReadOnly = true;
            this.DGV_statusColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            // 
            // DGV_downloadInfoColumn
            // 
            this.DGV_downloadInfoColumn.FillWeight = 200F;
            this.DGV_downloadInfoColumn.HeaderText = "Download info";
            this.DGV_downloadInfoColumn.Name = "DGV_downloadInfoColumn";
            this.DGV_downloadInfoColumn.ReadOnly = true;
            this.DGV_downloadInfoColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            this.DGV_downloadInfoColumn.Width = 200;
            // 
            // DownloadListUC
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.DGV);
            this.Name = "DownloadListUC";
            this.Size = new System.Drawing.Size(948, 201);
            ((System.ComponentModel.ISupportInitialize)(this.DGV)).EndInit();
            this.ResumeLayout(false);
        }
        #endregion

        private System.Windows.Forms.DataGridView DGV;
        private System.Windows.Forms.DataGridViewTextBoxColumn DGV_urlColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn DGV_outputFileNameColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn DGV_outputDirectoryColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn DGV_statusColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn DGV_downloadInfoColumn;
    }
}
