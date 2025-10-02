namespace System.Runtime.CompilerServices;
//https://stackoverflow.com/questions/64749385/predefined-type-system-runtime-compilerservices-isexternalinit-is-not-defined
//https://github.com/dotnet/core/issues/8016

internal static class IsExternalInit
{
}

internal class RequiredMemberAttribute : Attribute
{
}

internal class CompilerFeatureRequiredAttribute : Attribute
{
    public CompilerFeatureRequiredAttribute(string name)
    {
    }
}