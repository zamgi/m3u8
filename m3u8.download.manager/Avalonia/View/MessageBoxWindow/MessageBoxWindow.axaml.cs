using System;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;

using m3u8.download.manager.ui;

namespace m3u8.download.manager;

/// <summary>
/// 
/// </summary>
public partial class MessageBoxWindow : Window
{
    /// <summary>
    /// 
    /// </summary>
    [Flags]
    public enum ButtonTypeEnum
    {
        None   = 0,
        Ok     = 0x1,
        Cancel = (0x1 << 1),
        Yes    = (0x1 << 2),
        No     = (0x1 << 3),

        OkCancel = Ok | Cancel,
        YesNo    = Yes | No,
    }
    /// <summary>
    /// 
    /// </summary>
    public enum IconTypeEnum
    {
        None,
        Warning,
        Error,
        Info,
        Question,

        m3u8,
    }


    private ButtonTypeEnum _DialogResult;
    public MessageBoxWindow() => InitializeComponent();

    public static Task< ButtonTypeEnum > Show( 
        string text, string caption, ButtonTypeEnum buttons, IconTypeEnum icon, FontFamily fontFamily = null, Size? size = null ) => Show( null, text, caption, buttons, icon, fontFamily, size );
    public static async Task< ButtonTypeEnum > Show( 
          Window owner
        , string text
        , string caption
        , ButtonTypeEnum buttons
        , IconTypeEnum iconType
        , FontFamily fontFamily = null
        , Size? size = null )
    {
        var wnd = new MessageBoxWindow()
        {
            Message       = text,
            Caption       = caption,
            Buttons       = buttons,
            IconType      = iconType,
            SizeToContent = size.HasValue ? SizeToContent.Manual : SizeToContent.WidthAndHeight,
        };
        if ( fontFamily != null )
        {
            wnd.FontFamily = fontFamily;
        }
        if ( size.HasValue )
        {
            var sz = size.Value;
            (wnd.Width, wnd.Height) = (sz.Width, sz.Height);
            //wnd.Opened += (_, _) => { (wnd.Width, wnd.Height) = (sz.Width, sz.Height); };
            //wnd.Loaded += (_, _) => { (wnd.Width, wnd.Height) = (sz.Width, sz.Height); };
        }

        /*
        if ( buttons.HasFlag( ButtonTypeEnum.Cancel ) || (buttons == ButtonTypeEnum.Ok) || (buttons == ButtonTypeEnum.None) )
        {
            wnd.KeyDown += (_, e) => { if ( e.Key == Key.Escape ) wnd.Close(); };
        }
        //*/

        var topWnd = owner ?? Extensions.GetTopWindow();
        if ( topWnd != null )
        {            
            await wnd.ShowDialog( topWnd );
        }
        else
        {
            wnd.Topmost = true;
            wnd.Show();
        }

        return (wnd._DialogResult);
    }

    protected override void OnOpened( EventArgs e )
    {
        base.OnOpened( e );

        static void focus_button( Button button )
        {
            button.Focus();
            button.FocusAndBlinkBackColor( Brushes.Khaki, millisecondsDelay: 250 );
        }

        if ( _Buttons == ButtonTypeEnum.YesNo )
        {
            focus_button( this.yesButton );
        }
        else if ( (_Buttons == ButtonTypeEnum.OkCancel) || (_Buttons == ButtonTypeEnum.Ok) )
        {
            focus_button( this.okButton );
        }
    }
    protected override void OnKeyDown( KeyEventArgs e )
    {
        base.OnKeyDown( e );

        switch ( e.Key )
        {
            case Key.Enter: 
                if ( _Buttons.HasFlag( ButtonTypeEnum.Ok ) || _Buttons.HasFlag( ButtonTypeEnum.Yes ) )
                {
                    _DialogResult = _Buttons.HasFlag( ButtonTypeEnum.Yes ) ? ButtonTypeEnum.Yes : ButtonTypeEnum.Ok;
                    e.Handled = true;
                    this.Close();
                }
                break;
                
            case Key.Escape:
                if ( _Buttons.HasFlag( ButtonTypeEnum.Cancel ) || _Buttons.HasFlag( ButtonTypeEnum.No ) || 
                     (_Buttons == ButtonTypeEnum.Ok) || (_Buttons == ButtonTypeEnum.None) 
                   )
                {
                         if ( _Buttons.HasFlag( ButtonTypeEnum.Cancel ) ) _DialogResult = ButtonTypeEnum.Cancel;
                    else if ( _Buttons.HasFlag( ButtonTypeEnum.No     ) ) _DialogResult = ButtonTypeEnum.No;
                    else if ( _Buttons == ButtonTypeEnum.Ok             ) _DialogResult = ButtonTypeEnum.None/*Ok*/;
                    else _DialogResult = ButtonTypeEnum.None;

                    e.Handled = true;
                    this.Close();
                }
                break;
        }
    }

    public string Caption
    {
        get => this.Title;
        set => this.Title = value;
    }
    public string Message
    {
        get => this.messageTextBlock.Text;
        set => this.messageTextBlock.Text = value;
    }


    private ButtonTypeEnum _Buttons = ButtonTypeEnum.Ok | ButtonTypeEnum.Cancel;
    public ButtonTypeEnum Buttons
    {
        get => _Buttons;
        set
        {
            if ( _Buttons != value )
            {
                _Buttons = value;
                this.okButton    .IsVisible = _Buttons.HasFlag( ButtonTypeEnum.Ok );
                this.cancelButton.IsVisible = _Buttons.HasFlag( ButtonTypeEnum.Cancel );
                this.yesButton   .IsVisible = _Buttons.HasFlag( ButtonTypeEnum.Yes );
                this.noButton    .IsVisible = _Buttons.HasFlag( ButtonTypeEnum.No );
            }
        }
    }


    private IconTypeEnum _IconType;
    public IconTypeEnum IconType
    {
        get => _IconType;
        set
        {
            if ( _IconType != value )
            {
                //this.iconImage.IsVisible = (value != IconTypeEnum.None);
                var resourcePath = value switch
                {
                    IconTypeEnum.Warning  => "/Resources/MessageBoxIcons/warning.png",
                    IconTypeEnum.Error    => "/Resources/MessageBoxIcons/error.png",
                    IconTypeEnum.Question => "/Resources/MessageBoxIcons/question.png",
                    IconTypeEnum.Info     => "/Resources/MessageBoxIcons/info.png",
                    IconTypeEnum.m3u8     => "/Resources/m3u8_32x36.ico",
                    IconTypeEnum.None     => null, _ => null
                };

                //var uri    = new Uri( "avares://MyProjectName/Assets/my-image.png" );
                //var bitmap = new Bitmap( AssetLoader.Open( uri ) );
                this.iconImage.Source = (resourcePath != null) ? new Bitmap( ResourceLoader._GetResource_( resourcePath ) ) : null;

                _IconType = value;
            }
        }
    }

    private void okButton_Click( object sender, RoutedEventArgs e ) => CloseDialog( ButtonTypeEnum.Ok );
    private void cancelButton_Click( object? sender, RoutedEventArgs e ) => CloseDialog( ButtonTypeEnum.Cancel );
    private void yesButton_Click( object? sender, RoutedEventArgs e ) => CloseDialog( ButtonTypeEnum.Yes );
    private void noButton_Click( object? sender, RoutedEventArgs e ) => CloseDialog( ButtonTypeEnum.No );
    private void CloseDialog( ButtonTypeEnum dialogResult )
    {
        _DialogResult = dialogResult; 
        this.Close();
    }
}