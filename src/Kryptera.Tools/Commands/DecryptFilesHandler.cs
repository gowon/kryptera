﻿namespace Kryptera.Tools.Commands
{
    using System;
    using System.Collections.Generic;
    using System.CommandLine;
    using System.CommandLine.IO;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using CryptHash.Net.Encryption.AES.AEAD;
    using Extensions;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using Pastel;

    public class DecryptFilesHandler : AsyncRequestHandler<DecryptFiles>
    {
        private readonly IConsole _console;
        private readonly ILogger<DecryptFilesHandler> _logger;

        public DecryptFilesHandler(IConsole console, ILogger<DecryptFilesHandler> logger)
        {
            _console = console ?? throw new NullReferenceException(nameof(console));
            _logger = logger ?? throw new NullReferenceException(nameof(logger));
        }

        protected override async Task Handle(DecryptFiles request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Key))
            {
                throw new ArgumentException("Key must not be empty", nameof(request.Key));
            }

            // https://github.com/dotnet/standard/issues/260#issuecomment-290834776
            // https://docs.microsoft.com/en-us/dotnet/api/system.text.utf8encoding?view=net-5.0#remarks
            var password = new UTF8Encoding(false).GetBytes(request.Key);
            var aes = new AEAD_AES_256_GCM();
            var fileMap = new List<Tuple<FileInfo, FileInfo>>();

            switch (request.Source)
            {
                case DirectoryInfo sourceDirectory:
                    DirectoryInfo targetDirectory;
                    switch (request.Target)
                    {
                        case null:
                            targetDirectory = sourceDirectory;
                            break;
                        case DirectoryInfo directoryInfo:
                            targetDirectory = directoryInfo;
                            break;
                        default:
                            throw new InvalidOperationException(
                                "Target must be a directory when Source is a directory.");
                    }

                    if (sourceDirectory.Exists)
                    {
                        fileMap.AddRange(sourceDirectory.GetFiles()
                            .Select(file => new Tuple<FileInfo, FileInfo>(file,
                                new FileInfo(Path.Combine(targetDirectory.FullName,
                                    GenerateDecryptedFilename(file))))));
                    }

                    break;
                case FileInfo sourceFile:
                    FileInfo targetFile;

                    switch (request.Target)
                    {
                        case null:
                            targetFile = new FileInfo(GenerateDecryptedFilename(sourceFile));
                            break;
                        case DirectoryInfo directoryInfo:
                            targetFile = new FileInfo(Path.Combine(directoryInfo.FullName,
                                GenerateDecryptedFilename(sourceFile)));
                            break;
                        case FileInfo fileInfo:
                            targetFile = fileInfo;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(request.Target));
                    }

                    if (sourceFile.Exists)
                    {
                        fileMap.Add(new Tuple<FileInfo, FileInfo>(sourceFile, targetFile));
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(request.Source));
            }

            var stopwatch = Stopwatch.StartNew();
            foreach (var (source, target) in fileMap)
            {
                byte[] bytes;
                if (request.DecryptBase64)
                {
                    bytes = Convert.FromBase64String(await File.ReadAllTextAsync(source.FullName, cancellationToken));
                }
                else
                {
                    bytes = await File.ReadAllBytesAsync(source.FullName, cancellationToken);
                }

                var decrypted = aes.DecryptString(bytes, password);

                if (!decrypted.Success)
                {
                    _logger.LogWarning($"Unable to decrypt {source.FullName.Pastel(Color.DimGray)}.");
                    continue;
                }

                if (!target.Exists || request.ForceOverwrite)
                {
                    await File.WriteAllBytesAsync(target.FullName, decrypted.DecryptedDataBytes, cancellationToken);
                    _console.Out.WriteLine(
                        $"Wrote {decrypted.DecryptedDataBytes.Length.ToString().Pastel(Color.DeepSkyBlue)} bytes to {target.FullName.Pastel(Color.DimGray)}.");
                }
                else
                {
                    _logger.LogWarning(
                        $"File {target.FullName.Pastel(Color.DimGray)} already exists. To overwrite, please set the --force option.");
                }
            }

            stopwatch.Stop();
            _logger.LogInformation("Operation completed in {DurationMilliseconds}ms", stopwatch.ElapsedMilliseconds);
        }

        private static string GenerateDecryptedFilename(FileSystemInfo fileInfo)
        {
            var targetFilename = fileInfo.Name;
            targetFilename = targetFilename.EndsWith(Constants.EncryptedFileExtension)
                ? targetFilename.Remove(targetFilename.Length - Constants.EncryptedFileExtension.Length)
                : $"{Path.GetFileNameWithoutExtension(fileInfo.Name)}{Constants.DecryptedFileSuffix}{Path.GetExtension(fileInfo.Name)}";

            return targetFilename;
        }
    }
}