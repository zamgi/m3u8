using System.Globalization;

namespace System.Windows.Forms.Taskbar
{
    /// <summary>A refence to an icon resource</summary>
    public struct IconReference
    {
        private static readonly char[] COMMA_SEPARATOR = [ ',' ];
        private string _ModuleName;
        private string _ReferencePath;

        /// <summary>Overloaded constructor takes in the module name and resource id for the icon reference.</summary>
        /// <param name="moduleName">String specifying the name of an executable file, DLL, or icon file</param>
        /// <param name="resourceId">Zero-based index of the icon</param>
        public IconReference( string moduleName, int resourceId )
        {
            if ( string.IsNullOrEmpty( moduleName ) ) throw (new ArgumentNullException( nameof(moduleName) ));

            _ModuleName = moduleName;
            ResourceId = resourceId;
            _ReferencePath = string.Format( CultureInfo.InvariantCulture, "{0},{1}", moduleName, resourceId );
        }

        /// <summary>Overloaded constructor takes in the module name and resource id separated by a comma.</summary>
        /// <param name="refPath">Reference path for the icon consiting of the module name and resource id.</param>
        public IconReference( string refPath )
        {
            if ( string.IsNullOrEmpty( refPath ) ) throw (new ArgumentNullException( "refPath" ));

            var refParams = refPath.Split( COMMA_SEPARATOR );
            if ( (refParams.Length != 2) || string.IsNullOrEmpty( refParams[ 0 ] ) || string.IsNullOrEmpty( refParams[ 1 ] ) )
            {
                throw (new ArgumentException( "LocalizedMessages.InvalidReferencePath", "refPath" ));
            }

            _ModuleName = refParams[ 0 ];
            ResourceId = int.Parse( refParams[ 1 ], CultureInfo.InvariantCulture );

            _ReferencePath = refPath;
        }

        /// <summary>String specifying the name of an executable file, DLL, or icon file</summary>
        public string ModuleName
        {
            get => _ModuleName;
            set
            {
                if ( string.IsNullOrEmpty( value ) ) throw (new ArgumentNullException( "value" ));
                _ModuleName = value;
            }
        }

        /// <summary>Reference to a specific icon within a EXE, DLL or icon file.</summary>
        public string ReferencePath
        {
            get => _ReferencePath;
            set
            {
                if ( string.IsNullOrEmpty( value ) ) throw (new ArgumentNullException( "value" ));

                var refParams = value.Split( COMMA_SEPARATOR );
                if ( refParams.Length != 2 || string.IsNullOrEmpty( refParams[ 0 ] ) || string.IsNullOrEmpty( refParams[ 1 ] ) )
                {
                    throw (new ArgumentException( "LocalizedMessages.InvalidReferencePath", "value" ));
                }

                ModuleName = refParams[ 0 ];
                ResourceId = int.Parse( refParams[ 1 ], CultureInfo.InvariantCulture );

                _ReferencePath = value;
            }
        }

        /// <summary>Zero-based index of the icon</summary>
        public int ResourceId { get; set; }

        /// <summary>Implements the != (unequality) operator.</summary>
        /// <param name="icon1">First object to compare.</param>
        /// <param name="icon2">Second object to compare.</param>
        /// <returns>True if icon1 does not equals icon1; false otherwise.</returns>
        public static bool operator !=( IconReference icon1, IconReference icon2 ) => !(icon1 == icon2);

        /// <summary>Implements the == (equality) operator.</summary>
        /// <param name="icon1">First object to compare.</param>
        /// <param name="icon2">Second object to compare.</param>
        /// <returns>True if icon1 equals icon1; false otherwise.</returns>
        public static bool operator ==( IconReference icon1, IconReference icon2 ) => (icon1._ModuleName    == icon2._ModuleName) &&
                                                                                      (icon1._ReferencePath == icon2._ReferencePath) &&
                                                                                      (icon1.ResourceId     == icon2.ResourceId);

        /// <summary>Determines if this object is equal to another.</summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>Returns true if the objects are equal; false otherwise.</returns>
        public override bool Equals( object obj )
        {
            if ( (obj == null) || !(obj is IconReference iconRef) ) return (false);
            return (this == iconRef);
        }

        /// <summary>Generates a nearly unique hashcode for this structure.</summary>
        /// <returns>A hash code.</returns>
        public override int GetHashCode()
        {
            var hash = _ModuleName.GetHashCode();
            hash = hash * 31 + _ReferencePath.GetHashCode();
            hash = hash * 31 + ResourceId.GetHashCode();
            return (hash);
        }
    }
}