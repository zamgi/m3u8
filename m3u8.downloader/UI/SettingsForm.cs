using System;
using System.Windows.Forms;

namespace m3u8.downloader
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed partial class SettingsForm : Form
    {
        private SettingsForm() => InitializeComponent();
        public SettingsForm( int attemptRequestCountByPart, TimeSpan requestTimeoutByPart ) : this()
        {
            AttemptRequestCountByPart = attemptRequestCountByPart;
            RequestTimeoutByPart      = requestTimeoutByPart;
        }

        public int AttemptRequestCountByPart
        {
            get => Convert.ToInt32( attemptRequestCountByPartNUD.Value );
            set => attemptRequestCountByPartNUD.Value = value;
        }
        public TimeSpan RequestTimeoutByPart
        {
            get => requestTimeoutByPartDTP.Value.TimeOfDay; // TimeSpan.FromTicks( (requestTimeoutByPartDTP.Value.TimeOfDay - requestTimeoutByPartDTP.MinDate.Date).Ticks );
            set => requestTimeoutByPartDTP.Value = requestTimeoutByPartDTP.MinDate.Date + value;
        }
    }
}
