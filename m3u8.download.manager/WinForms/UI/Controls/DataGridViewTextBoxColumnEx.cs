namespace System.Windows.Forms
{
    /// <summary>
    /// 
    /// </summary>
    internal class DataGridViewTextBoxColumnEx : DataGridViewTextBoxColumn
    {
        public DataGridViewTextBoxColumnEx() : base()
        {
            this.ReadOnly = true;
            this.SortMode = DataGridViewColumnSortMode.Programmatic;
        }

        new public int Width 
        {
            get => base.Width;
            set 
            {
                base.Width = value;
                if ( !_Init_Width.HasValue )
                {
                    _Init_Width = value;
                }
            }
        }

        private int? _Init_Width;
        public int Init_Width 
        { 
            get => _Init_Width.GetValueOrDefault( this.Width );
            set => _Init_Width = value; 
        }

        new public bool Visible
        {
            get => base.Visible;
            set
            {
                base.Visible = value;
                if ( _Is_Init_Visible_autoSet )
                {
                    Init_Visible = value;
                    _Is_Init_Visible_autoSet = false;
                }
            }
        }
        public bool Init_Visible { get; set; } = true;
        private bool _Is_Init_Visible_autoSet = true;
    }
}
