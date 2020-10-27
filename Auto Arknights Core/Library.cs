using System.Diagnostics.CodeAnalysis;
using System.Runtime.Intrinsics.X86;

[assembly: SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize",
                           Justification = "There is no inheritors of these classes",
                           Scope = "module")]

namespace REVUnit.AutoArknights.Core
{
    public static class Library
    {
        public static bool CheckIfSupported() => Avx2.IsSupported;
    }
}