namespace System.Windows.Forms
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class ButtonWithFocusCues : Button
    {
        protected override bool ShowFocusCues => base.Focused;
    }
}
