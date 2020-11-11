using System.Diagnostics.CodeAnalysis;
using System.Runtime.Intrinsics.X86;

[assembly: SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize",
                           Justification = "There is no inheritors of these classes",
                           Scope = "module")]
[assembly:
    SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression",
                    Justification = "Suppressed suppressed that disrupted suppressed",
                    Scope = "type", Target = "~T:REVUnit.AutoArknights.Core.Log.Level")]

namespace REVUnit.AutoArknights.Core
{
    public static class Library
    {
        public static bool CheckIfSupported() => Avx2.IsSupported;
    }
}