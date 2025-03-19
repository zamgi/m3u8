using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

namespace System.Windows.Forms.Taskbar
{
    /// <summary>Defines the shell property description information for a property.</summary>
    public class ShellPropertyDescription : IDisposable
    {
        private PropertyAggregationType? _AggregationTypes;
        private string _CanonicalName;
        private PropertyColumnStateOptions? _ColumnState;
        private PropertyConditionOperation? _ConditionOperation;
        private PropertyConditionType? _ConditionType;
        private uint? _DefaultColumWidth;
        private string _DisplayName;
        private PropertyDisplayType? _DisplayType;
        private string _EditInvitation;
        private PropertyGroupingRange? _GroupingRange;
        private IPropertyDescription _NativePropertyDescription;
        private ReadOnlyCollection<ShellPropertyEnumType> _PropertyEnumTypes;
        private PropertyKey _PropertyKey;
        private PropertyTypeOptions? _PropertyTypeFlags;
        private PropertyViewOptions? _PropertyViewFlags;
        private PropertySortDescription? _SortDescription;
        private Type _ValueType;
        private VarEnum? _VarEnumType;

        internal ShellPropertyDescription( PropertyKey key ) => _PropertyKey = key;
        ~ShellPropertyDescription() => Dispose( false );

        /// <summary>
        /// Gets a value that describes how the property values are displayed when multiple items are selected in the user interface (UI).
        /// </summary>
        public PropertyAggregationType AggregationTypes
        {
            get
            {
                if ( NativePropertyDescription != null && _AggregationTypes == null )
                {
                    var hr = NativePropertyDescription.GetAggregationType( out var tempAggregationTypes );
                    if ( CoreErrorHelper.Succeeded( hr ) )
                    {
                        _AggregationTypes = tempAggregationTypes;
                    }
                }

                return (_AggregationTypes.HasValue ? _AggregationTypes.Value : default);
            }
        }

        /// <summary>Gets the case-sensitive name of a property as it is known to the system, regardless of its localized name.</summary>
        public string CanonicalName
        {
            get
            {
                if ( _CanonicalName == null )
                {
                    PropertySystemNativeMethods.PSGetNameFromPropertyKey( ref _PropertyKey, out _CanonicalName );
                }
                return (_CanonicalName);
            }
        }

        /// <summary>
        /// Gets the column state flag, which describes how the property should be treated by interfaces or APIs that use this flag.
        /// </summary>
        public PropertyColumnStateOptions ColumnState
        {
            get
            {
                // If default/first value, try to get it again, otherwise used the cached one.
                if ( NativePropertyDescription != null && _ColumnState == null )
                {
                    var hr = NativePropertyDescription.GetColumnState( out var state );
                    if ( CoreErrorHelper.Succeeded( hr ) )
                    {
                        _ColumnState = state;
                    }
                }
                return (_ColumnState.HasValue ? _ColumnState.Value : default);
            }
        }

        /// <summary>
        /// Gets the default condition operation to use when displaying the property in the query builder user interface (UI). This
        /// influences the list of predicate conditions (for example, equals, less than, and contains) that are shown for this property.
        /// </summary>
        /// <remarks>
        /// For more information, see the <c>conditionType</c> attribute of the <c>typeInfo</c> element in the property's .propdesc file.
        /// </remarks>
        public PropertyConditionOperation ConditionOperation
        {
            get
            {
                // If default/first value, try to get it again, otherwise used the cached one.
                if ( NativePropertyDescription != null && _ConditionOperation == null )
                {
                    var hr = NativePropertyDescription.GetConditionType( out var tempConditionType, out var tempConditionOperation );
                    if ( CoreErrorHelper.Succeeded( hr ) )
                    {
                        _ConditionOperation = tempConditionOperation;
                        _ConditionType = tempConditionType;
                    }
                }
                return (_ConditionOperation.HasValue ? _ConditionOperation.Value : default);
            }
        }

        /// <summary>
        /// Gets the condition type to use when displaying the property in the query builder user interface (UI). This influences the list of
        /// predicate conditions (for example, equals, less than, and
        /// contains) that are shown for this property.
        /// </summary>
        /// <remarks>
        /// For more information, see the <c>conditionType</c> attribute of the <c>typeInfo</c> element in the property's .propdesc file.
        /// </remarks>
        public PropertyConditionType ConditionType
        {
            get
            {
                // If default/first value, try to get it again, otherwise used the cached one.
                if ( NativePropertyDescription != null && _ConditionType == null )
                {
                    var hr = NativePropertyDescription.GetConditionType( out var tempConditionType, out var tempConditionOperation );
                    if ( CoreErrorHelper.Succeeded( hr ) )
                    {
                        _ConditionOperation = tempConditionOperation;
                        _ConditionType = tempConditionType;
                    }
                }
                return (_ConditionType.HasValue ? _ConditionType.Value : default);
            }
        }

        /// <summary>Gets the default user interface (UI) column width for this property.</summary>
        public uint DefaultColumWidth
        {
            get
            {
                if ( NativePropertyDescription != null && !_DefaultColumWidth.HasValue )
                {
                    var hr = NativePropertyDescription.GetDefaultColumnWidth( out var tempDefaultColumWidth );
                    if ( CoreErrorHelper.Succeeded( hr ) )
                    {
                        _DefaultColumWidth = tempDefaultColumWidth;
                    }
                }
                return (_DefaultColumWidth.HasValue ? _DefaultColumWidth.Value : default);
            }
        }

        /// <summary>Gets the display name of the property as it is shown in any user interface (UI).</summary>
        public string DisplayName
        {
            get
            {
                if ( NativePropertyDescription != null && _DisplayName == null )
                {
                    var hr = NativePropertyDescription.GetDisplayName( out var dispNameptr );
                    if ( CoreErrorHelper.Succeeded( hr ) && dispNameptr != IntPtr.Zero )
                    {
                        _DisplayName = Marshal.PtrToStringUni( dispNameptr );

                        // Free the string
                        Marshal.FreeCoTaskMem( dispNameptr );
                    }
                }
                return (_DisplayName);
            }
        }

        /// <summary>Gets the current data type used to display the property.</summary>
        public PropertyDisplayType DisplayType
        {
            get
            {
                if ( NativePropertyDescription != null && _DisplayType == null )
                {
                    var hr = NativePropertyDescription.GetDisplayType( out var tempDisplayType );
                    if ( CoreErrorHelper.Succeeded( hr ) )
                    {
                        _DisplayType = tempDisplayType;
                    }
                }
                return (_DisplayType.HasValue ? _DisplayType.Value : default);
            }
        }

        /// <summary>Gets the text used in edit controls hosted in various dialog boxes.</summary>
        public string EditInvitation
        {
            get
            {
                if ( NativePropertyDescription != null && _EditInvitation == null )
                {
                    // EditInvitation can be empty, so ignore the HR value, but don't throw an exception
                    var hr = NativePropertyDescription.GetEditInvitation( out var ptr );

                    if ( CoreErrorHelper.Succeeded( hr ) && ptr != IntPtr.Zero )
                    {
                        _EditInvitation = Marshal.PtrToStringUni( ptr );
                        // Free the string
                        Marshal.FreeCoTaskMem( ptr );
                    }
                }
                return (_EditInvitation);
            }
        }

        /// <summary>Gets the method used when a view is grouped by this property.</summary>
        /// <remarks>
        /// The information retrieved by this method comes from the <c>groupingRange</c> attribute of the <c>typeInfo</c> element in the
        /// property's .propdesc file.
        /// </remarks>
        public PropertyGroupingRange GroupingRange
        {
            get
            {
                // If default/first value, try to get it again, otherwise used the cached one.
                if ( NativePropertyDescription != null && _GroupingRange == null )
                {
                    var hr = NativePropertyDescription.GetGroupingRange( out var tempGroupingRange );
                    if ( CoreErrorHelper.Succeeded( hr ) )
                    {
                        _GroupingRange = tempGroupingRange;
                    }
                }
                return (_GroupingRange.HasValue ? _GroupingRange.Value : default);
            }
        }

        /// <summary>Gets a value that determines if the native property description is present on the system.</summary>
        public bool HasSystemDescription => NativePropertyDescription != null;

        /// <summary>Gets a list of the possible values for this property.</summary>
        public ReadOnlyCollection<ShellPropertyEnumType> PropertyEnumTypes
        {
            get
            {
                if ( NativePropertyDescription != null && _PropertyEnumTypes == null )
                {
                    var propEnumTypeList = new List<ShellPropertyEnumType>();
                    var guid             = new Guid(ShellIIDGuid.IPropertyEnumTypeList );

                    var hr = NativePropertyDescription.GetEnumTypeList( ref guid, out var nativeList );
                    if ( nativeList != null && CoreErrorHelper.Succeeded( hr ) )
                    {
                        nativeList.GetCount( out var count );
                        guid = new Guid(ShellIIDGuid.IPropertyEnumType );

                        for ( uint i = 0; i < count; i++ )
                        {
                            nativeList.GetAt( i, ref guid, out var nativeEnumType );
                            propEnumTypeList.Add( new ShellPropertyEnumType( nativeEnumType ) );
                        }
                    }

                    _PropertyEnumTypes = new ReadOnlyCollection<ShellPropertyEnumType>( propEnumTypeList );
                }
                return (_PropertyEnumTypes);
            }
        }

        /// <summary>Gets the property key identifying the underlying property.</summary>
        public PropertyKey PropertyKey => _PropertyKey;

        /// <summary>Gets the current sort description flags for the property, which indicate the particular wordings of sort offerings.</summary>
        /// <remarks>
        /// The settings retrieved by this method are set through the <c>sortDescription</c> attribute of the <c>labelInfo</c> element in the
        /// property's .propdesc file.
        /// </remarks>
        public PropertySortDescription SortDescription
        {
            get
            {
                // If default/first value, try to get it again, otherwise used the cached one.
                if ( NativePropertyDescription != null && _SortDescription == null )
                {
                    var hr = NativePropertyDescription.GetSortDescription( out var tempSortDescription );
                    if ( CoreErrorHelper.Succeeded( hr ) )
                    {
                        _SortDescription = tempSortDescription;
                    }
                }
                return (_SortDescription.HasValue ? _SortDescription.Value : default);
            }
        }

        /// <summary>Gets a set of flags that describe the uses and capabilities of the property.</summary>
        public PropertyTypeOptions TypeFlags
        {
            get
            {
                if ( NativePropertyDescription != null && _PropertyTypeFlags == null )
                {
                    var hr = NativePropertyDescription.GetTypeFlags( PropertyTypeOptions.MaskAll, out var tempFlags );
                    _PropertyTypeFlags = CoreErrorHelper.Succeeded( hr ) ? tempFlags : default;
                }

                return (_PropertyTypeFlags.HasValue ? _PropertyTypeFlags.Value : default);
            }
        }

        /// <summary>Gets the .NET system type for a value of this property, or null if the value is empty.</summary>
        public Type ValueType
        {
            get
            {
                if ( _ValueType == null )
                {
                    _ValueType = ShellPropertyFactory.VarEnumToSystemType( VarEnumType );
                }
                return (_ValueType);
            }
        }

        /// <summary>Gets the VarEnum OLE type for this property.</summary>
        public VarEnum VarEnumType
        {
            get
            {
                if ( NativePropertyDescription != null && _VarEnumType == null )
                {
                    var hr = NativePropertyDescription.GetPropertyType( out var tempType );
                    if ( CoreErrorHelper.Succeeded( hr ) )
                    {
                        _VarEnumType = tempType;
                    }
                }
                return (_VarEnumType.HasValue ? _VarEnumType.Value : default);
            }
        }

        /// <summary>Gets the current set of flags governing the property's view.</summary>
        public PropertyViewOptions ViewFlags
        {
            get
            {
                if ( NativePropertyDescription != null && _PropertyViewFlags == null )
                {
                    var hr = NativePropertyDescription.GetViewFlags( out var tempFlags );
                    _PropertyViewFlags = CoreErrorHelper.Succeeded( hr ) ? tempFlags : default;
                }
                return (_PropertyViewFlags.HasValue ? _PropertyViewFlags.Value : default);
            }
        }

        /// <summary>Get the native property description COM interface</summary>
        internal IPropertyDescription NativePropertyDescription
        {
            get
            {
                if ( _NativePropertyDescription == null )
                {
                    var guid = new Guid(ShellIIDGuid.IPropertyDescription );
                    PropertySystemNativeMethods.PSGetPropertyDescription( ref _PropertyKey, ref guid, out _NativePropertyDescription );
                }
                return (_NativePropertyDescription);
            }
        }

        /// <summary>Release the native objects</summary>
        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        /// <summary>Gets the localized display string that describes the current sort order.</summary>
        /// <param name="descending">
        /// Indicates the sort order should reference the string "Z on top"; otherwise, the sort order should reference the string "A on top".
        /// </param>
        /// <returns>The sort description for this property.</returns>
        /// <remarks>
        /// The string retrieved by this method is determined by flags set in the <c>sortDescription</c> attribute of the <c>labelInfo</c>
        /// element in the property's .propdesc file.
        /// </remarks>
        public string GetSortDescriptionLabel( bool descending )
        {
            var label = string.Empty;

            if ( NativePropertyDescription != null )
            {
                var hr = NativePropertyDescription.GetSortDescriptionLabel( descending, out var ptr );
                if ( CoreErrorHelper.Succeeded( hr ) && ptr != IntPtr.Zero )
                {
                    label = Marshal.PtrToStringUni( ptr );
                    // Free the string
                    Marshal.FreeCoTaskMem( ptr );
                }
            }

            return (label);
        }

        /// <summary>Release the native objects</summary>
        /// <param name="disposing">Indicates that this is being called from Dispose(), rather than the finalizer.</param>
        protected virtual void Dispose( bool disposing )
        {
            if ( _NativePropertyDescription != null )
            {
                Marshal.ReleaseComObject( _NativePropertyDescription );
                _NativePropertyDescription = null;
            }

            if ( disposing )
            {
                // and the managed ones
                _CanonicalName     = null;
                _DisplayName       = null;
                _EditInvitation    = null;
                _DefaultColumWidth = null;
                _ValueType         = null;
                _PropertyEnumTypes = null;
            }
        }
    }
}