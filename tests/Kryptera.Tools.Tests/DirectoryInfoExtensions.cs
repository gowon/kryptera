namespace Kryptera.Tools.Tests
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;

    [ExcludeFromCodeCoverage]
    public static class DirectoryInfoExtensions
    {
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