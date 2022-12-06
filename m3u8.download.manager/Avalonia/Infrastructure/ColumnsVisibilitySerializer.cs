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
    internal static class ColumnsVisibilitySerializer
    {
        /// <summary>
        /// 
        /// </summary>
        [DataContract] private sealed class Obj_4_Serialize
        {
            public Obj_4_Serialize() { }
            public Obj_4_Serialize( DataGridColumn col )
            {
                ColumnText = col.GetTextEx();
                IsVisible  = col.IsVisible;
            }
            [DataMember(Name="t")] public string ColumnText { [M(O.AggressiveInlining)] get; set; }
            [DataMember(Name="v")] public bool   IsVisible  { [M(O.AggressiveInlining)] get; set; }
        }
        
        public static string ToJSON( IEnumerable< DataGridColumn > columns ) => columns.Select( col => new Obj_4_Serialize( col ) ).ToJSON();
        public static IEnumerable< (string ColumnText, bool IsVisible) > FromJSON( string json )
        {
            if ( !json.IsNullOrWhiteSpace() )
            {
                try
                {
                    var rows = Extensions.FromJSON< Obj_4_Serialize[] >( json ).Select( x => (x.ColumnText, x.IsVisible) );
                    return (rows);
                }
                catch ( Exception ex )
                {
                    Debug.WriteLine( ex );
                }
            }
            return (Enumerable.Empty< (string ColumnText, bool IsVisible) >());
        }

        public static string GetTextEx( this DataGridColumn col ) => (col.Header is TextBlock t) ? t.Text : $"Column_DisplayIndex_{col.DisplayIndex}"/*(col.Header?.ToString() ?? "?")*/;
        public static void Restore( IEnumerable< DataGridColumn > columns, string json )
        {
            var d = FromJSON( json ).GroupBy( t => t.ColumnText ).ToDictionary( g => g.Key, g => g.First().IsVisible );
            foreach ( var col in columns )
            {
                if ( d.TryGetValue( col.GetTextEx(), out var isVisible ) )
                {
                    col.IsVisible = isVisible;
                }
            }
        }
        public static void RestoreVisibility( this IEnumerable< DataGridColumn > columns, string json ) => Restore( columns, json );
    }
}
