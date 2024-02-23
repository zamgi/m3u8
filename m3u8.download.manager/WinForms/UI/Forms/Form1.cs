using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;

namespace m3u8.download.manager.ui
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void getRequestHeadersButton_Click( object sender, EventArgs e )
        {
            var dict = requestHeadersEditor1.GetRequestHeaders();

            var buf = new StringBuilder();
            foreach ( var p in dict )
            {
                if ( buf.Length != 0 ) buf.AppendLine();
                buf.Append( p.Key ).Append( " = " ).Append( p.Value );
            }
            MessageBox.Show( buf.ToString() );
        }

        private void setRequestHeadersButton_Click( object sender, EventArgs e )
        {
            var requestHeaders = new Dictionary< string, string >
            {
                { "Accept", "*/*" },
                { "Accept-Encoding", "gzip, deflate, br" },
                { "Accept-Language", "ru,en-US;q=0.9,en;q=0.8" },

                { "Cache-Control", "no-cache" },
                { "Pragma", "no-cache" },
                { "Connection", "keep-alive" },
                { "Host", "09b-8c6-300g0.v.plground.live:10403" },
                { "Origin" , "https://ollo-as.newplayjj.com:9443"  },
                { "Referer", "https://ollo-as.newplayjj.com:9443/" },
                { "Sec-Fetch-Dest", "empty" },
                { "Sec-Fetch-Mode", "cors" },
                { "Sec-Fetch-Site", "cross-site" },
                { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/121.0.0.0 Safari/537.36" },

                { "sec-ch-ua", "\"Not A(Brand\";v=\"99\", \"Google Chrome\";v=\"121\", \"Chromium\";v=\"121\"" },
                { "sec-ch-ua-mobile", "?0" },
                { "sec-ch-ua-platform", "\"Windows\"" }
            };
            requestHeadersEditor1.SetRequestHeaders( requestHeaders );
        }
    }
}
