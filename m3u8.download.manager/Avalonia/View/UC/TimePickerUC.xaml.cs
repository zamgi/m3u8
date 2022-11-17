using System;

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class TimePickerUC : UserControl
    {
        #region [.field's.]
        private NumericUpDown hoursNUD;
        private NumericUpDown minutesNUD;
        private NumericUpDown secondsNUD;
        #endregion

        #region [.ctor().]
        public TimePickerUC()
        {
            this.InitializeComponent();

            hoursNUD   = this.FindControl< NumericUpDown >( nameof(hoursNUD)   ); hoursNUD  .ValueChanged += hoursNUD_ValueChanged;
            minutesNUD = this.FindControl< NumericUpDown >( nameof(minutesNUD) ); minutesNUD.ValueChanged += minutesNUD_ValueChanged;
            secondsNUD = this.FindControl< NumericUpDown >( nameof(secondsNUD) ); secondsNUD.ValueChanged += secondsNUD_ValueChanged;
        }
        private void InitializeComponent() => AvaloniaXamlLoader.Load( this );
        #endregion

        #region [.private.]
        private void hoursNUD_ValueChanged( object sender, NumericUpDownValueChangedEventArgs e )
        {
            if ( 24 <= e.NewValue )
            {
                e.Handled = true;
                hoursNUD.Value = 0;
            }
        }
        private void minutesNUD_ValueChanged( object sender, NumericUpDownValueChangedEventArgs e )
        {
            if ( 60 <= e.NewValue )
            {
                e.Handled = true;
                //hoursNUD.Value++;
                minutesNUD.Value = 0;
            }
        }
        private void secondsNUD_ValueChanged( object sender, NumericUpDownValueChangedEventArgs e )
        {
            if ( 60 <= e.NewValue )
            {
                e.Handled = true;
                //minutesNUD.Value++;                
                secondsNUD.Value = 0;
            }
        }
        #endregion

        #region [.public.]
        public TimeSpan Value
        {
            get => new TimeSpan( Math.Min( Math.Max( (int) hoursNUD  .Value, 0 ), 23 ),
                                 Math.Min( Math.Max( (int) minutesNUD.Value, 0 ), 59 ),
                                 Math.Min( Math.Max( (int) secondsNUD.Value, 0 ), 59 )
                               );
            set
            {
                hoursNUD  .Value = value.Hours;
                minutesNUD.Value = value.Minutes;
                secondsNUD.Value = value.Seconds;
            }
        }
        #endregion
    }
}
