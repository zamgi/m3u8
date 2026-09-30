using System.Windows.Forms;

using m3u8.download.manager.Properties;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class OutputDirectorySuggestionsComboBox : ComboBoxEx
    {
        private OutputDirectorySuggestionsComboBox( Settings settings ) : base( settings.OutputDirectorySuggestions,
                                                                                settings.OutputDirectorySuggestionsMaxCount,
                                                                                itemsValues => settings.OutputDirectorySuggestions = itemsValues ) { }
        public OutputDirectorySuggestionsComboBox() : this( Settings.Default ) { }
    }
}
