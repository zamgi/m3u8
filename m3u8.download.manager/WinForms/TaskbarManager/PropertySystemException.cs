using System.Runtime.InteropServices;
#if !(NETCOREAPP)
using System.Runtime.Serialization;
#endif

namespace System.Windows.Forms.Taskbar
{
    /// <summary>An exception thrown when an error occurs while dealing with the Property System API.</summary>
#if !(NETCOREAPP)
    [Serializable] 
#endif
    public class PropertySystemException : ExternalException
    {
        /// <summary>Default constructor.</summary>
        public PropertySystemException() { }

        /// <summary>Initializes an excpetion with a custom message.</summary>
        public PropertySystemException( string message ) : base( message ) { }

        /// <summary>Initializes an exception with custom message and inner exception.</summary>
        public PropertySystemException( string message, Exception innerException ) : base( message, innerException ) { }

        /// <summary>Initializes an exception with custom message and error code.</summary>
        public PropertySystemException( string message, int errorCode ) : base( message, errorCode ) { }
#if !(NETCOREAPP)
        /// <summary>Initializes an exception from serialization info and a context.</summary>
        protected PropertySystemException( SerializationInfo info, StreamingContext context ) : base( info, context ) { }
#endif
    }
}