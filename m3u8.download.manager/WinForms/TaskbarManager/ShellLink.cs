namespace System.Windows.Forms.Taskbar
{
    /// <summary>Represents a link to existing FileSystem or Virtual item.</summary>
    public class ShellLink : ShellObject
    {
        /// <summary>Path for this file e.g. c:\Windows\file.txt,</summary>
        private string _InternalPath;
        private string _InternalArguments;
        private string _InternalComments;
        private string _InternalTargetLocation;

        internal ShellLink( IShellItem2 shellItem ) => _NativeShellItem = shellItem;

        /// <summary>Gets the arguments associated with this link.</summary>
        public string Arguments
        {
            get
            {
                if ( string.IsNullOrEmpty( _InternalArguments ) && NativeShellItem2 != null )
                {
                    _InternalArguments = Properties.System.Link.Arguments.Value;
                }
                return (_InternalArguments);
            }
        }

        /// <summary>Gets the comments associated with this link.</summary>
        public string Comments
        {
            get
            {
                if ( string.IsNullOrEmpty( _InternalComments ) && NativeShellItem2 != null )
                {
                    _InternalComments = Properties.System.Comment.Value;
                }
                return (_InternalComments);
            }
        }

        /// <summary>The path for this link</summary>
        public virtual string Path
        {
            get
            {
                if ( _InternalPath == null && NativeShellItem != null )
                {
                    _InternalPath = base.ParsingName;
                }
                return (_InternalPath);
            }
            protected set => _InternalPath = value;
        }

        /// <summary>Gets the location to which this link points to.</summary>
        public string TargetLocation
        {
            get
            {
                if ( string.IsNullOrEmpty( _InternalTargetLocation ) && NativeShellItem2 != null )
                {
                    _InternalTargetLocation = Properties.System.Link.TargetParsingPath.Value;
                }
                return (_InternalTargetLocation);
            }
            set
            {
                if ( value == null ) return;

                _InternalTargetLocation = value;

                if ( NativeShellItem2 != null )
                {
                    Properties.System.Link.TargetParsingPath.Value = _InternalTargetLocation;
                }
            }
        }

        /// <summary>Gets the ShellObject to which this link points to.</summary>
        public ShellObject TargetShellObject => ShellObjectFactory.Create( TargetLocation );

        /// <summary>Gets or sets the link's title</summary>
        public string Title
        {
            get
            {
                if ( NativeShellItem2 != null ) return (Properties.System.Title.Value);
                return (null);
            }
            set
            {
                if ( value == null )
                {
                    throw (new ArgumentNullException( "value" ));
                }

                if ( NativeShellItem2 != null )
                {
                    Properties.System.Title.Value = value;
                }
            }
        }
    }
}