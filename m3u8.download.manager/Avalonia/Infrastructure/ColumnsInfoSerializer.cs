using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

using Avalonia.Controls;
using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;

namespace m3u8.download.manager
{
    /// <summary>
    /// 
    /// </summary>
    internal static class ColumnsInfoSerializer
    {
        /// <summary>
        /// 
        /// </summary>
        [DataContract] private sealed class Obj_4_Serialize
        {
            public Obj_4_Serialize() { }
            public Obj_4_Serialize( DataGridColumn col, int index )
            {
                ColumnText   = col.GetTextEx();
                IsVisible    = col.IsVisible;
                Width        = col.ActualWidth;
                //DisplayIndex = col.DisplayIndex;
                //Index        = index;
            }
            [DataMember(Name="t")] public string ColumnText   { [M(O.AggressiveInlining)] get; set; }
            [DataMember(Name="v")] public bool   IsVisible    { [M(O.AggressiveInlining)] get; set; }
            [DataMember(Name="w")] public double Width        { [M(O.AggressiveInlining)] get; set; }
            //[DataMember(Name="d")] public int    DisplayIndex { [M(O.AggressiveInlining)] get; set; }
            //[DataMember(Name="i")] public int    Index        { [M(O.AggressiveInlining)] get; set; }
        }
        
        public static string ToJSON( IEnumerable< DataGridColumn > columns ) => columns.Select( (col, i) => new Obj_4_Serialize( col, i ) ).ToJSON();
        public static IEnumerable< (string ColumnText, bool IsVisible, double Width) > FromJSON( string json )
        {
            if ( !json.IsNullOrWhiteSpace() )
            {
                try
                {
                    var rows = Extensions.FromJSON< Obj_4_Serialize[] >( json ).Select( x => (x.ColumnText, x.IsVisible, x.Width) );
                    return (rows);
                }
                catch ( Exception ex )
                {
                    Debug.WriteLine( ex );
                }
            }
            return (Enumerable.Empty< (string ColumnText, bool IsVisible, double Width) >());
        }

        public static string GetTextEx( this DataGridColumn col ) => (col.Header is TextBlock t) ? t.Text : $"Column_DisplayIndex_{col.DisplayIndex}"/*(col.Header?.ToString() ?? "?")*/;
        public static void Restore( this IEnumerable< DataGridColumn > columns, string json )
        {
            var d = FromJSON( json ).GroupBy( t => t.ColumnText ).ToDictionary( g => g.Key, g => g.First() );
            foreach ( var col in columns )
            {
                if ( d.TryGetValue( col.GetTextEx(), out var t ) )
                {
                    col.IsVisible = t.IsVisible;
                    col.Width     = new DataGridLength( t.Width );
                }
            }
        }
        public static void Restore_NoThrow( this IEnumerable< DataGridColumn > columns, string json )
        {
            if ( !json.IsNullOrEmpty() )
            {
                try
                {
                    Restore( columns, json );
                }
                catch ( Exception ex )
                {
                    Debug.WriteLine( ex );
                }
            }
        }
    }
}
