using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Forms;

using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;

namespace m3u8.download.manager.ui.infrastructure
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    internal sealed class DGVColumnWidth
    {
        [DataMember(Name="t")]  public string Text         { get; set; }
        [DataMember(Name="w")]  public int    Width        { get; set; }
        [DataMember(Name="di")] public int    DisplayIndex { get; set; }
        [DataMember(Name="v")]  public bool   Visible      { get; set; }

        public DGVColumnWidth() => Visible = true;
        public DGVColumnWidth( DataGridViewColumn column )
        {
            Text         = ((!string.IsNullOrEmpty( column.Name )) ? column.Name : column.HeaderText);
            Width        = column.Width;
            DisplayIndex = column.DisplayIndex;
            Visible      = column.Visible;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    internal sealed class DGVColumnWidths
    {
        [DataMember(Name="n")]   public string Name { get; set; }
        [DataMember(Name="cws")] public List< DGVColumnWidth > ColumnWidths { get; set; }

        public bool ShouldSerializeColumnWidths() => (0 < ColumnWidths.Count);

        public DGVColumnWidths() => ColumnWidths = new List< DGVColumnWidth >();
        public DGVColumnWidths( DataGridView dgv, bool skipNonResizableColumns = true )
        {
            Name = dgv.Name;
            ColumnWidths = new List< DGVColumnWidth >( dgv.Columns.Count );
            foreach ( DataGridViewColumn column in dgv.Columns )
            {
                if ( !skipNonResizableColumns || (column.Resizable != DataGridViewTriState.False) )
                {
                    ColumnWidths.Add( new DGVColumnWidth( column ) );
                }
            }
        }

        public void RestoreInDGV( DataGridView dgv, bool skipNonResizableColumns = true )
        {
            var hasAllDisplayIndex = (ColumnWidths.Select( o => o.DisplayIndex ).Distinct().Count() == dgv.Columns.Count);
            var dict = ColumnWidths.GroupBy( o => o.Text ).ToDictionary( g => g.Key, g => g.First() );
            
            foreach ( DataGridViewColumn column in dgv.Columns )
            {
                if ( !skipNonResizableColumns || (column.Resizable != DataGridViewTriState.False) )
                {
                    var name = (!string.IsNullOrEmpty( column.Name ) ? column.Name : column.HeaderText);
                    if ( dict.TryGetValue( name, out var x ) )
                    {
                        column.Width = x.Width;
                        if ( hasAllDisplayIndex )
                        {
                            column.DisplayIndex = x.DisplayIndex;
                            column.Visible      = x.Visible;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    internal sealed class SplitDistance
    {
        [DataMember(Name="n")] public string Name     { get; set; }
        [DataMember(Name="d")] public int    Distance { get; set; }

        public SplitDistance() => Distance = 200;
        internal SplitDistance( SplitContainer sc )
        {
            Name     = sc.Name;
            Distance = sc.SplitterDistance;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    internal sealed class FormData
    {
        [DataMember(Name="r")]  public Rectangle       Rect        { get; set; }
        [DataMember(Name="ws")] public FormWindowState WindowState { get; set; }

        [DataMember(Name="sds")]    public List< SplitDistance   > SplitDistances  { get; set; }
        [DataMember(Name="dgvcws")] public List< DGVColumnWidths > DGVColumnWidths { get; set; }

        /// <summary>
        /// https://www.newtonsoft.com/json/help/html/ConditionalProperties.htm
        /// </summary>
        public bool ShouldSerializeSplitDistances()  => (0 < SplitDistances.Count);
        public bool ShouldSerializeDGVColumnWidths() => (0 < DGVColumnWidths.Count);

        public FormData()
        {
            Rect            = default;
            WindowState     = FormWindowState.Normal;
            SplitDistances  = new List< SplitDistance  >();
            DGVColumnWidths = new List< DGVColumnWidths >();
        }
        internal FormData( Rectangle r, FormWindowState ws ) : this()
        {
            Rect        = r;
            WindowState = ws;
        }


        [IgnoreDataMember] private Dictionary< string, DGVColumnWidths > _dgvcws_Dict;
        [M(O.AggressiveInlining)] public DGVColumnWidths TryGetDGVColumnWidths( string name )
        {
            if ( (name == null) || (DGVColumnWidths.Count == 0) ) return (null);

            if ( _dgvcws_Dict == null )
            {
                _dgvcws_Dict = DGVColumnWidths.GroupBy( o => o.Name ).ToDictionary( g => g.Key, g => g.First() );
            }
            return (_dgvcws_Dict.TryGetValue( name, out var dgvcws ) ? dgvcws : null);
        }

        [IgnoreDataMember] private Dictionary< string, SplitDistance > _sds_Dict;
        [M(O.AggressiveInlining)] public SplitDistance TryGetSplitDistance( string name )
        {
            if ( (name == null) || (SplitDistances.Count == 0) ) return (null);

            if ( _sds_Dict == null )
            {
                _sds_Dict = SplitDistances.GroupBy( o => o.Name ).ToDictionary( g => g.Key, g => g.First() );
            }
            return (_sds_Dict.TryGetValue( name, out var sd ) ? sd : null);
        }

        [M(O.AggressiveInlining)] public bool IsEmptyLists() => ((0 == SplitDistances.Count) && (0 == DGVColumnWidths.Count));
    }

    /// <summary>
    /// 
    /// </summary>
    internal static class FormPositionStorer
    {
        private const int MIN_HEIGHT = 200;
        private const int MIN_WIDTH  = 250;

        private static void CorrectPosition( this Form form, int? minWidth = null, int? minHeight = null )
        {
            Rectangle workingArea = Screen.GetWorkingArea( form );
            if ( form.Top < workingArea.Top )
            {
                form.Top = workingArea.Top;
            }
            if ( form.Left < workingArea.Left )
            {
                form.Left = workingArea.Left;
            }
            if ( workingArea.Bottom < form.Bottom )
            {
                form.Height = workingArea.Bottom - form.Top;
            }
            if ( workingArea.Right < form.Right )
            {
                form.Width = workingArea.Right - form.Left;
            }

            var min_height = minHeight.GetValueOrDefault( MIN_HEIGHT );
            if ( form.Height < min_height )
            {
                form.Top    -= min_height - form.Height;
                form.Height += min_height - form.Height;
                form.CorrectPosition( minWidth, minHeight );
            }

            var min_width = minWidth.GetValueOrDefault( MIN_WIDTH );
            if ( form.Width < min_width )
            {
                form.Left  -= min_width - form.Width;
                form.Width += min_width - form.Width;
                form.CorrectPosition( minWidth, minHeight );
            }
        }

        private static void save_Recurrent( this Control parent, FormData d )
        {
            #region [.DGV //-1-//.]
            var dgv = parent as DataGridView;
            if ( dgv != null )
            {
                d.DGVColumnWidths.Add( new DGVColumnWidths( dgv ) );
                goto NEXT;
            }
            #endregion

            #region [.SplitContainer //-2-//.]
            var sc = parent as SplitContainer;
            if ( sc != null )
            {
                d.SplitDistances.Add( new SplitDistance( sc ) );
                goto NEXT;
            }
            #endregion

        NEXT:
            foreach ( Control c in parent.Controls )
            {
                c.save_Recurrent( d );
            }
        }
        private static void load_Recurrent( this Control parent, FormData d )
        {
            #region [.DGV //-1-//.]
            var dgv = parent as DataGridView;
            if ( dgv != null )
            {
                var dgvcws = d.TryGetDGVColumnWidths( dgv.Name );
                if ( dgvcws != null )
                {
                    dgvcws.RestoreInDGV( dgv );
                    goto NEXT;
                }
            }
            #endregion

            #region [.SplitContainer //-2-//.]
            var sc = parent as SplitContainer;
            if ( sc != null )
            {
                var sd = d.TryGetSplitDistance( sc.Name );
                if ( sd != null )
                {
                    sc.SplitterDistance = ((sc.Panel1MinSize <= sd.Distance) ? sd.Distance : sc.Panel1MinSize);
                    goto NEXT;
                }
            }
            #endregion

        NEXT:
            foreach ( Control c in parent.Controls )
            {
                c.load_Recurrent( d );
            }
        }

        public static void LoadOnlyHeight( Form form, string formPositionJson )
        {
            if ( string.IsNullOrWhiteSpace( formPositionJson ) ) return;

            try
            {
                //---------------------------------------------------------------------------//
                var d = Extensions.FromJSON< FormData >( formPositionJson ); //---var d = JsonConvert.DeserializeObject< FormData >( formPositionJson );
                //---------------------------------------------------------------------------//

                #region [.Form position.]
                if ( d.WindowState == FormWindowState.Normal )
                {
                    //form.Bounds = d.Rect;
                    form.Bounds = new Rectangle( form.Bounds.Location, new Size( form.Bounds.Width, d.Rect.Height ) );
                }
                else
                {
                    form.SetBounds( d.Rect.X, d.Rect.Y, d.Rect.Width, d.Rect.Height, BoundsSpecified.Height );
                }
                if ( d.WindowState != FormWindowState.Minimized )
                {
                    form.WindowState = d.WindowState;
                }

                if ( form.WindowState == FormWindowState.Normal )
                {
                    form.CorrectPosition();
                }
                #endregion
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );
            }
        }
        public static void LoadAllExcludeHeight( Form form, string formPositionJson, int? minWidth = null, int? minHeight = null )
        {
            if ( string.IsNullOrWhiteSpace( formPositionJson ) ) return;

            try
            {
                //---------------------------------------------------------------------------//
                var d = Extensions.FromJSON< FormData >( formPositionJson ); //---var d = JsonConvert.DeserializeObject< FormData >( formPositionJson );
                //---------------------------------------------------------------------------//

                #region [.Form position.]
                if ( d.WindowState == FormWindowState.Normal )
                {
                    //form.Bounds = d.Rect;
                    form.Bounds = new Rectangle( form.Bounds.Location, new Size( d.Rect.Width, form.Bounds.Height ) );
                }
                else
                {
                    form.SetBounds( d.Rect.X, d.Rect.Y, d.Rect.Width, d.Rect.Height, BoundsSpecified.Location | BoundsSpecified.Width );
                }
                if ( d.WindowState != FormWindowState.Minimized )
                {
                    form.WindowState = d.WindowState;
                }

                if ( form.WindowState == FormWindowState.Normal )
                {
                    form.CorrectPosition( minWidth, minHeight );
                }
                #endregion
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );
            }
        }
        public static void Load( Form form, string formPositionJson )
        {
            if ( string.IsNullOrWhiteSpace( formPositionJson ) ) return;

            try
            {
                //---------------------------------------------------------------------------//
                var d = Extensions.FromJSON< FormData >( formPositionJson ); //---var d = JsonConvert.DeserializeObject< FormData >( formPositionJson );
                //---------------------------------------------------------------------------//

                #region [.Form position.]
                if ( d.WindowState == FormWindowState.Normal )
                {
                    form.Bounds = d.Rect;
                }
                else
                {
                    form.SetBounds( d.Rect.X, d.Rect.Y, d.Rect.Width, d.Rect.Height, BoundsSpecified.All );
                }
                if ( d.WindowState != FormWindowState.Minimized )
                {
                    form.WindowState = d.WindowState;
                }
                if ( form.WindowState == FormWindowState.Normal )
                {
                    form.CorrectPosition();
                }
                #endregion

                if ( !d.IsEmptyLists() )
                {
                    form.load_Recurrent( d );
                }
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );
            }
        }        

        private static string Save( Form form, bool saveOnlyFormPosition, int? height = null )
        {
            try
            {
                var rc = ((form.WindowState == FormWindowState.Normal) ? form.Bounds : form.RestoreBounds);
                if ( height.HasValue )
                {
                    rc.Height = height.Value;
                }
                var d = new FormData( rc, form.WindowState );

                if ( !saveOnlyFormPosition )
                {
                    form.save_Recurrent( d );
                }

                //------------------------------------------------------------------//
                var json = d.ToJSON(); //---var json = JsonConvert.SerializeObject( d, Formatting.None );
                return (json);
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );
                return (null);
            }
        }
        public static string Save( Form form ) => Save( form, false );
        public static string SaveOnlyPos( Form form ) => Save( form, true );
        public static string SaveOnlyPos( Form form, int height ) => Save( form, true, height );
    }
}
