using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace System.Windows.Forms.Taskbar
{
    /// <summary>Internal class to represent the KnownFolder settings/properties</summary>
    internal class KnownFolderSettings
    {
        private FolderProperties _KnownFolderProperties;
        internal KnownFolderSettings( IKnownFolderNative knownFolderNative ) => GetFolderProperties( knownFolderNative );

        /// <summary>Gets this known folder's canonical name.</summary>
        /// <value>A <see cref="System.String"/> object.</value>
        public string CanonicalName => _KnownFolderProperties.canonicalName;

        /// <summary>Gets the category designation for this known folder.</summary>
        /// <value>A <see cref="FolderCategory"/> value.</value>
        public FolderCategory Category => _KnownFolderProperties.category;

        /// <summary>Gets an value that describes this known folder's behaviors.</summary>
        /// <value>A <see cref="DefinitionOptions"/> value.</value>
        public DefinitionOptions DefinitionOptions => _KnownFolderProperties.definitionOptions;

        /// <summary>Gets this known folder's description.</summary>
        /// <value>A <see cref="System.String"/> object.</value>
        public string Description => _KnownFolderProperties.description;

        /// <summary>Gets the unique identifier for this known folder.</summary>
        /// <value>A <see cref="System.Guid"/> value.</value>
        public Guid FolderId => _KnownFolderProperties.folderId;

        /// <summary>Gets a string representation of this known folder's type.</summary>
        /// <value>A <see cref="System.String"/> object.</value>
        public string FolderType => _KnownFolderProperties.folderType;

        /// <summary>Gets the unique identifier for this known folder's type.</summary>
        /// <value>A <see cref="System.Guid"/> value.</value>
        public Guid FolderTypeId => _KnownFolderProperties.folderTypeId;

        /// <summary>Gets this known folder's localized name.</summary>
        /// <value>A <see cref="System.String"/> object.</value>
        public string LocalizedName => _KnownFolderProperties.localizedName;

        /// <summary>Gets the resource identifier for this known folder's localized name.</summary>
        /// <value>A <see cref="System.String"/> object.</value>
        public string LocalizedNameResourceId => _KnownFolderProperties.localizedNameResourceId;

        /// <summary>Gets the unique identifier for this known folder's parent folder.</summary>
        /// <value>A <see cref="System.Guid"/> value.</value>
        public Guid ParentId => _KnownFolderProperties.parentId;

        /// <summary>Gets the path for this known folder.</summary>
        /// <value>A <see cref="System.String"/> object.</value>
        public string Path => _KnownFolderProperties.path;

        /// <summary>Gets a value that indicates whether this known folder's path exists on the computer.</summary>
        /// <value>A bool <see cref="System.Boolean"/> value.</value>
        /// <remarks>
        /// If this property value is <b>false</b>, the folder might be a virtual folder ( <see cref="Category"/> property will be
        /// <see cref="FolderCategory.Virtual"/> for virtual folders)
        /// </remarks>
        public bool PathExists => _KnownFolderProperties.pathExists;

        /// <summary>
        /// Gets a value that states whether this known folder can have its path set to a new value, including any restrictions on the redirection.
        /// </summary>
        /// <value>A <see cref="RedirectionCapability"/> value.</value>
        public RedirectionCapability Redirection => _KnownFolderProperties.redirection;

        /// <summary>Gets this known folder's relative path.</summary>
        /// <value>A <see cref="System.String"/> object.</value>
        public string RelativePath => _KnownFolderProperties.relativePath;

        /// <summary>Gets this known folder's security attributes.</summary>
        /// <value>A <see cref="System.String"/> object.</value>
        public string Security => _KnownFolderProperties.security;

        /// <summary>Gets this known folder's tool tip text.</summary>
        /// <value>A <see cref="System.String"/> object.</value>
        public string Tooltip => _KnownFolderProperties.tooltip;

        /// <summary>Gets the resource identifier for this known folder's tool tip text.</summary>
        /// <value>A <see cref="System.String"/> object.</value>
        public string TooltipResourceId => _KnownFolderProperties.tooltipResourceId;

        /// <summary>Gets this known folder's file attributes, such as "read-only".</summary>
        /// <value>A <see cref="FileAttributes"/> value.</value>
        public FileAttributes FileAttributes => _KnownFolderProperties.fileAttributes;

        /// <summary>Populates a structure that contains this known folder's properties.</summary>
        private void GetFolderProperties( IKnownFolderNative knownFolderNative )
        {
            Debug.Assert( knownFolderNative != null );

            knownFolderNative.GetFolderDefinition( out var nativeFolderDefinition );

            try
            {
                _KnownFolderProperties.category = nativeFolderDefinition.category;
                _KnownFolderProperties.canonicalName = Marshal.PtrToStringUni( nativeFolderDefinition.name );
                _KnownFolderProperties.description = Marshal.PtrToStringUni( nativeFolderDefinition.description );
                _KnownFolderProperties.parentId = nativeFolderDefinition.parentId;
                _KnownFolderProperties.relativePath = Marshal.PtrToStringUni( nativeFolderDefinition.relativePath );
                _KnownFolderProperties.parsingName = Marshal.PtrToStringUni( nativeFolderDefinition.parsingName );
                _KnownFolderProperties.tooltipResourceId = Marshal.PtrToStringUni( nativeFolderDefinition.tooltip );
                _KnownFolderProperties.localizedNameResourceId = Marshal.PtrToStringUni( nativeFolderDefinition.localizedName );
                _KnownFolderProperties.iconResourceId = Marshal.PtrToStringUni( nativeFolderDefinition.icon );
                _KnownFolderProperties.security = Marshal.PtrToStringUni( nativeFolderDefinition.security );
                _KnownFolderProperties.fileAttributes = (FileAttributes) nativeFolderDefinition.attributes;
                _KnownFolderProperties.definitionOptions = nativeFolderDefinition.definitionOptions;
                _KnownFolderProperties.folderTypeId = nativeFolderDefinition.folderTypeId;
                _KnownFolderProperties.folderType = FolderTypes.GetFolderType( _KnownFolderProperties.folderTypeId );

                _KnownFolderProperties.path = GetPath( out var pathExists, knownFolderNative );
                _KnownFolderProperties.pathExists = pathExists;

                _KnownFolderProperties.redirection = knownFolderNative.GetRedirectionCapabilities();

                // Turn tooltip, localized name and icon resource IDs into the actual resources.
                _KnownFolderProperties.tooltip = CoreHelpers.GetStringResource( _KnownFolderProperties.tooltipResourceId );
                _KnownFolderProperties.localizedName = CoreHelpers.GetStringResource( _KnownFolderProperties.localizedNameResourceId );

                _KnownFolderProperties.folderId = knownFolderNative.GetId();
            }
            finally
            {
                // Clean up memory.
                Marshal.FreeCoTaskMem( nativeFolderDefinition.name );
                Marshal.FreeCoTaskMem( nativeFolderDefinition.description );
                Marshal.FreeCoTaskMem( nativeFolderDefinition.relativePath );
                Marshal.FreeCoTaskMem( nativeFolderDefinition.parsingName );
                Marshal.FreeCoTaskMem( nativeFolderDefinition.tooltip );
                Marshal.FreeCoTaskMem( nativeFolderDefinition.localizedName );
                Marshal.FreeCoTaskMem( nativeFolderDefinition.icon );
                Marshal.FreeCoTaskMem( nativeFolderDefinition.security );
            }
        }

        /// <summary>Gets the path of this this known folder.</summary>
        /// <param name="fileExists">
        /// Returns false if the folder is virtual, or a boolean value that indicates whether this known folder exists.
        /// </param>
        /// <param name="knownFolderNative">Native IKnownFolder reference</param>
        /// <returns>A <see cref="System.String"/> containing the path, or <see cref="string.Empty"/> if this known folder does not exist.</returns>
        private string GetPath( out bool fileExists, IKnownFolderNative knownFolderNative )
        {
            Debug.Assert( knownFolderNative != null );

            var kfPath = string.Empty;
            fileExists = true;

            // Virtual folders do not have path.
            if ( _KnownFolderProperties.category == FolderCategory.Virtual )
            {
                fileExists = false;
                return (kfPath);
            }

            try
            {
                kfPath = knownFolderNative.GetPath( 0 );
            }
            catch ( FileNotFoundException )
            {
                fileExists = false;
            }
            catch ( DirectoryNotFoundException )
            {
                fileExists = false;
            }

            return (kfPath);
        }
    }
}