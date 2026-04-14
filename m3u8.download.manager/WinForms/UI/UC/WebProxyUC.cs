using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using m3u8.download.manager.Properties;

using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

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
        public delegate void WebProxyChangedEventHandler( bool enabled, string addressRaw );

        public event WebProxyChangedEventHandler OnWebProxyChanged;
        private void Fire_OnWebProxyChanged( bool enabled, string addressRaw ) => OnWebProxyChanged?.Invoke( enabled, addressRaw );

        private Settings _Settings;
        private Color _UncheckColor = Color.Gray;
        private Color _BlinkBackColor = Color.Khaki;
        public WebProxyUC( Settings settings )
        {
            _Settings = settings;
            InitializeComponent();

            torBrowserSocks5CheckBox.CheckedChanged += torBrowserSocks5CheckBox_CheckedChanged;
            torServerSocks5CheckBox.CheckedChanged += torServerSocks5CheckBox_CheckedChanged;
            socks5CheckBox.CheckedChanged += socks5CheckBox_CheckedChanged;
            httpCheckBox.CheckedChanged += httpCheckBox_CheckedChanged;
            editWebProxyGroupBox.Controls.OfType< CheckBox >().Where( c => !c.Checked ).ForEach( c => c.ForeColor = _UncheckColor );
            this.Controls.OfType< GroupBox >().ForEach( c => c.ForeColor = Color.DodgerBlue );
        }

        private bool HasAnyEditWebProxyGroupBoxChecked() => editWebProxyGroupBox.Controls.OfType< CheckBox >().Any( c => c.Checked );
        public UrlHelper.WebProxyUrlEnumType? Scheme
        {
            get
            {
                if ( torBrowserSocks5CheckBox.Checked || torServerSocks5CheckBox.Checked || socks5CheckBox.Checked )
                {
                    return (UrlHelper.WebProxyUrlEnumType.Socks5);
                }
                else if ( httpCheckBox.Checked )
                {
                    return (UrlHelper.WebProxyUrlEnumType.Http);
                }
                return (null);
            }
        }
        public bool UsedWebProxyAddress
        {
            get
            {
                var suc = HasAnyEditWebProxyGroupBoxChecked();
                return (suc && HasValidAddresPort());
            }
        }
        public string WebProxyAddress
        {
            get
            {
                var scheme = this.Scheme;
                var webProxyAddress = default(string);
                if ( scheme.HasValue && TryParseUrlTexts( out var address, out var port ) )
                {
                    webProxyAddress = UrlHelper.GetWebProxyAddressText( scheme.Value, address, port );
                }
                return (webProxyAddress);
            }
            set
            {
                if ( UrlHelper.TryParseWebProxyUrl( value?.Trim(), out var t ) )
                {
                    switch ( t.UrlType )
                    {
                        case UrlHelper.WebProxyUrlEnumType.Http: httpCheckBox.Checked = true; break;
                        case UrlHelper.WebProxyUrlEnumType.Socks5:
                            var webProxyAddressText = t.GetWebProxyAddressText();
                            if ( Resources.TOR_BROWSER_SOCKS5_ADDRESS.EqualIgnoreCase( webProxyAddressText ) )
                            {
                                torBrowserSocks5CheckBox.Checked = true;
                            }
                            else if ( Resources.TOR_BROWSER_SOCKS5_ADDRESS.EqualIgnoreCase( webProxyAddressText ) )
                            {
                                torServerSocks5CheckBox.Checked = true;
                            }
                            else
                            {
                                socks5CheckBox.Checked = true;
                            }
                            break;
                    }

                    (addressTextBox.Text, portTextBox.Text) = (t.Address, t.Port.HasValue ? t.Port.ToString() : null);
                }
                else
                {
                    editWebProxyGroupBox.Controls.OfType< CheckBox >().ForEach( c => c.Checked = false );
                }
            }
        }
        public (string Username, string Password) Credentials
        {
            get => (userNameTextBox.Text.Trim(), passwordTextBox.Text.Trim());
            set => (userNameTextBox.Text, passwordTextBox.Text) = (value.Username?.Trim(), value.Password?.Trim());
        }

        private void httpCheckBox_CheckedChanged( object sender, EventArgs e )
        {            
            AnyCheckBox_CheckedChanged( sender );
        }
        private void socks5CheckBox_CheckedChanged( object sender, EventArgs e )
        {
            AnyCheckBox_CheckedChanged( sender );
        }
        private void torServerSocks5CheckBox_CheckedChanged( object sender, EventArgs e )
        {
            AnyCheckBox_CheckedChanged( sender );

            if ( torServerSocks5CheckBox.Checked )
                Set_AddressPort( Resources.TOR_SERVER_SOCKS5_ADDRESS );
        }
        private void torBrowserSocks5CheckBox_CheckedChanged( object sender, EventArgs e )
        {
            AnyCheckBox_CheckedChanged( sender );

            if ( torBrowserSocks5CheckBox.Checked )
                Set_AddressPort( Resources.TOR_BROWSER_SOCKS5_ADDRESS );
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

                Fire_OnWebProxyChanged( activeCheckBox.Checked/*this.UsedWebProxyAddress*/, this.WebProxyAddress );
                _IgnoreCheckedChanged = false;
            }
        }

        private void Set_AddressPort( string addressRaw )
        {
            if ( UrlHelper.TryParseWebProxyUrl( addressRaw, out var t ) )
            {
                addressTextBox.Text = t.Address;
                portTextBox.Text    = t.Port?.ToString();
                portTextBox.Visible = t.Port.HasValue;
                //portNumUpDown.Maximum = Math.Min( portNumUpDown.Minimum, port );
                //portNumUpDown.Maximum = Math.Max( portNumUpDown.Maximum, port );
                //portNumUpDown.Value = port;
                //portNumUpDown.Visible = true;
                BlinkManager.BlinkBackColor( portTextBox, _BlinkBackColor );
            }
            else
            {
                addressTextBox.Text = addressRaw;
                //portNumUpDown.Visible = false;
                portTextBox.Visible = false;
            }

            BlinkManager.BlinkBackColor( addressTextBox, _BlinkBackColor );
            
        }

        private bool HasValidAddresPort() => TryParseUrlTexts( out _, out _ );
        private bool TryParseUrlTexts( out string address, out int? port ) => TryParseUrlTexts( addressTextBox.Text.Trim(), portTextBox.Text.Trim(), out address, out port );
        private static bool TryParseUrlTexts( string addressRaw, string portRaw, out string address, out int? port )
        {
            addressRaw = addressRaw?.Trim() ?? string.Empty;

            var suc = UrlHelper.TryParseWebProxyUrl( addressRaw, out var res );
            if ( suc )
            {
                (address, port) = (res.Address, res.Port);
                return (true);
            }

            suc = UrlHelper.TryParseHostnameAndPort( addressRaw, out var t );
            if ( suc )
            {
                (address, port) = (t.Address, t.Port);
                return (true);
            }

            if ( int.TryParse( portRaw?.Trim(), out var p ) && (0 < p) && (p <= 0xFFFF) )
            {
                var i = addressRaw.LastIndexOf(':');
                if ( i != -1 ) addressRaw = addressRaw.Substring( 0, i );
                addressRaw += ':' + p.ToString();
            }

            suc = UrlHelper.TryParseHostnameAndPort( addressRaw, out t );
            (address, port) = (t.Address, t.Port);
            return (suc);
        }
    }
}
