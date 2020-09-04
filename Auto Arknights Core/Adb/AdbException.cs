using System;

namespace REVUnit.AutoArknights.Core
{
    public class AdbException : Exception
    {
        public AdbException(string? message) : base(message)
        {
        }
    }
}