﻿using System.Runtime.InteropServices;

namespace System.Windows.Forms.Taskbar
{
    /// <summary>
    /// Defines a partial class that implements helper methods for retrieving Shell properties using a canonical name, property key, or a
    /// strongly-typed property. Also provides access to all the strongly-typed system properties and default properties collections.
    /// </summary>
    public partial class ShellProperties : IDisposable
    {
        private ShellPropertyCollection _DefaultPropertyCollection;
        private PropertySystem _PropertySystem;

        internal ShellProperties( ShellObject parent ) => ParentShellObject = parent;

        /// <summary>Gets the collection of all the default properties for this item.</summary>
        public ShellPropertyCollection DefaultPropertyCollection
        {
            get
            {
                if ( _DefaultPropertyCollection == null )
                {
                    _DefaultPropertyCollection = new ShellPropertyCollection( ParentShellObject );
                }
                return (_DefaultPropertyCollection);
            }
        }

        /// <summary>Gets all the properties for the system through an accessor.</summary>
        public PropertySystem System
        {
            get
            {
                if ( _PropertySystem == null )
                {
                    _PropertySystem = new PropertySystem( ParentShellObject );
                }
                return (_PropertySystem);
            }
        }

        private ShellObject ParentShellObject { get; set; }

        /// <summary>Cleans up memory</summary>
        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        /// <summary>Returns a property available in the default property collection using the given property key.</summary>
        /// <param name="key">The property key.</param>
        /// <returns>An IShellProperty.</returns>
        public IShellProperty GetProperty( PropertyKey key ) => CreateTypedProperty( key );

        /// <summary>Returns a property available in the default property collection using the given canonical name.</summary>
        /// <param name="canonicalName">The canonical name.</param>
        /// <returns>An IShellProperty.</returns>
        public IShellProperty GetProperty( string canonicalName ) => CreateTypedProperty( canonicalName );

        /// <summary>Returns a strongly typed property available in the default property collection using the given property key.</summary>
        /// <typeparam name="T">The type of property to retrieve.</typeparam>
        /// <param name="key">The property key.</param>
        /// <returns>A strongly-typed ShellProperty for the given property key.</returns>
        public ShellProperty<T> GetProperty<T>( PropertyKey key ) => CreateTypedProperty( key ) as ShellProperty<T>;

        /// <summary>Returns a strongly typed property available in the default property collection using the given canonical name.</summary>
        /// <typeparam name="T">The type of property to retrieve.</typeparam>
        /// <param name="canonicalName">The canonical name.</param>
        /// <returns>A strongly-typed ShellProperty for the given canonical name.</returns>
        public ShellProperty<T> GetProperty<T>( string canonicalName ) => CreateTypedProperty( canonicalName ) as ShellProperty<T>;

        /// <summary>Returns the shell property writer used when writing multiple properties.</summary>
        /// <returns>A ShellPropertyWriter.</returns>
        /// <remarks>
        /// Use the Using pattern with the returned ShellPropertyWriter or manually call the Close method on the writer to commit the changes
        /// and dispose the writer
        /// </remarks>
        public ShellPropertyWriter GetPropertyWriter() => new ShellPropertyWriter( ParentShellObject );

        internal IShellProperty CreateTypedProperty<T>( PropertyKey propKey )
        {
            var desc = ShellPropertyDescriptionsCache.Cache.GetPropertyDescription( propKey );
            return (new ShellProperty<T>( propKey, desc, ParentShellObject ));
        }

        internal IShellProperty CreateTypedProperty( PropertyKey propKey ) => ShellPropertyFactory.CreateShellProperty( propKey, ParentShellObject );

        internal IShellProperty CreateTypedProperty( string canonicalName )
        {
            // Otherwise, call the native PropertyStore method

            var result = PropertySystemNativeMethods.PSGetPropertyKeyFromName( canonicalName, out var propKey );

            if ( !CoreErrorHelper.Succeeded( result ) )
            {
                throw (new ArgumentException( "LocalizedMessages.ShellInvalidCanonicalName", Marshal.GetExceptionForHR( result ) ));
            }
            return (CreateTypedProperty( propKey ));
        }

        protected virtual void Dispose( bool disposed )
        {
            if ( disposed )
            {
                _DefaultPropertyCollection?.Dispose();
            }
        }
    }
}