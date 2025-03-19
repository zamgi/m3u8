using System.Globalization;
using System.Runtime.InteropServices;

namespace System.Windows.Forms.Taskbar
{
    /// <summary>Defines a unique key for a Shell Property</summary>
    [StructLayout(LayoutKind.Sequential, Pack=4)]
    public struct PropertyKey : IEquatable<PropertyKey>
    {
        private Guid _FormatId;
        private readonly int _PropertyId;

        /// <summary>A unique GUID for the property</summary>
        public Guid FormatId => _FormatId;

        /// <summary>Property identifier (PID)</summary>
        public int PropertyId => _PropertyId;

        /// <summary>PropertyKey Constructor</summary>
        /// <param name="formatId">A unique GUID for the property</param>
        /// <param name="propertyId">Property identifier (PID)</param>
        public PropertyKey( Guid formatId, int propertyId )
        {
            _FormatId = formatId;
            _PropertyId = propertyId;
        }

        /// <summary>PropertyKey Constructor</summary>
        /// <param name="formatId">A string represenstion of a GUID for the property</param>
        /// <param name="propertyId">Property identifier (PID)</param>
        public PropertyKey( string formatId, int propertyId )
        {
            _FormatId = new Guid(formatId );
            _PropertyId = propertyId;
        }

        /// <summary>Returns whether this object is equal to another. This is vital for performance of value types.</summary>
        /// <param name="other">The object to compare against.</param>
        /// <returns>Equality result.</returns>
        public bool Equals( PropertyKey other ) => other.Equals( (object) this );

        /// <summary>Returns the hash code of the object. This is vital for performance of value types.</summary>
        
        public override int GetHashCode() => _FormatId.GetHashCode() ^ _PropertyId;

        /// <summary>Returns whether this object is equal to another. This is vital for performance of value types.</summary>
        /// <param name="obj">The object to compare against.</param>
        /// <returns>Equality result.</returns>
        public override bool Equals( object obj )
        {
            if ( obj == null )
                return (false);

            if ( !(obj is PropertyKey other) )
                return (false);

            return other._FormatId.Equals( _FormatId ) && (other._PropertyId == _PropertyId);
        }

        /// <summary>Implements the == (equality) operator.</summary>
        /// <param name="propKey1">First property key to compare.</param>
        /// <param name="propKey2">Second property key to compare.</param>
        /// <returns>true if object a equals object b. false otherwise.</returns>
        public static bool operator ==( in PropertyKey propKey1, in PropertyKey propKey2 ) => propKey1.Equals( propKey2 );

        /// <summary>Implements the != (inequality) operator.</summary>
        /// <param name="propKey1">First property key to compare</param>
        /// <param name="propKey2">Second property key to compare.</param>
        /// <returns>true if object a does not equal object b. false otherwise.</returns>
        public static bool operator !=( in PropertyKey propKey1, in PropertyKey propKey2 ) => !propKey1.Equals( propKey2 );

        /// <summary>Override ToString() to provide a user friendly string representation</summary>
        /// <returns>String representing the property key</returns>
        public override string ToString() => $"{_FormatId:B}, {_PropertyId}";
    }
}