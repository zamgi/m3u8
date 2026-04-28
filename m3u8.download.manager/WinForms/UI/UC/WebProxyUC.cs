using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

using m3u8.download.manager.Properties;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed partial class WebProxyUC : UserControl
    {
        /// <summary>
        /// 
        /// </summary>
        public delegate void WebProxyChangedEventHandler( bool used, string addressRaw );

        public event WebProxyChangedEventHandler OnWebProxyChanged;
        
        #region [.ctor().]
        private Color _UncheckColor   = Color.Gray;
        private Color _BlinkBackColor = Color.Khaki;
        public WebProxyUC()
        {
            InitializeComponent();

            torBrowserSocks5CheckBox.CheckedChanged += torBrowserSocks5CheckBox_CheckedChanged;
            torServerSocks5CheckBox .CheckedChanged += torServerSocks5CheckBox_CheckedChanged;
            socks5CheckBox          .CheckedChanged += socks5CheckBox_CheckedChanged;
            httpCheckBox            .CheckedChanged += httpCheckBox_CheckedChanged;
            addressTextBox          .TextChanged    += addressOrPortTextBox_TextChanged;
            portTextBox             .TextChanged    += addressOrPortTextBox_TextChanged;
            editWebProxyGroupBox.Controls.OfType< CheckBox >().Where( c => !c.Checked ).ForEach( c => c.ForeColor = _UncheckColor );
            this.Controls.OfType< GroupBox >().ForEach( c => c.ForeColor = Color.DodgerBlue );
        }
        #endregion

        #region [.public.]
        public web_proxy_info GetWebProxyInfo()
        {
            var addressRaw = addressTextBox.Text.Trim();

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
                var portRaw = portTextBox.Text.Trim();
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
                    case WebProxyUrlEnumType.Http: httpCheckBox.Checked = true; break;
                    case WebProxyUrlEnumType.Socks5:
                        var webProxyAddressText_local = t.GetWebProxyAddressText();
                        if ( Resources.TOR_BROWSER_SOCKS5_ADDRESS.EqualIgnoreCase( webProxyAddressText_local ) )
                        {
                            torBrowserSocks5CheckBox.Checked = true;
                        }
                        else if ( Resources.TOR_BROWSER_SOCKS5_ADDRESS.EqualIgnoreCase( webProxyAddressText_local ) )
                        {
                            torServerSocks5CheckBox.Checked = true;
                        }
                        else
                        {
                            socks5CheckBox.Checked = true;
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
        
        public void Activate()
        {
            if ( this.ActiveControl == null )
            {
                this.BeginInvoke( new Action( () => addressTextBox.Focus() ) );
            }
        }
        #endregion

        #region [.private methods.]
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

            var isVisible_testConnectionButton = !addressTextBox.Text.IsNullOrWhiteSpace();
            if ( testConnectionButton.Visible != isVisible_testConnectionButton )
            {
                testConnectionButton.Visible = isVisible_testConnectionButton;
                if ( isVisible_testConnectionButton ) BlinkManager.BlinkBackColor( testConnectionButton, _BlinkBackColor );
            }
        }

        private void UncheckAllEditWebProxyGroupBox() => editWebProxyGroupBox.Controls.OfType< CheckBox >().ForEach( c => c.Checked = false );
        private bool HasAnyEditWebProxyGroupBoxChecked() => editWebProxyGroupBox.Controls.OfType< CheckBox >().Any( c => c.Checked );        
        private (string Username, string Password) Credentials
        {
            get => (userNameTextBox.Text.Trim(), passwordTextBox.Text.Trim());
            set => (userNameTextBox.Text, passwordTextBox.Text) = (value.Username?.Trim(), value.Password?.Trim());
        }
        private WebProxyUrlEnumType? GetUrlType()
        {
            if ( torBrowserSocks5CheckBox.Checked || torServerSocks5CheckBox.Checked || socks5CheckBox.Checked )
            {
                return (WebProxyUrlEnumType.Socks5);
            }
            else if ( httpCheckBox.Checked )
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

            if ( torServerSocks5CheckBox.Checked )
            {
                Set_AddressPort( Resources.TOR_SERVER_SOCKS5_ADDRESS );
                Fire_OnWebProxyChanged( true );
            }
        }
        private void torBrowserSocks5CheckBox_CheckedChanged( object sender, EventArgs e )
        {
            AnyCheckBox_CheckedChanged( sender );

            if ( torBrowserSocks5CheckBox.Checked )
            {
                Set_AddressPort( Resources.TOR_BROWSER_SOCKS5_ADDRESS );
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
                if ( activeCheckBox.Checked )
                {
                    activeCheckBox.ForeColor = this.ForeColor;
                    foreach ( var cb in editWebProxyGroupBox.Controls.OfType< CheckBox >() )
                    {
                        if ( cb != activeCheckBox )
                        {
                            cb.Checked = false;
                            cb.ForeColor = _UncheckColor;
                        }
                    }
                }
                else
                {
                    activeCheckBox.ForeColor = _UncheckColor;
                }

                Fire_OnWebProxyChanged( activeCheckBox.Checked );
                _IgnoreCheckedChanged = false;
            }
        }

        private void Set_AddressPort( string addressRaw )
        {
            _IgnoreTextChanged = true;
            if ( UrlHelper.TryParseWebProxyUrl( addressRaw, out var t, out _ ) )
            {
                addressTextBox.Text    = t.Hostname;
                portTextBox   .Text    = t.Port?.ToString();
                portTextBox   .Visible = t.Port.HasValue;
                BlinkManager.BlinkBackColor( portTextBox, _BlinkBackColor );
            }
            else
            {
                addressTextBox.Text    = addressRaw;
                portTextBox   .Text    = null;
                portTextBox   .Visible = false;
            }
            _IgnoreTextChanged = false;
            BlinkManager.BlinkBackColor( addressTextBox, _BlinkBackColor );
        }
        private bool TryParseUrlTexts( out string hostname, out int? port ) => TryParseUrlTexts( addressTextBox.Text.Trim(), portTextBox.Text.Trim(), out hostname, out port );
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
        private async void testConnectionButton_Click( object sender, EventArgs e )
        {
            const string CAPTION = "web proxy";

            var webProxyInfo = this.GetWebProxyInfo();
            var webProxyAddressText = default(string);
            var cts = default(CancellationTokenSource);
            try
            {
                using ( cts = new CancellationTokenSource() )
                using ( var wb = WaitBannerUC.Create( this, cts, captionText: "test link..." ) )
                {
                    wb.ShowCurrentAndTotalSteps = false;
                    (var resp, webProxyAddressText) = await TestWebProxyConnectionHelper.TestConnection_Routine( webProxyInfo, cts.Token, 
                                webProxyAddressText => wb.BeginInvoke(() => wb.SetCaptionText( $"web proxy -> {webProxyAddressText}", showPercentSteps: false ) ) );
                    using ( resp )
                    {
                        resp.EnsureSuccessStatusCode();
                    }
                }

                this.FindForm().MessageBox_ShowInformation( $"test connection use web proxy -> success. \r\n({webProxyAddressText})", CAPTION );
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
                this.FindForm().MessageBox_ShowError( msg, CAPTION );
            }
        }
        #endregion
    }
}
