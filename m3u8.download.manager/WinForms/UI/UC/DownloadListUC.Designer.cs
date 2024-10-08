﻿namespace m3u8.download.manager.ui
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
            this.DGV = new System.Windows.Forms.DataGridViewEx();            
            this.DGV_outputFileNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumnEx();
            this.DGV_outputDirectoryColumn = new System.Windows.Forms.DataGridViewTextBoxColumnEx();
            this.DGV_statusColumn = new System.Windows.Forms.DataGridViewTextBoxColumnEx();
            this.DGV_downloadProgressColumn = new System.Windows.Forms.DataGridViewTextBoxColumnEx();
            this.DGV_downloadTimeColumn = new System.Windows.Forms.DataGridViewTextBoxColumnEx();
            this.DGV_approxRemainedTimeColumn = new System.Windows.Forms.DataGridViewTextBoxColumnEx();
            this.DGV_downloadSpeedColumn = new System.Windows.Forms.DataGridViewTextBoxColumnEx();
            this.DGV_downloadBytesColumn = new System.Windows.Forms.DataGridViewTextBoxColumnEx();
            this.DGV_approxRemainedBytesColumn = new System.Windows.Forms.DataGridViewTextBoxColumnEx();
            this.DGV_approxTotalBytesColumn = new System.Windows.Forms.DataGridViewTextBoxColumnEx();
            this.DGV_isLiveStreamColumn = new System.Windows.Forms.DataGridViewTextBoxColumnEx();
            this.DGV_liveStreamMaxFileSizeColumn = new System.Windows.Forms.DataGridViewTextBoxColumnEx();
            this.DGV_requestHeadersColumn = new System.Windows.Forms.DataGridViewTextBoxColumnEx();
            this.DGV_urlColumn = new System.Windows.Forms.DataGridViewTextBoxColumnEx();            
            this.toolTip = new System.Windows.Forms.ToolTip();
            ((System.ComponentModel.ISupportInitialize)(this.DGV)).BeginInit();
            this.SuspendLayout();
            // 
            // DGV
            // 
            this.DGV.AllowUserToAddRows = false;
            this.DGV.AllowUserToDeleteRows = false;
            this.DGV.AllowUserToOrderColumns = true;
            this.DGV.GridColor = DefaultColors.DGV.GridLinesColor;
            this.DGV.BorderStyle = System.Windows.Forms.BorderStyle.None;
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
            this.DGV.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[]
            {
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
                this.DGV_isLiveStreamColumn,
                this.DGV_liveStreamMaxFileSizeColumn,
                this.DGV_requestHeadersColumn,
                this.DGV_urlColumn
            });
            cs2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            cs2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));            
            cs2.ForeColor = System.Drawing.SystemColors.ControlText;
            cs2.BackColor = System.Drawing.SystemColors.Window;            
            cs2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            cs2.SelectionBackColor = DefaultColors.DGV.SelectionBackColor_Suc; //---System.Drawing.Color.CadetBlue;
            cs2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.DGV.DefaultCellStyle = cs2;
            this.DGV.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DGV.Location = new System.Drawing.Point(0, 0);
            this.DGV.MultiSelect = true; //false;
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
            //---this.DGV.CurrentCellChanged += new System.EventHandler(this.DGV_CurrentCellChanged);
            this.DGV.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.DGV_CellPainting);
            this.DGV.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DGV_CellClick);
            this.DGV.CellMouseEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.DGV_CellMouseEnter);
            this.DGV.CellMouseLeave += new System.Windows.Forms.DataGridViewCellEventHandler(this.DGV_CellMouseLeave);
            this.DGV.CellMouseMove += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.DGV_CellMouseMove);
            this.DGV.ColumnHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.DGV_ColumnHeaderMouseClick);
            this.DGV.MouseDown +=new System.Windows.Forms.MouseEventHandler(this.DGV_MouseDown);
            this.DGV.MouseClick += new System.Windows.Forms.MouseEventHandler(this.DGV_MouseClick);
            this.DGV.DoubleClick += new System.EventHandler(this.DGV_DoubleClick);
            this.DGV.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.DGV_DataError);
            this.DGV.StartDrawSelectRect += new System.EventHandler(this.DGV_StartDrawSelectRect);
            this.DGV.EndDrawSelectRect += new System.EventHandler(this.DGV_EndDrawSelectRect);
            // 
            // DGV_outputFileNameColumn
            // 
            this.DGV_outputFileNameColumn.Width = 350;
            this.DGV_outputFileNameColumn.HeaderText = "Output file name";
            this.DGV_outputFileNameColumn.Name = "DGV_outputFileNameColumn";
            // 
            // DGV_outputDirectoryColumn
            // 
            this.DGV_outputDirectoryColumn.Width = 90;
            this.DGV_outputDirectoryColumn.HeaderText = "Output directory";
            this.DGV_outputDirectoryColumn.Name = "DGV_outputDirectoryColumn";
            // 
            // DGV_statusColumn
            // 
            this.DGV_statusColumn.Width = 80;
            this.DGV_statusColumn.HeaderText = "Status";
            this.DGV_statusColumn.Name = "DGV_statusColumn";
            // 
            // DGV_downloadProgressColumn
            // 
            this.DGV_downloadProgressColumn.Width = 125;
            this.DGV_downloadProgressColumn.HeaderText = "Progress";
            this.DGV_downloadProgressColumn.Name = "DGV_downloadProgress";
            // 
            // DGV_downloadTimeColumn
            // 
            this.DGV_downloadTimeColumn.Width = 80;
            this.DGV_downloadTimeColumn.HeaderText = "Time";
            this.DGV_downloadTimeColumn.Name = "DGV_downloadTimeColumn";
            // 
            // DGV_approxRemainedTimeColumn
            // 
            this.DGV_approxRemainedTimeColumn.Width = 80;
            this.DGV_approxRemainedTimeColumn.HeaderText = "Time Remained ~";
            this.DGV_approxRemainedTimeColumn.Name = "DGV_approxRemainedTimeColumn";
            // 
            // DGV_downloadSpeedColumn
            // 
            this.DGV_downloadSpeedColumn.Width = 80;
            this.DGV_downloadSpeedColumn.HeaderText = "Speed (↑Instant)";
            this.DGV_downloadSpeedColumn.Name = "DGV_downloadSpeedColumn";
            // 
            // DGV_downloadBytesColumn
            // 
            this.DGV_downloadBytesColumn.Width = 80;
            this.DGV_downloadBytesColumn.HeaderText = "Received (Size)";
            this.DGV_downloadBytesColumn.Name = "DGV_downloadBytesColumn";
            // 
            // DGV_approxRemainedBytesColumn
            // 
            this.DGV_approxRemainedBytesColumn.Width = 80;
            this.DGV_approxRemainedBytesColumn.HeaderText = "Remained ~ (Size)";
            this.DGV_approxRemainedBytesColumn.Name = "DGV_approxRemainedBytesColumn";
            // 
            // DGV_approxTotalBytesColumn
            // 
            this.DGV_approxTotalBytesColumn.Width = 80;
            this.DGV_approxTotalBytesColumn.HeaderText = "Total ~ (Size)";
            this.DGV_approxTotalBytesColumn.Name = "DGV_approxTotalBytesColumn";
            // 
            // DGV_isLiveStreamColumn
            // 
            this.DGV_isLiveStreamColumn.Width = 80;
            this.DGV_isLiveStreamColumn.Visible = false;
            this.DGV_isLiveStreamColumn.HeaderText = "Is Live Stream";
            this.DGV_isLiveStreamColumn.Name = "DGV_isLiveStreamColumn";
            this.DGV_isLiveStreamColumn.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            // 
            // DGV_liveStreamMaxFileSizeColumn
            // 
            this.DGV_liveStreamMaxFileSizeColumn.Width = 90;
            this.DGV_liveStreamMaxFileSizeColumn.Visible = false;
            this.DGV_liveStreamMaxFileSizeColumn.HeaderText = "Live Stream Max File Size";
            this.DGV_liveStreamMaxFileSizeColumn.Name = "DGV_liveStreamMaxFileSizeColumn";
            //---this.DGV_liveStreamMaxFileSizeColumn.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            // 
            // DGV_requestHeadersColumn
            // 
            this.DGV_requestHeadersColumn.Width = 800;
            this.DGV_requestHeadersColumn.Visible = false;
            this.DGV_requestHeadersColumn.HeaderText = "Request Headers";
            this.DGV_requestHeadersColumn.Name = "DGV_requestHeadersColumn";
            // 
            // DGV_urlColumn
            // 
            this.DGV_urlColumn.Width = 800;
            this.DGV_urlColumn.Visible = false;
            this.DGV_urlColumn.HeaderText = "Url";
            this.DGV_urlColumn.Name = "DGV_urlColumn";            
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

        private System.Windows.Forms.DataGridViewEx DGV;
        private System.Windows.Forms.DataGridViewTextBoxColumnEx DGV_outputFileNameColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumnEx DGV_outputDirectoryColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumnEx DGV_statusColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumnEx DGV_downloadProgressColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumnEx DGV_downloadTimeColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumnEx DGV_approxRemainedTimeColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumnEx DGV_downloadSpeedColumn;

        private System.Windows.Forms.DataGridViewTextBoxColumnEx DGV_downloadBytesColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumnEx DGV_approxRemainedBytesColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumnEx DGV_approxTotalBytesColumn;

        private System.Windows.Forms.DataGridViewTextBoxColumnEx DGV_isLiveStreamColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumnEx DGV_liveStreamMaxFileSizeColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumnEx DGV_requestHeadersColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumnEx DGV_urlColumn;

        private System.Windows.Forms.ToolTip toolTip;
    }
}
