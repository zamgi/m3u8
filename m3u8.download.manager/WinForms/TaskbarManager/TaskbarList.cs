namespace System.Windows.Forms.Taskbar
{
    /// <summary>
    /// Provides internal access to the functions provided by the ITaskbarList4 interface,
    /// without being forced to refer to it through another singleton.
    /// </summary>
    internal static class TaskbarList
    {
        private static readonly object _SyncLock = new object();

        private static ITaskbarList4 _TaskbarList;
        internal static ITaskbarList4 Instance
        {
            get
            {
                if ( _TaskbarList == null )
                {
                    lock ( _SyncLock )
                    {
                        if ( _TaskbarList == null )
                        {
                            _TaskbarList = (ITaskbarList4) new CTaskbarList();
                            _TaskbarList.HrInit();
                        }
                    }
                }
                return (_TaskbarList);
            }
        }
    }
}
