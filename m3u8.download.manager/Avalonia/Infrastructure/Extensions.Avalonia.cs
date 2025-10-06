using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;

using MsBox.Avalonia;
using MsBox.Avalonia.Base;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;

using m3u8.download.manager.controllers;
using m3u8.download.manager.models;
using m3u8.download.manager.ui;

using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;

namespace m3u8.download.manager
{
    /// <summary>
    /// 
    /// </summary>
    internal static partial class Extensions
    {
        public static string GetDownloadListColumnsInfoJson( this SettingsPropertyChangeController settingsController ) => settingsController.Settings.DownloadListColumnsInfoJson;
        public static void SetDownloadListColumnsInfoJson( this SettingsPropertyChangeController settingsController, string json ) => settingsController.Settings.DownloadListColumnsInfoJson = json;

        public static Window GetMainWindow() => ((Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) ? desktop.MainWindow : null);
        public static Window GetTopWindow() => ((Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) ? (desktop.Windows.LastOrDefault() ?? desktop.MainWindow) : null);

        public static Task ShowDialogEx( this Window dialog ) => dialog.ShowDialog( GetMainWindow() );

        #region [.MessageBox's.]
        public static IMsBox< ButtonResult > Create_MsBoxStandardWindow( string text, string caption, ButtonEnum buttons, Icon icon, FontFamily fontFamily = null )
        {
            static ClickEnum get_EnterDefaultButton( ButtonEnum buttons ) => buttons switch
            {
                ButtonEnum.Ok          => ClickEnum.Ok,
                ButtonEnum.YesNo       => ClickEnum.Yes,
                ButtonEnum.OkCancel    => ClickEnum.Ok,
                ButtonEnum.OkAbort     => ClickEnum.Ok,
                ButtonEnum.YesNoCancel => ClickEnum.Yes,
                ButtonEnum.YesNoAbort  => ClickEnum.Yes,
                _ => ClickEnum.Ok,
            };
            static ClickEnum get_EscDefaultButton( ButtonEnum buttons ) => buttons switch
            { 
                ButtonEnum.Ok          => ClickEnum.Ok,
                ButtonEnum.YesNo       => ClickEnum.No,
                ButtonEnum.OkCancel    => ClickEnum.Cancel,
                ButtonEnum.OkAbort     => ClickEnum.Abort,
                ButtonEnum.YesNoCancel => ClickEnum.Cancel,
                ButtonEnum.YesNoAbort  => ClickEnum.Abort,
                _ => ClickEnum.Cancel,
            };

            var p = new MessageBoxStandardParams()
            { 
                ButtonDefinitions     = buttons,
                EnterDefaultButton    = get_EnterDefaultButton( buttons ),
                EscDefaultButton      = get_EscDefaultButton  ( buttons ),
                Icon                  = icon,
                ContentTitle          = caption,
                ContentMessage        = text,
                CanResize             = true,
                WindowIcon            = new WindowIcon( ResourceLoader._GetResource_( "/Resources/m3u8_32x36.ico" ) ),
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
            };
            if ( fontFamily != null ) p.FontFamily = fontFamily;

            var msgbox = MessageBoxManager.GetMessageBoxStandard( p );

            #region comm. not-allowed anymore. [.adjustment of the created window (through reflection).]
            /*
            var window_field = msgbox.GetType().GetField( "_window", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic );
            if ( (window_field != null) && (window_field.GetValue( msgbox ) is Window window) )
            {
                window.ShowInTaskbar = false;
                #region comm.
                //window.Opened += async (s, e) =>
                //{
                //    var w = window.Width;
                //    window.Width = w - 1;
                //    await Task.Delay( 1 );
                //    window.Width = w;
                //};
                #endregion
            }
            //*/
            #endregion

            #region [.by reflection get inner for KeyDown-event.]
            var view_field = msgbox.GetType().GetField( "_view", BindingFlags.Instance | BindingFlags.NonPublic );
            if ( (view_field != null) && (view_field.GetValue( msgbox ) is UserControl uc) )
            {
                uc.AttachedToVisualTree += (_s, _e) =>
                {
                    var wnd = (Window) _e.Root; //(Window) uc.GetVisualRoot();
                    var keyDown = new EventHandler< KeyEventArgs >((s, e) =>
                    {
                        if ( e.Key == Key.Escape )
                        {
                            e.Handled = true; 
                            wnd.Close();
                        }
                    });
                    uc .KeyDown += keyDown;
                    wnd.KeyDown += keyDown;
                };

            }
            #endregion

            return (msgbox);
        }
        public static async Task< ButtonResult > ShowEx( this IMsBox< ButtonResult > msgbox )
        {
            var window = GetTopWindow();
            if ( window != null )
            {
                return (await msgbox.ShowWindowDialogAsync( window ));
            }
            else
            {
                return (await msgbox.ShowWindowAsync());
            }
        }

        public static Task MessageBox_Show( string text, string caption, Icon icon ) => MessageBox_ShowWithOkButton( text, caption, icon );
        public static Task MessageBox_ShowError( string text, string caption ) => MessageBox_ShowWithOkButton( text, caption, Icon.Error );
        public static Task MessageBox_ShowError( this Exception ex, string caption ) => MessageBox_ShowError( ex.ToString(), caption );
        public static Task MessageBox_ShowInformation( this Window window, string text, string caption ) => Create_MsBoxStandardWindow( text, caption, ButtonEnum.Ok, Icon.Info ).ShowWindowDialogAsync( window );
        public static Task MessageBox_ShowError( this Window window, string text, string caption ) => Create_MsBoxStandardWindow( text, caption, ButtonEnum.Ok, Icon.Error ).ShowWindowDialogAsync( window );
        public static Task< ButtonResult > MessageBox_ShowQuestion( this Window window, string text, string caption, ButtonEnum buttons = ButtonEnum.YesNo ) => Create_MsBoxStandardWindow( text, caption, buttons, Icon.Info ).ShowWindowDialogAsync( window );
        private static async Task MessageBox_ShowWithOkButton( string text, string caption, Icon icon )
        {
            var msgbox = Create_MsBoxStandardWindow( text, caption, ButtonEnum.Ok, icon );
            var window = GetTopWindow();
            if ( window != null )
            {
                await msgbox.ShowWindowDialogAsync( window );
            }
            else
            {
                await msgbox.ShowAsync();
            }
        }
        #endregion

        #region [.allowed Command by current status.]
        [M(O.AggressiveInlining)] public static bool StartDownload_IsAllowed ( this DownloadStatus status ) => (status == DownloadStatus.Created ) ||
                                                                                                               (status == DownloadStatus.Paused  ) ||
                                                                                                               (status == DownloadStatus.Canceled) ||
                                                                                                               (status == DownloadStatus.Finished) ||
                                                                                                               (status == DownloadStatus.Error   );
        [M(O.AggressiveInlining)] public static bool CancelDownload_IsAllowed( this DownloadStatus status ) => (status == DownloadStatus.Started ) ||
                                                                                                               (status == DownloadStatus.Running ) ||
                                                                                                               (status == DownloadStatus.Wait    ) ||
                                                                                                               (status == DownloadStatus.Paused  );
        [M(O.AggressiveInlining)] public static bool PauseDownload_IsAllowed ( this DownloadStatus status ) => (status == DownloadStatus.Started ) ||
                                                                                                               (status == DownloadStatus.Running );
        #endregion

        [M(O.AggressiveInlining)] public static T Find_Ex< T >( this Window window, string name ) where T : class => window.Find< T >( name ) ?? (window.TryFindResource( name, out var x ) ? (T) x : null);       
        [M(O.AggressiveInlining)] public static T Find_Ex< T >( this UserControl uc, string name ) where T : class => uc.Find< T >( name ) ?? (uc.TryFindResource( name, out var x ) ? (T) x : null);        
        [M(O.AggressiveInlining)] public static MenuItem Find_MenuItem( this ContextMenu contextMenu, string name ) => contextMenu.Items.Cast< MenuItem >().First( m => m.Name == name );

        [M(O.AggressiveInlining)] public static Rect GetBoundsByTopAncestor( this Visual v, double? width = null )
        {
            var b_1 = v.Bounds;
            var pt  = b_1.Position;
            foreach ( var p in v.GetVisualAncestors() )
            {
                var b_2 = p.Bounds;
                pt = new Point( pt.X + b_2.X, pt.Y + b_2.Y );
            }
            var sz = new Size( width.GetValueOrDefault( b_1.Size.Width ), b_1.Size.Height );
            return (new Rect( pt, sz ));
        }

        //[M(O.AggressiveInlining)] public static bool TryGet< T >( this IDataObject data, out T t )
        //{
        //    if ( data.Get( nameof(T) ) is T _t )
        //    {
        //        t = _t;
        //        return (true);
        //    }
        //    t = default;
        //    return (false);
        //}
        //[M(O.AggressiveInlining)] public static void Set< T >( this DataObject data, T t ) => data.Set( nameof(T), t );


        [M(O.AggressiveInlining)] public static T GetSelectedDownloadRow< T >( this DataGrid dgv ) where T : class => (dgv.SelectedItem as T);
        [M(O.AggressiveInlining)] public static IReadOnlyList< T > GetSelectedDownloadRows< T >( this DataGrid dgv )
        {
            var srs = dgv.SelectedItems;
            var lst = new List< T >( srs.Count );
            foreach ( var row in srs.Cast< T >() )
            {
                lst.Add( row );
            }
            return (lst);
        }

        public static bool IsFocused_SelfOrDescendants( this Visual v ) => v.GetSelfAndVisualDescendants().OfType< InputElement >().Any( e => e.IsFocused );

        public static bool TryParse2DataGridLength( this string v, out DataGridLength gridLength, double displayValue = 0 )
        {
            if ( v.EqualIgnoreCase( "auto" ) )
            {
                gridLength = new DataGridLength( 1, DataGridLengthUnitType.Auto, desiredValue: 0, displayValue );
                return (true);
            }
            if ( v.EqualIgnoreCase( "*" ) )
            {
                gridLength = new DataGridLength( 1, DataGridLengthUnitType.Star, desiredValue: 0, displayValue );
                return (true);
            }
            if ( double.TryParse( v, out var d ) && (0 < d) )
            {
                gridLength = new DataGridLength( d, DataGridLengthUnitType.Pixel/*, desiredValue: 0, displayValue*/ );
                return (true);
            }
            if ( v.EndsWith( "*" ) && double.TryParse( v.Substring( 0, v.Length - 1 ), out d ) && (0 < d) )
            {
                gridLength = new DataGridLength( d, DataGridLengthUnitType.Star, desiredValue: 0, displayValue );
                return (true);
            }
#if DEBUG
            else if ( Debugger.IsAttached )
            {
                Debugger.Break();
            }
#endif
            gridLength = default;
            return (false);
        }
    }
}
