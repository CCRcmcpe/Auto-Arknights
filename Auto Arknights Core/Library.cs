﻿using System.Diagnostics.CodeAnalysis;
using System.Runtime.Intrinsics.X86;

[assembly: SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize", Scope = "module")]

[assembly:
    SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Scope = "type",
                    Target = "~T:REVUnit.AutoArknights.Core.Log.Level")]

[assembly:
    SuppressMessage("Performance", "CA1806:Do not ignore method results", Scope = "member",
                    Target =
                        "~M:REVUnit.AutoArknights.Core.Tasks.Suspend.Execute~REVUnit.AutoArknights.Core.Tasks.ExecuteResult")]

namespace REVUnit.AutoArknights.Core
{
    public static class Library
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public static ISettings Settings { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public static bool CheckIfSupported() => Avx2.IsSupported;
    }
}