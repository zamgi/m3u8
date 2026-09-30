using System.Windows.Forms;

using m3u8.download.manager.Properties;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class OutputFilenameSuggestionsComboBox : ComboBoxEx
    {
        private OutputFilenameSuggestionsComboBox( Settings settings ) : base( settings.OutputFilenameSuggestions,
                                                                               settings.OutputFilenameSuggestionsMaxCount,
                                                                               itemsValues => settings.OutputFilenameSuggestions = itemsValues ) { }
        public OutputFilenameSuggestionsComboBox() : this( Settings.Default ) { }
        //=> base.OnClearMenuItemClick += OutputFilenameSuggestionsComboBox_OnClearMenuItemClick;

        //private void OutputFilenameSuggestionsComboBox_OnClearMenuItemClick( object sender, EventArgs e ) { }
    }
}
