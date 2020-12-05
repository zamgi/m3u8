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
            this.DGV_downloadProgressColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DGV_downloadTimeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DGV_approxRemainedTimeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DGV_downloadSpeedColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DGV_downloadBytesColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DGV_approxRemainedBytesColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DGV_approxTotalBytesColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.DGV)).BeginInit();
            this.SuspendLayout();
            // 
            // DGV
            // 
            this.DGV.AllowUserToAddRows = false;
            this.DGV.AllowUserToDeleteRows = false;
            this.DGV.AllowUserToOrderColumns = true;
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
                this.DGV_outputFileNameColumn,
                this.DGV_outputDirectoryColumn,
                this.DGV_statusColumn,
                this.DGV_downloadProgressColumn,
                this.DGV_downloadTimeColumn,
                this.DGV_approxRemainedTimeColumn,
                this.DGV_downloadSpeedColumn,
                this.DGV_downloadBytesColumn,
                this.DGV_approxRemainedBytesColumn,
                this.DGV_approxTotalBytesColumn,
                this.DGV_urlColumn
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
            this.DGV.MouseDown +=new System.Windows.Forms.MouseEventHandler(this.DGV_MouseDown);
            this.DGV.MouseClick += new System.Windows.Forms.MouseEventHandler(this.DGV_MouseClick);
            this.DGV.DoubleClick += new System.EventHandler(this.DGV_DoubleClick);
            this.DGV.MouseMove += new System.Windows.Forms.MouseEventHandler(this.DGV_MouseMove);
            this.DGV.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.DGV_DataError);
            // 
            // DGV_urlColumn
            // 
            this.DGV_urlColumn.FillWeight = 120F;
            this.DGV_urlColumn.Width = 120;
            this.DGV_urlColumn.HeaderText = "Url";
            this.DGV_urlColumn.Name = "DGV_urlColumn";
            this.DGV_urlColumn.ReadOnly = true;
            this.DGV_urlColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;            
            // 
            // DGV_outputFileNameColumn
            // 
            this.DGV_outputFileNameColumn.FillWeight = 180F;
            this.DGV_outputFileNameColumn.Width = 180;
            this.DGV_outputFileNameColumn.HeaderText = "Output file name";
            this.DGV_outputFileNameColumn.Name = "DGV_outputFileNameColumn";
            this.DGV_outputFileNameColumn.ReadOnly = true;
            this.DGV_outputFileNameColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            // 
            // DGV_outputDirectoryColumn
            // 
            this.DGV_outputDirectoryColumn.FillWeight = 90F;
            this.DGV_outputDirectoryColumn.Width = 90;
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
            // DGV_downloadProgressColumn
            // 
            this.DGV_downloadProgressColumn.FillWeight = 125F;
            this.DGV_downloadProgressColumn.Width = 125;
            this.DGV_downloadProgressColumn.HeaderText = "Progress";
            this.DGV_downloadProgressColumn.Name = "DGV_downloadProgress";
            this.DGV_downloadProgressColumn.ReadOnly = true;
            this.DGV_downloadProgressColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            // 
            // DGV_downloadTimeColumn
            // 
            this.DGV_downloadTimeColumn.FillWeight = 80F;
            this.DGV_downloadTimeColumn.Width = 80;
            this.DGV_downloadTimeColumn.HeaderText = "Time";
            this.DGV_downloadTimeColumn.Name = "DGV_downloadTimeColumn";
            this.DGV_downloadTimeColumn.ReadOnly = true;
            this.DGV_downloadTimeColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // DGV_approxRemainedTimeColumn
            // 
            this.DGV_approxRemainedTimeColumn.FillWeight = 80F;
            this.DGV_approxRemainedTimeColumn.Width = 80;
            this.DGV_approxRemainedTimeColumn.HeaderText = "Time Remained ~";
            this.DGV_approxRemainedTimeColumn.Name = "DGV_approxRemainedTimeColumn";
            this.DGV_approxRemainedTimeColumn.ReadOnly = true;
            this.DGV_approxRemainedTimeColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // DGV_downloadSpeedColumn
            // 
            this.DGV_downloadSpeedColumn.FillWeight = 80F;
            this.DGV_downloadSpeedColumn.Width = 80;
            this.DGV_downloadSpeedColumn.HeaderText = "Speed";
            this.DGV_downloadSpeedColumn.Name = "DGV_downloadSpeedColumn";
            this.DGV_downloadSpeedColumn.ReadOnly = true;
            this.DGV_downloadSpeedColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // DGV_downloadBytesColumn
            // 
            this.DGV_downloadBytesColumn.FillWeight = 80F;
            this.DGV_downloadBytesColumn.Width = 80;
            this.DGV_downloadBytesColumn.HeaderText = "Received (Size)";
            this.DGV_downloadBytesColumn.Name = "DGV_downloadBytesColumn";
            this.DGV_downloadBytesColumn.ReadOnly = true;
            this.DGV_downloadBytesColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            // 
            // DGV_approxRemainedBytesColumn
            // 
            this.DGV_approxRemainedBytesColumn.FillWeight = 80F;
            this.DGV_approxRemainedBytesColumn.Width = 80;
            this.DGV_approxRemainedBytesColumn.HeaderText = "Remained ~ (Size)";
            this.DGV_approxRemainedBytesColumn.Name = "DGV_approxRemainedBytesColumn";
            this.DGV_approxRemainedBytesColumn.ReadOnly = true;
            this.DGV_approxRemainedBytesColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;
            // 
            // DGV_approxTotalBytesColumn
            // 
            this.DGV_approxTotalBytesColumn.FillWeight = 80F;
            this.DGV_approxTotalBytesColumn.Width = 80;
            this.DGV_approxTotalBytesColumn.HeaderText = "Total ~ (Size)";
            this.DGV_approxTotalBytesColumn.Name = "DGV_approxTotalBytesColumn";
            this.DGV_approxTotalBytesColumn.ReadOnly = true;
            this.DGV_approxTotalBytesColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Programmatic;

            // 
            // DownloadListUC
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.DGV);
            this.Name = "DownloadListUC";
            this.Size = new System.Drawing.Size(1200, 201);
            ((System.ComponentModel.ISupportInitialize)(this.DGV)).EndInit();
            this.ResumeLayout(false);
        }        
        #endregion

        private System.Windows.Forms.DataGridView DGV;
        private System.Windows.Forms.DataGridViewTextBoxColumn DGV_urlColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn DGV_outputFileNameColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn DGV_outputDirectoryColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn DGV_statusColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn DGV_downloadProgressColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn DGV_downloadTimeColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn DGV_approxRemainedTimeColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn DGV_downloadSpeedColumn;

        private System.Windows.Forms.DataGridViewTextBoxColumn DGV_downloadBytesColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn DGV_approxRemainedBytesColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn DGV_approxTotalBytesColumn;
    }
}
