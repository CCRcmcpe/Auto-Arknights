using System.Runtime.Intrinsics.X86;

namespace REVUnit.AutoArknights.Core
{
    public static class Library
    {
        public static bool CheckIfSupported()
        {
            return Avx2.IsSupported;
        }
    }
}