namespace System.Windows.Forms
{
    /// <summary>
    /// 
    /// </summary>
    internal class ButtonWithFocusCues : Button
    {
        protected override bool ShowFocusCues
        {
            get { return (base.Focused); } //get { return (base.ShowFocusCues); }
        }
    }
}
