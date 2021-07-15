namespace Kryptera.Tools.Tests.Helpers
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;

    [ExcludeFromCodeCoverage]
    internal static class Utilities
    {
        public static bool IsBase64String(string value)
        {
            try
            {
                _ = Convert.FromBase64String(value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void CopyAllFilesTo(this DirectoryInfo source, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);
            foreach (var fileInfo in source.GetFiles())
            {
                Console.WriteLine(@"Copying {0}\{1}", target.FullName, fileInfo.Name);
                fileInfo.CopyTo(Path.Combine(target.FullName, fileInfo.Name), true);
            }

            foreach (var sourceSubDir in source.GetDirectories())
            {
                var targetSubDir =
                    target.CreateSubdirectory(sourceSubDir.Name);
                CopyAllFilesTo(sourceSubDir, targetSubDir);
            }
        }
    }
}