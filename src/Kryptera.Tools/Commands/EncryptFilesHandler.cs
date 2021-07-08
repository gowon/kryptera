namespace Kryptera.Tools.Commands
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

    public class EncryptFilesHandler : AsyncRequestHandler<EncryptFiles>
    {
        private readonly IConsole _console;
        private readonly ILogger<EncryptFilesHandler> _logger;

        public EncryptFilesHandler(IConsole console, ILogger<EncryptFilesHandler> logger)
        {
            _console = console ?? throw new NullReferenceException(nameof(console));
            _logger = logger ?? throw new NullReferenceException(nameof(logger));
        }

        protected override async Task Handle(EncryptFiles request, CancellationToken cancellationToken)
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

                    if (!targetDirectory.Exists)
                    {
                        targetDirectory.Create();
                    }

                    if (sourceDirectory.Exists)
                    {
                        fileMap.AddRange(sourceDirectory.GetFiles()
                            .Where(info => !info.Extension.Equals(Constants.EncryptedFileExtension,
                                StringComparison.OrdinalIgnoreCase))
                            .Select(file => new Tuple<FileInfo, FileInfo>(file,
                                new FileInfo(Path.Combine(targetDirectory.FullName,
                                    $"{file.Name}{Constants.EncryptedFileExtension}")))));
                    }

                    break;
                case FileInfo sourceFile:
                    FileInfo targetFile;

                    switch (request.Target)
                    {
                        case null:
                            targetFile = new FileInfo($"{sourceFile.FullName}{Constants.EncryptedFileExtension}");
                            break;
                        case DirectoryInfo directoryInfo:
                            targetFile = new FileInfo(Path.Combine(directoryInfo.FullName,
                                $"{sourceFile.Name}{Constants.EncryptedFileExtension}"));
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
                var bytes = await File.ReadAllBytesAsync(source.FullName, cancellationToken);
                var encrypted = aes.EncryptString(bytes, password);

                if (!encrypted.Success)
                {
                    _logger.LogWarning($"Unable to encrypt {source.FullName.Pastel(Color.DimGray)}.");
                    continue;
                }

                if (!target.Exists || request.ForceOverwrite)
                {
                    if (request.EncryptToBase64)
                    {
                        await File.WriteAllTextAsync(target.FullName, encrypted.EncryptedDataBase64String,
                            cancellationToken);
                    }
                    else
                    {
                        await File.WriteAllBytesAsync(target.FullName, encrypted.EncryptedDataBytes, cancellationToken);
                    }

                    var length = request.EncryptToBase64
                        ? encrypted.EncryptedDataBase64String.Length
                        : encrypted.EncryptedDataBytes.Length;

                    _console.Out.WriteLine(
                        $"Wrote {length.ToString().Pastel(Color.DeepSkyBlue)} bytes to {target.FullName.Pastel(Color.DimGray)}.");
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
    }
}