namespace System.Windows.Forms.Taskbar
{
    /// <summary>Defines the read-only properties for default shell icon sizes.</summary>
    public static class DefaultIconSize
    {
#if WPF
        /// <summary>The extra-large size property for a 256x256 pixel Shell Icon.</summary>
        public static readonly System.Windows.Size ExtraLarge = new System.Windows.Size( 256, 256 );

        /// <summary>The large size property for a 48x48 pixel Shell Icon.</summary>
        public static readonly System.Windows.Size Large = new System.Windows.Size( 48, 48 );

        /// <summary>The maximum size for a Shell Icon, 256x256 pixels.</summary>
        public static readonly System.Windows.Size Maximum = new System.Windows.Size( 256, 256 );

        /// <summary>The medium size property for a 32x32 pixel Shell Icon.</summary>
        public static readonly System.Windows.Size Medium = new System.Windows.Size( 32, 32 );

        /// <summary>The small size property for a 16x16 pixel Shell Icon.</summary>
        public static readonly System.Windows.Size Small = new System.Windows.Size( 16, 16 );
#endif
    }

    /// <summary>Defines the read-only properties for default shell thumbnail sizes.</summary>
    public static class DefaultThumbnailSize
    {
#if WPF
        /// <summary>Gets the extra-large size property for a 1024x1024 pixel Shell Thumbnail.</summary>
        public static readonly System.Windows.Size ExtraLarge = new System.Windows.Size( 1024, 1024 );

        /// <summary>Gets the large size property for a 256x256 pixel Shell Thumbnail.</summary>
        public static readonly System.Windows.Size Large = new System.Windows.Size( 256, 256 );

        /// <summary>Maximum size for the Shell Thumbnail, 1024x1024 pixels.</summary>
        public static readonly System.Windows.Size Maximum = new System.Windows.Size( 1024, 1024 );

        /// <summary>Gets the medium size property for a 96x96 pixel Shell Thumbnail.</summary>
        public static readonly System.Windows.Size Medium = new System.Windows.Size( 96, 96 );

        /// <summary>Gets the small size property for a 32x32 pixel Shell Thumbnail.</summary>
        public static readonly System.Windows.Size Small = new System.Windows.Size( 32, 32 );
#endif
    }
}