#if !NETCOREAPP
namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// 
    /// </summary>
    internal static class IsExternalInit { }
    /// <summary>
    /// 
    /// </summary>
    internal sealed class RequiredMemberAttribute : Attribute { }
    /// <summary>
    /// 
    /// </summary>
    internal sealed class CompilerFeatureRequiredAttribute : Attribute
    {
        public CompilerFeatureRequiredAttribute( string featureName ) { }
    }
}
namespace System.Diagnostics.CodeAnalysis
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class SetsRequiredMembersAttribute : Attribute { }
}
#endif