using System.Collections.Generic;

namespace System.Windows.Forms.Taskbar
{
    internal class ShellPropertyDescriptionsCache
    {
        private static ShellPropertyDescriptionsCache _CacheInstance;
        private readonly IDictionary<PropertyKey, ShellPropertyDescription> _PropsDictionary;
        private ShellPropertyDescriptionsCache() => _PropsDictionary = new Dictionary<PropertyKey, ShellPropertyDescription>();

        public static ShellPropertyDescriptionsCache Cache
        {
            get
            {
                if ( _CacheInstance == null )
                {
                    _CacheInstance = new ShellPropertyDescriptionsCache();
                }
                return (_CacheInstance);
            }
        }

        public ShellPropertyDescription GetPropertyDescription( PropertyKey key )
        {
            if ( !_PropsDictionary.TryGetValue( key, out var val ) )
            {
                val = new ShellPropertyDescription( key );
                _PropsDictionary.Add( key, val );
            }
            return (val);
        }
    }
}