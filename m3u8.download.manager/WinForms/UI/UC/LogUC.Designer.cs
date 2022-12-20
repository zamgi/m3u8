namespace m3u8.download.manager.ui
{
    partial class LogUC
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
            System.Windows.Forms.DataGridViewCellStyle cs3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle cs4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle cs5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle cs6 = new System.Windows.Forms.DataGridViewCellStyle();
            this.DGV = new System.Windows.Forms.DataGridViewEx();
            this.DGV_requestColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DGV_responseColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.DGV)).BeginInit();
            this.SuspendLayout();
            // 
            // DGV
            //
            this.DGV.AllowUserToAddRows = false;
            this.DGV.AllowUserToDeleteRows = false;
            cs1.BackColor = System.Drawing.Color.WhiteSmoke;
            this.DGV.AlternatingRowsDefaultCellStyle = cs1;
            cs2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            cs2.BackColor = System.Drawing.SystemColors.Control;
            cs2.ForeColor = System.Drawing.Color.Gray;
            cs2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            cs2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            cs2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.DGV.ColumnHeadersDefaultCellStyle = cs2;
            this.DGV.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DGV.Columns.AddRange( new System.Windows.Forms.DataGridViewColumn[] 
            {
                this.DGV_requestColumn,
                this.DGV_responseColumn
            });
            cs3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopLeft;
            cs3.BackColor = System.Drawing.SystemColors.Window;
            cs3.ForeColor = System.Drawing.SystemColors.ControlText;
            cs3.SelectionBackColor = System.Drawing.Color.DodgerBlue;
            cs3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            cs3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.DGV.DefaultCellStyle = cs3;
            this.DGV.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DGV.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.DGV.Location = new System.Drawing.Point(0, 0);
            this.DGV.ReadOnly = true;
            this.DGV.RowHeadersVisible = false;
            cs4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopLeft;
            cs4.BackColor = System.Drawing.Color.White;
            this.DGV.RowsDefaultCellStyle = cs4;
            this.DGV.RowTemplate.DefaultCellStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.DGV.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.DGV.Size = new System.Drawing.Size(746, 452);
            this.DGV.TabIndex = 0;
            this.DGV.VirtualMode = true;
            this.DGV.Resize += new System.EventHandler(this.DGV_Resize);
            this.DGV.MouseClick += new System.Windows.Forms.MouseEventHandler(this.DGV_MouseClick);
            this.DGV.CellValueNeeded += new System.Windows.Forms.DataGridViewCellValueEventHandler(this.DGV_CellValueNeeded);
            this.DGV.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.DGV_CellFormatting);
            this.DGV.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.DGV_CellPainting);
            this.DGV.RowDividerDoubleClick += new System.Windows.Forms.DataGridViewRowDividerDoubleClickEventHandler(this.DGV_RowDividerDoubleClick);
            //---this.DGV.RowHeightInfoNeeded += new System.Windows.Forms.DataGridViewRowHeightInfoNeededEventHandler(this.DGV_RowHeightInfoNeeded);
            // 
            // DGV_requestColumn
            // 
            cs5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopLeft;
            this.DGV_requestColumn.DefaultCellStyle = cs5;
            this.DGV_requestColumn.FillWeight = 350F;
            this.DGV_requestColumn.HeaderText = "Request";
            this.DGV_requestColumn.MinimumWidth = 75;
            this.DGV_requestColumn.ReadOnly = true;
            this.DGV_requestColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.DGV_requestColumn.Width = 350;
            // 
            // DGV_responseColumn
            // 
            cs6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopLeft;
            this.DGV_responseColumn.DefaultCellStyle = cs6;
            this.DGV_responseColumn.FillWeight = 350F;
            this.DGV_responseColumn.HeaderText = "Response";
            this.DGV_responseColumn.MinimumWidth = 75;
            this.DGV_responseColumn.ReadOnly = true;
            this.DGV_responseColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.DGV_responseColumn.Width = 350;
            // 
            // LogUC
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.DGV);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.5F);
            this.Size = new System.Drawing.Size(746, 452);
            ((System.ComponentModel.ISupportInitialize)(this.DGV)).EndInit();
            this.ResumeLayout(false);
        }
        #endregion

        private System.Windows.Forms.DataGridViewEx DGV;
        private System.Windows.Forms.DataGridViewTextBoxColumn DGV_requestColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn DGV_responseColumn;
    }
}
