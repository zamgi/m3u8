using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;

using m3u8.infrastructure;

using _Resources_ = m3u8.download.manager.Properties.Resources;
using _SC_        = m3u8.download.manager.controllers.SettingsPropertyChangeController;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class WebProxyUC : UserControl
    {
        /// <summary>
        /// 
        /// </summary>
        public delegate void WebProxyChangedEventHandler( bool used, string addressRaw );

        public event WebProxyChangedEventHandler OnWebProxyChanged;

        #region [.fields.]
        private StackPanel editWebProxyStackPanel;
        private CheckBox torBrowserSocks5CheckBox;
        private CheckBox torServerSocks5CheckBox;
        private CheckBox socks5CheckBox;
        private CheckBox httpCheckBox;

        private TextBox addressTextBox;
        private TextBox portTextBox;
        private TextBox userNameTextBox;
        private TextBox passwordTextBox;

        private Button testConnectionButton;

        private _SC_ _SC;
        #endregion

        #region [.ctor().]
        private IBrush _UncheckColor   = Brushes.Gray;
        private IBrush _BlinkBackColor = Brushes.Khaki;
        public WebProxyUC() => this.InitializeComponent();
        internal void SetSettingsController( _SC_ sc ) => _SC = sc ?? throw (new ArgumentNullException());

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load( this );

            editWebProxyStackPanel   = this.Find< StackPanel >( nameof(editWebProxyStackPanel) );
            torBrowserSocks5CheckBox = this.Find< CheckBox >( nameof(torBrowserSocks5CheckBox) ); torBrowserSocks5CheckBox.IsCheckedChanged += torBrowserSocks5CheckBox_CheckedChanged;
            torServerSocks5CheckBox  = this.Find< CheckBox >( nameof(torServerSocks5CheckBox) );  torServerSocks5CheckBox .IsCheckedChanged += torServerSocks5CheckBox_CheckedChanged;
            socks5CheckBox           = this.Find< CheckBox >( nameof(socks5CheckBox) );           socks5CheckBox          .IsCheckedChanged += socks5CheckBox_CheckedChanged;
            httpCheckBox             = this.Find< CheckBox >( nameof(httpCheckBox) );             httpCheckBox            .IsCheckedChanged += httpCheckBox_CheckedChanged;

            addressTextBox  = this.Find< TextBox >( nameof(addressTextBox) ); addressTextBox.TextChanged += addressOrPortTextBox_TextChanged;
            portTextBox     = this.Find< TextBox >( nameof(portTextBox) );    portTextBox   .TextChanged += addressOrPortTextBox_TextChanged;
            userNameTextBox = this.Find< TextBox >( nameof(userNameTextBox) );
            passwordTextBox = this.Find< TextBox >( nameof(passwordTextBox) );

            testConnectionButton = this.Find< Button >( nameof(testConnectionButton) ); testConnectionButton.Click += testConnectionButton_Click;
        }

        //protected override void OnLoaded( RoutedEventArgs e )
        //{
        //    base.OnLoaded( e );

        //    CorrectCheckBoxesStyle();
        //    DGV.Focus();
        //}
        protected override void OnKeyDown( KeyEventArgs e )
        {
            //switch ( e.Key )
            //{
            //    case Key.Space:
            //        if ( DGV.IsFocused_SelfOrDescendants() )
            //        {
            //            TryCheckByKey();
            //        }
            //        break;

            //    case Key.Insert:
            //        if ( DGV.IsFocused_SelfOrDescendants() )
            //        {
            //            addRowMenuItem_Click( null, EventArgs.Empty );
            //        }
            //        break;

            //    case Key.Delete:
            //        if ( DGV.IsFocused_SelfOrDescendants() )
            //        {
            //            deleteRowMenuItem_Click( null, EventArgs.Empty );
            //        }
            //        break;

            //    case Key.PageDown:
            //    case Key.PageUp:
            //        e.Handled = ((e.KeyModifiers & KeyModifiers.Control) == KeyModifiers.Control);
            //        return;
            //}

            base.OnKeyDown( e );
        }
        #endregion

        #region [.public methods.]
        public web_proxy_info GetWebProxyInfo()
        {
            var addressRaw = addressTextBox.Text?.Trim() ?? string.Empty;

            var suc = UrlHelper.TryParseWebProxyUrl( addressRaw, out var wpi, out _ );
            if ( suc )
            {
                return (wpi);
            }

            var t = default((string Hostname, int? Port, Exception Error));
            var sepIdx = addressRaw.LastIndexOf(':');
            if ( sepIdx != -1 )
            {
                suc = UrlHelper.TryParseHostnameAndPort( addressRaw, out t );
            }

            if ( !suc )
            {
                var portRaw = portTextBox.Text?.Trim() ?? string.Empty;
                if ( int.TryParse( portRaw?.Trim(), out var p ) && (0 < p) && (p <= 0xFFFF) )
                {
                    if ( sepIdx != -1 ) addressRaw = addressRaw.Substring( 0, sepIdx );
                    addressRaw += ':' + p.ToString();
                }

                suc = UrlHelper.TryParseHostnameAndPort( addressRaw, out t );
            }

            var urlType = this.GetUrlType();
            var webProxyInfo = new web_proxy_info()
            { 
                Hostname    = t.Hostname,
                Port        = t.Port,
                UrlType     = urlType.GetValueOrDefault(),
                UseWebProxy = suc && HasAnyEditWebProxyGroupBoxChecked(),
                Credentials = this.Credentials,
            };
            return (webProxyInfo);
        }
        public void SetWebProxyInfo( in web_proxy_info t )
        {
            if ( t.UseWebProxy )
            {
                switch ( t.UrlType )
                {
                    case WebProxyUrlEnumType.Http: httpCheckBox.IsChecked = true; break;
                    case WebProxyUrlEnumType.Socks5:
                        var webProxyAddressText_local = t.GetWebProxyAddressText();
                        if ( _Resources_.TOR_BROWSER_SOCKS5_ADDRESS.EqualIgnoreCase( webProxyAddressText_local ) )
                        {
                            torBrowserSocks5CheckBox.IsChecked = true;
                        }
                        else if ( _Resources_.TOR_BROWSER_SOCKS5_ADDRESS.EqualIgnoreCase( webProxyAddressText_local ) )
                        {
                            torServerSocks5CheckBox.IsChecked = true;
                        }
                        else
                        {
                            socks5CheckBox.IsChecked = true;
                        }
                        break;
                }
            }
            else
            {
                UncheckAllEditWebProxyGroupBox();
            }

            (addressTextBox.Text, portTextBox.Text) = (t.Hostname, t.Port.HasValue ? t.Port.ToString() : null);            

            this.Credentials = t.Credentials;
        }
        #endregion

        #region [.private method's.]
        private void Fire_OnWebProxyChanged( bool used )
        {
            var addressRaw = GetUsedWebProxyAddressIfUsed( used );
            OnWebProxyChanged?.Invoke( used && !addressRaw.IsNullOrWhiteSpace(), addressRaw );
        }

        private bool _IgnoreTextChanged;
        private void addressOrPortTextBox_TextChanged( object sender, EventArgs e )
        {
            if ( !_IgnoreTextChanged )
            {
                Fire_OnWebProxyChanged( HasAnyEditWebProxyGroupBoxChecked() );
            }

            var isVisible_testConnectionButton = !(addressTextBox.Text?.IsNullOrWhiteSpace()).GetValueOrDefault();
            if ( testConnectionButton.IsVisible != isVisible_testConnectionButton )
            {
                testConnectionButton.IsVisible = isVisible_testConnectionButton;
                if ( isVisible_testConnectionButton ) BlinkManager.BlinkBackColor( testConnectionButton, _BlinkBackColor );
            }
        }

        private void UncheckAllEditWebProxyGroupBox() => editWebProxyStackPanel.Children.OfType< CheckBox >().ForEach( c => c.IsChecked = false );
        private bool HasAnyEditWebProxyGroupBoxChecked() => editWebProxyStackPanel.Children.OfType< CheckBox >().Any( c => c.IsChecked.GetValueOrDefault() );        
        private (string Username, string Password) Credentials
        {
            get => (userNameTextBox.Text?.Trim(), passwordTextBox.Text?.Trim());
            set => (userNameTextBox.Text, passwordTextBox.Text) = (value.Username?.Trim(), value.Password?.Trim());
        }
        private WebProxyUrlEnumType? GetUrlType()
        {
            if ( torBrowserSocks5CheckBox.IsChecked.GetValueOrDefault() || torServerSocks5CheckBox.IsChecked.GetValueOrDefault() || socks5CheckBox.IsChecked.GetValueOrDefault() )
            {
                return (WebProxyUrlEnumType.Socks5);
            }
            else if ( httpCheckBox.IsChecked.GetValueOrDefault() )
            {
                return (WebProxyUrlEnumType.Http);
            }
            return (null);
        }
        private string GetUsedWebProxyAddressIfUsed( bool usedWebProxy )
        {
            var webProxyAddressText = default(string);
            if ( usedWebProxy )
            {
                var urlType = this.GetUrlType();
                if ( urlType.HasValue && TryParseUrlTexts( out var hostname, out var port ) )
                {
                    webProxyAddressText = web_proxy_info.GetWebProxyAddressText( urlType.Value, hostname, port );
                }
            }
            return (webProxyAddressText);
        }

        private void httpCheckBox_CheckedChanged( object sender, EventArgs e ) => AnyCheckBox_CheckedChanged( sender );
        private void socks5CheckBox_CheckedChanged( object sender, EventArgs e ) => AnyCheckBox_CheckedChanged( sender );
        private void torServerSocks5CheckBox_CheckedChanged( object sender, EventArgs e )
        {
            AnyCheckBox_CheckedChanged( sender );

            if ( torServerSocks5CheckBox.IsChecked.GetValueOrDefault() )
            {
                Set_AddressPort( _Resources_.TOR_SERVER_SOCKS5_ADDRESS );
                Fire_OnWebProxyChanged( true );
            }
        }
        private void torBrowserSocks5CheckBox_CheckedChanged( object sender, EventArgs e )
        {
            AnyCheckBox_CheckedChanged( sender );

            if ( torBrowserSocks5CheckBox.IsChecked.GetValueOrDefault() )
            {
                Set_AddressPort( _Resources_.TOR_BROWSER_SOCKS5_ADDRESS );
                Fire_OnWebProxyChanged( true );
            }
        }

        private bool _IgnoreCheckedChanged;
        private void AnyCheckBox_CheckedChanged( object sender )
        {
            if ( _IgnoreCheckedChanged ) return;

            if ( sender is CheckBox activeCheckBox )
            {
                _IgnoreCheckedChanged = true;
                if ( activeCheckBox.IsChecked.GetValueOrDefault() )
                {
                    activeCheckBox.Foreground = this.Foreground;
                    foreach ( var cb in editWebProxyStackPanel.Children.OfType< CheckBox >() )
                    {
                        if ( cb != activeCheckBox )
                        {
                            cb.IsChecked  = false;
                            cb.Foreground = _UncheckColor;
                        }
                    }
                }
                else
                {
                    activeCheckBox.Foreground = _UncheckColor;
                }

                Fire_OnWebProxyChanged( activeCheckBox.IsChecked.GetValueOrDefault() );
                _IgnoreCheckedChanged = false;
            }
        }

        private void Set_AddressPort( string addressRaw )
        {
            _IgnoreTextChanged = true;
            if ( UrlHelper.TryParseWebProxyUrl( addressRaw, out var t, out _ ) )
            {
                addressTextBox.Text      = t.Hostname;
                portTextBox   .Text      = t.Port?.ToString();
                portTextBox   .IsVisible = t.Port.HasValue;
                BlinkManager.BlinkBackColor( portTextBox, _BlinkBackColor );
            }
            else
            {
                addressTextBox.Text      = addressRaw;
                portTextBox   .Text      = null;
                portTextBox   .IsVisible = false;
            }
            _IgnoreTextChanged = false;
            BlinkManager.BlinkBackColor( addressTextBox, _BlinkBackColor );
        }
        private bool TryParseUrlTexts( out string hostname, out int? port ) => TryParseUrlTexts( addressTextBox.Text?.Trim(), portTextBox.Text?.Trim(), out hostname, out port );
        private static bool TryParseUrlTexts( string addressRaw, string portRaw, out string hostname, out int? port )
        {
            addressRaw = addressRaw?.Trim() ?? string.Empty;

            var suc = UrlHelper.TryParseWebProxyUrl( addressRaw, out var t, out _ );
            if ( suc )
            {
                (hostname, port) = (t.Hostname, t.Port);
                return (true);
            }

            var sepIdx = addressRaw.LastIndexOf(':');
            if ( sepIdx != -1 )
            {
                suc = UrlHelper.TryParseHostnameAndPort( addressRaw, out var x );
                if ( suc )
                {
                    (hostname, port) = (x.Hostname, x.Port);
                    return (true);
                }
            }

            if ( int.TryParse( portRaw?.Trim(), out var p ) && (0 < p) && (p <= 0xFFFF) )
            {
                if ( sepIdx != -1 ) addressRaw = addressRaw.Substring( 0, sepIdx );
                addressRaw += ':' + p.ToString();
            }

            suc = UrlHelper.TryParseHostnameAndPort( addressRaw, out var y );
            (hostname, port) = (y.Hostname, y.Port);
            return (suc);
        }
        #endregion

        #region [.test connection.]
        private async void testConnectionButton_Click( object sender, EventArgs e ) //=> OnTestConnectionButtonClick?.Invoke( this/*sender*/, e );
        {
            const string TEST_URL = "https://google.com";
            const string CAPTION  = "web proxy";

            var webProxyInfo = this.GetWebProxyInfo();
            var webProxyAddressText = default(string);
            var cts = default(CancellationTokenSource);
            var topWindow = Extensions.GetTopWindow();
            try
            {
                using ( cts = new CancellationTokenSource() )
                using ( WaitBannerForm.CreateAndShow( topWindow, cts, captionText: "test link...", null, out var wb ) )
                {
                    wb.ShowCurrentAndTotalSteps = false; wb.ShowPercentSteps = false;
                    (var resp, webProxyAddressText) = await TestConnection_Routine( webProxyInfo, TEST_URL, cts.Token, 
                                webProxyAddressText => wb.SetCaptionText( $"web proxy -> {webProxyAddressText}"/*, showPercentSteps: false*/ ));
                    using ( resp )
                    {
                        resp.EnsureSuccessStatusCode();
                    }
                }

                await /*this.FindForm()*/topWindow.MessageBox_ShowInformation( $"test connection use web proxy -> success. \r\n({webProxyAddressText})", CAPTION );
            }
            catch ( OperationCanceledException ) when (cts?.IsCancellationRequested == true)
            {
                ; //suppress
            }
            catch ( Exception ex )
            {
                var msg = $"test connection use web proxy -> failed " +
                          (webProxyAddressText.IsNullOrWhiteSpace() ? null : $"\r\n({webProxyAddressText})") +                          
                          $" -> \r\n\r\n{ex.GetType().FullName}: \r\n{ex.Message}"; //$" -> \r\n\r\n{ex}";
                await /*this.FindForm()*/topWindow.MessageBox_ShowError( msg, CAPTION );
            }
        }
        private static async Task< (HttpResponseMessage resp, string webProxyAddressText) > TestConnection_Routine( 
            web_proxy_info webProxyInfo, string test_url, CancellationToken ct, Action< string > changeWebProxyAddressAction )
        {
            //var timeout  = TimeSpan.FromSeconds( 10 ); //var (timeout, _) = _SC.GetCreateM3u8ClientParams();
            try
            {
                var webProxyAddressText = webProxyInfo.GetWebProxyAddressText();
                var webProxy            = webProxyInfo.CreateWebProxy( webProxyAddressText );
                var (hc, _, d) = HttpClientFactory_WithRefCount.Get( webProxy/*, timeout*/ );
                using ( d )
                {
                    changeWebProxyAddressAction?.Invoke( webProxyAddressText );
                    //await Task.Delay( 5000 );
                    var resp = await hc.GetAsync( test_url, HttpCompletionOption.ResponseHeadersRead, ct );
                    return (resp, webProxyAddressText);
                }
            }
            catch ( OperationCanceledException ) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch ( Exception /*ex*/ ) when (!webProxyInfo.UseWebProxy && (webProxyInfo.UrlType == default))
            {
                foreach ( var urlType in Enum.GetValues( typeof(WebProxyUrlEnumType) ).Cast< WebProxyUrlEnumType >().Where( x => x != default ) )
                {
                    try
                    {
                        var webProxyAddressText = webProxyInfo.GetWebProxyAddressText( urlType );
                        var webProxy            = webProxyInfo.CreateWebProxy( webProxyAddressText );
                        var (hc, _, d) = HttpClientFactory_WithRefCount.Get( webProxy/*, timeout*/ );
                        using ( d )
                        {
                            changeWebProxyAddressAction?.Invoke( webProxyAddressText );
                            //await Task.Delay( 1000 );
                            var resp = await hc.GetAsync( test_url, HttpCompletionOption.ResponseHeadersRead, ct );
                            return (resp, webProxyAddressText);
                        }
                    }
                    catch ( OperationCanceledException ) when ( ct.IsCancellationRequested )
                    {
                        throw;
                    }
                    catch ( Exception /*inner_ex*/ )
                    {
                        ;
                    }
                }

                throw;
            }
        }
        #endregion
    }
}
