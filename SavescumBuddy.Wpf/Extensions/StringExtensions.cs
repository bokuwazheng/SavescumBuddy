using System;
using System.IO;

namespace SavescumBuddy.Wpf.Extensions
{
    public static class StringExtensions
    {
        public static bool IsDirectoryWritable(this string value, bool throwIfFails = false)
        {
            try
            {
                using var fs = File.Create(Path.Combine(value, Path.GetRandomFileName()), 1, FileOptions.DeleteOnClose);
                return true;
            }
            catch
            {
                if (throwIfFails)
                    throw new Exception($"Directory ({ value }) is read-only.");
                else
                    return false;
            }
        }
    }
}
