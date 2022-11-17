using System;
using System.Reflection;

using Avalonia.Markup.Xaml.Styling;

namespace m3u8.download.manager.ui
{
    /// <summary>
    /// 
    /// </summary>
    internal static class GlobalStyles
    {
        private static StyleInclude _Dark;
        public static StyleInclude Dark
        {
            get
            {
                if ( _Dark == null )
                {
                    var selfAssemblyName = Assembly.GetExecutingAssembly().GetName().Name;

                    _Dark = new StyleInclude( new Uri( $"resm:Styles?assembly={selfAssemblyName}" ) )
                    {
                        Source = new Uri( "resm:Avalonia.Themes.Default.Accents.BaseDark.xaml?assembly=Avalonia.Themes.Default" )
                    };
                }
                return (_Dark);
            }
        }


        private static StyleInclude _Light;
        public static StyleInclude Light
        {
            get
            {
                if ( _Light == null )
                {
                    var selfAssemblyName = Assembly.GetExecutingAssembly().GetName().Name;

                    _Light = new StyleInclude( new Uri( $"resm:Styles?assembly={selfAssemblyName}" ) )
                    {
                        Source = new Uri( "resm:Avalonia.Themes.Default.Accents.BaseLight.xaml?assembly=Avalonia.Themes.Default" )
                    };
                }
                return (_Light);
            }
        }
    }
}
