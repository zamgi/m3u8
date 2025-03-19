namespace System.Windows.Forms.Taskbar
{
    /// <summary>Defines the enumeration values for a property type.</summary>
    public class ShellPropertyEnumType
    {
        private string _DisplayText;
        private PropEnumType? _EnumType;
        private object _MinValue, _SetValue, _EnumerationValue;
        internal ShellPropertyEnumType( IPropertyEnumType nativePropertyEnumType ) => NativePropertyEnumType = nativePropertyEnumType;

        /// <summary>Gets display text from an enumeration information structure.</summary>
        public string DisplayText
        {
            get
            {
                if ( _DisplayText == null )
                {
                    NativePropertyEnumType.GetDisplayText( out _DisplayText );
                }
                return (_DisplayText);
            }
        }

        /// <summary>Gets an enumeration type from an enumeration information structure.</summary>
        public PropEnumType EnumType
        {
            get
            {
                if ( !_EnumType.HasValue )
                {
                    NativePropertyEnumType.GetEnumType( out var tempEnumType );
                    _EnumType = tempEnumType;
                }
                return (_EnumType.Value);
            }
        }

        /// <summary>Gets a minimum value from an enumeration information structure.</summary>
        public object RangeMinValue
        {
            get
            {
                if ( _MinValue == null )
                {
                    using ( var propVar = new PropVariant() )
                    {
                        NativePropertyEnumType.GetRangeMinValue( propVar );
                        _MinValue = propVar.Value;
                    }
                }
                return (_MinValue);
            }
        }

        /// <summary>Gets a set value from an enumeration information structure.</summary>
        public object RangeSetValue
        {
            get
            {
                if ( _SetValue == null )
                {
                    using ( var propVar = new PropVariant() )
                    {
                        NativePropertyEnumType.GetRangeSetValue( propVar );
                        _SetValue = propVar.Value;
                    }
                }
                return (_SetValue);
            }
        }

        /// <summary>Gets a value from an enumeration information structure.</summary>
        public object RangeValue
        {
            get
            {
                if ( _EnumerationValue == null )
                {
                    using ( var propVar = new PropVariant() )
                    {
                        NativePropertyEnumType.GetValue( propVar );
                        _EnumerationValue = propVar.Value;
                    }
                }
                return (_EnumerationValue);
            }
        }

        private IPropertyEnumType NativePropertyEnumType { set; get; }
    }
}