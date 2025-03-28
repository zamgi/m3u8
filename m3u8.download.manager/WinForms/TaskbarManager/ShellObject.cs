﻿using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace System.Windows.Forms.Taskbar
{
    /// <summary>The base class for all Shell objects in Shell Namespace.</summary>
    public abstract class ShellObject : IDisposable, IEquatable<ShellObject>
    {
        /// <summary>Internal member to keep track of the native IShellItem2</summary>
        internal IShellItem2 _NativeShellItem;

        /// <summary>A friendly name for this object that' suitable for display</summary>
        private string _InternalName;

        /// <summary>Parsing name for this Object e.g. c:\Windows\file.txt, or ::{Some Guid}</summary>
        private string _InternalParsingName;

        /// <summary>PID List (PIDL) for this object</summary>
        private IntPtr _InternalPIDL;
        private int? _HashValue;

        private ShellObject _ParentShellObject;
        private ShellProperties _Properties;
        private ShellThumbnail _Thumbnail;

        internal ShellObject() { }
        internal ShellObject( IShellItem2 shellItem ) => _NativeShellItem = shellItem;

        /// <summary>Implement the finalizer.</summary>
        ~ShellObject() => Dispose( false );

        /// <summary>Indicates whether this feature is supported on the current platform.</summary>
        public static bool IsPlatformSupported =>
                // We need Windows Vista onwards ...
                CoreHelpers.RunningOnVista;

        /// <summary>Gets a value that determines if this ShellObject is a file system object.</summary>
        public bool IsFileSystemObject
        {
            get
            {
                try
                {
                    NativeShellItem.GetAttributes( ShellNativeMethods.ShellFileGetAttributesOptions.FileSystem, out var sfgao );
                    return (sfgao & ShellNativeMethods.ShellFileGetAttributesOptions.FileSystem) != 0;
                }
                catch ( FileNotFoundException )
                {
                    return (false);
                }
                catch ( NullReferenceException )
                {
                    // NativeShellItem is null
                    return (false);
                }
            }
        }

        /// <summary>Gets a value that determines if this ShellObject is a link or shortcut.</summary>
        public bool IsLink
        {
            get
            {
                try
                {
                    NativeShellItem.GetAttributes( ShellNativeMethods.ShellFileGetAttributesOptions.Link, out var sfgao );
                    return (sfgao & ShellNativeMethods.ShellFileGetAttributesOptions.Link) != 0;
                }
                catch ( FileNotFoundException )
                {
                    return (false);
                }
                catch ( NullReferenceException )
                {
                    // NativeShellItem is null
                    return (false);
                }
            }
        }

        /// <summary>Gets the normal display for this ShellItem.</summary>
        public virtual string Name
        {
            get
            {
                if ( _InternalName == null && NativeShellItem != null )
                {
                    var pszString = IntPtr.Zero;
                    var hr = NativeShellItem.GetDisplayName( ShellNativeMethods.ShellItemDesignNameOptions.Normal, out pszString );
                    if ( (hr == HResult.Ok) && (pszString != IntPtr.Zero) )
                    {
                        _InternalName = Marshal.PtrToStringAuto( pszString );

                        // Free the string
                        Marshal.FreeCoTaskMem( pszString );
                    }
                }
                return (_InternalName);
            }

            protected set => _InternalName = value;
        }

        /// <summary>Gets the parent ShellObject. Returns null if the object has no parent, i.e. if this object is the Desktop folder.</summary>
        public ShellObject Parent
        {
            get
            {
                if ( _ParentShellObject == null && NativeShellItem2 != null )
                {
                    var hr = NativeShellItem2.GetParent( out var parentShellItem );
                    if ( (hr == HResult.Ok) && (parentShellItem != null) )
                    {
                        _ParentShellObject = ShellObjectFactory.Create( parentShellItem );
                    }
                    else if ( hr == HResult.NoObject )
                    {
                        // Should return null if the parent is desktop
                        return (null);
                    }
                    else
                    {
                        throw (new ShellException( hr ));
                    }
                }

                return (_ParentShellObject);
            }
        }

        /// <summary>Gets the parsing name for this ShellItem.</summary>
        public virtual string ParsingName
        {
            get
            {
                if ( (_InternalParsingName == null) && (_NativeShellItem != null) )
                {
                    _InternalParsingName = ShellHelper.GetParsingName( _NativeShellItem );
                }
                return (_InternalParsingName ?? string.Empty);
            }
            protected set => _InternalParsingName = value;
        }

        /// <summary>Gets an object that allows the manipulation of ShellProperties for this shell item.</summary>
        public ShellProperties Properties
        {
            get
            {
                if ( _Properties == null ) _Properties = new ShellProperties( this );
                return (_Properties);
            }
        }

        /// <summary>Gets the thumbnail of the ShellObject.</summary>
        public ShellThumbnail Thumbnail
        {
            get
            {
                if ( _Thumbnail == null ) _Thumbnail = new ShellThumbnail( this );
                return (_Thumbnail);
            }
        }

        /// <summary>
        /// Gets access to the native IPropertyStore (if one is already created for this item and still valid. This is usually done by the
        /// ShellPropertyWriter class. The reference will be set to null when the writer has been closed/commited).
        /// </summary>
        internal IPropertyStore NativePropertyStore { get; set; }

        /// <summary>Return the native ShellFolder object</summary>
        internal virtual IShellItem NativeShellItem => NativeShellItem2;

        /// <summary>Return the native ShellFolder object as newer IShellItem2</summary>
        /// <exception cref="System.Runtime.InteropServices.ExternalException">
        /// If the native object cannot be created. The ErrorCode member will contain the external error code.
        /// </exception>
        internal virtual IShellItem2 NativeShellItem2
        {
            get
            {
                if ( _NativeShellItem == null && ParsingName != null )
                {
                    var guid = new Guid(ShellIIDGuid.IShellItem2 );
                    var retCode = ShellNativeMethods.SHCreateItemFromParsingName( ParsingName, IntPtr.Zero, ref guid, out _NativeShellItem );

                    if ( (_NativeShellItem == null) || !CoreErrorHelper.Succeeded( retCode ) )
                    {
                        throw (new ShellException( "LocalizedMessages.ShellObjectCreationFailed", Marshal.GetExceptionForHR( retCode ) ));
                    }
                }
                return (_NativeShellItem);
            }
        }

        /// <summary>Gets the PID List (PIDL) for this ShellItem.</summary>
        internal virtual IntPtr PIDL
        {
            get
            {
                // Get teh PIDL for the ShellItem
                if ( (_InternalPIDL == IntPtr.Zero) && (NativeShellItem != null) )
                {
                    _InternalPIDL = ShellHelper.PidlFromShellItem( NativeShellItem );
                }
                return (_InternalPIDL);
            }
            set => _InternalPIDL = value;
        }

        /// <summary>
        /// Creates a ShellObject subclass given a parsing name. For file system items, this method will only accept absolute paths.
        /// </summary>
        /// <param name="parsingName">The parsing name of the object.</param>
        /// <returns>A newly constructed ShellObject object.</returns>
        public static ShellObject FromParsingName( string parsingName ) => ShellObjectFactory.Create( parsingName );

        /// <summary>Implements the != (inequality) operator.</summary>
        /// <param name="leftShellObject">First object to compare.</param>
        /// <param name="rightShellObject">Second object to compare.</param>
        /// <returns>True if leftShellObject does not equal leftShellObject; false otherwise.</returns>
        public static bool operator !=( ShellObject leftShellObject, ShellObject rightShellObject ) => !(leftShellObject == rightShellObject);

        /// <summary>Implements the == (equality) operator.</summary>
        /// <param name="leftShellObject">First object to compare.</param>
        /// <param name="rightShellObject">Second object to compare.</param>
        /// <returns>True if leftShellObject equals rightShellObject; false otherwise.</returns>
        public static bool operator ==( ShellObject leftShellObject, ShellObject rightShellObject )
        {
            if ( (object) leftShellObject == null )
            {
                return ((object) rightShellObject == null);
            }
            return (leftShellObject.Equals( rightShellObject ));
        }

        /// <summary>Release the native objects.</summary>
        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        /// <summary>Determines if two ShellObjects are identical.</summary>
        /// <param name="other">The ShellObject to comare this one to.</param>
        /// <returns>True if the ShellObjects are equal, false otherwise.</returns>
        public bool Equals( ShellObject other )
        {
            var areEqual = false;

            if ( other != null )
            {
                var ifirst = NativeShellItem;
                var isecond = other.NativeShellItem;
                if ( ifirst != null && isecond != null )
                {
                    var hr = ifirst.Compare( isecond, SICHINTF.SICHINT_ALLFIELDS, out var result );
                    areEqual = (hr == HResult.Ok) && (result == 0);
                }
            }

            return (areEqual);
        }

        /// <summary>Returns whether this object is equal to another.</summary>
        /// <param name="obj">The object to compare against.</param>
        /// <returns>Equality result.</returns>
        public override bool Equals( object obj ) => Equals( obj as ShellObject );

        /// <summary>
        /// Returns the display name of the ShellFolder object. DisplayNameType represents one of the values that indicates how the name
        /// should look. See <see cref="Microsoft.WindowsAPICodePack.Shell.DisplayNameType"/> for a list of possible values.
        /// </summary>
        /// <param name="displayNameType">A disaply name type.</param>
        /// <returns>A string.</returns>
        public virtual string GetDisplayName( DisplayNameType displayNameType )
        {
            string returnValue = null;
            NativeShellItem2?.GetDisplayName( (ShellNativeMethods.ShellItemDesignNameOptions) displayNameType, out returnValue );
            return (returnValue);
        }

        /// <summary>Returns the hash code of the object.</summary>
        public override int GetHashCode()
        {
            if ( !_HashValue.HasValue )
            {
                var size = ShellNativeMethods.ILGetSize( PIDL );
                if ( size != 0 )
                {
                    var pidlData = new byte[ size ];
                    Marshal.Copy( PIDL, pidlData, 0, (int) size );

                    // Using FNV-1a hash algorithm because a cryptographically secure algorithm is not required for this use
                    const int p = 16777619;
                    int hash = -2128831035;
                    for ( int i = 0; i < pidlData.Length; i++ )
                    {
                        hash = (hash ^ pidlData[ i ]) * p;
                    }
                    _HashValue = hash;
                }
                else
                {
                    _HashValue = 0;
                }
            }
            return (_HashValue.Value);
        }

        /// <summary>Overrides object.ToString()</summary>
        /// <returns>A string representation of the object.</returns>
        public override string ToString() => Name;

        /// <summary>
        /// Updates the native shell item that maps to this shell object. This is necessary when the shell item changes after the shell
        /// object has been created. Without this method call, the retrieval of properties will return stale data.
        /// </summary>
        /// <param name="bindContext">Bind context object</param>
        public void Update( IBindCtx bindContext )
        {
            var hr = HResult.Ok;

            if ( NativeShellItem2 != null )
            {
                hr = NativeShellItem2.Update( bindContext );
            }

            if ( CoreErrorHelper.Failed( hr ) )
            {
                throw (new ShellException( hr ));
            }
        }

        /// <summary>Release the native and managed objects</summary>
        /// <param name="disposing">Indicates that this is being called from Dispose(), rather than the finalizer.</param>
        protected virtual void Dispose( bool disposing )
        {
            if ( disposing )
            {
                _InternalName = null;
                _InternalParsingName = null;
                _Properties = null;
                _Thumbnail = null;
                _ParentShellObject = null;
            }

            if ( _Properties != null )
            {
                _Properties.Dispose();
            }

            if ( _InternalPIDL != IntPtr.Zero )
            {
                ShellNativeMethods.ILFree( _InternalPIDL );
                _InternalPIDL = IntPtr.Zero;
            }

            if ( _NativeShellItem != null )
            {
                Marshal.ReleaseComObject( _NativeShellItem );
                _NativeShellItem = null;
            }

            if ( NativePropertyStore != null )
            {
                Marshal.ReleaseComObject( NativePropertyStore );
                NativePropertyStore = null;
            }
        }
    }
}