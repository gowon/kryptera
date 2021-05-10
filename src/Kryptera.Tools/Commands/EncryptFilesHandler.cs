namespace Kryptera.Tools.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using CryptHash.Net.Encryption.AES.AEAD;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using Pastel;

    public class EncryptFilesHandler : AsyncRequestHandler<EncryptFiles>
    {
        private readonly ILogger<EncryptFilesHandler> _logger;

        public EncryptFilesHandler(ILogger<EncryptFilesHandler> logger)
        {
            _logger = logger;
        }

        protected override async Task Handle(EncryptFiles request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Key))
            {
                throw new ArgumentException("Key must not be empty", nameof(request.Key));
            }

            var password = Convert.FromBase64String(request.Key);
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
                                new FileInfo(Path.Combine(targetDirectory.FullName, $"{file.Name}.aes")))));
                    }

                    break;
                case FileInfo sourceFile:
                    FileInfo targetFile;

                    switch (request.Target)
                    {
                        case null:
                            targetFile = new FileInfo($"{sourceFile.FullName}.aes");
                            break;
                        case DirectoryInfo directoryInfo:
                            targetFile = new FileInfo(Path.Combine(directoryInfo.FullName, $"{sourceFile.Name}.aes"));
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

                    Console.WriteLine(
                        $"Wrote {length.ToString().Pastel(Color.DeepSkyBlue)} bytes to {target.FullName.Pastel(Color.DimGray)}.");
                }
                else
                {
                    _logger.LogWarning($"File {target.FullName.Pastel(Color.DimGray)} already exists. To overwrite, please set the --force option.");
                }
            }

            stopwatch.Stop();
            _logger.LogInformation("Operation completed in {DurationMilliseconds}ms", stopwatch.ElapsedMilliseconds);
        }
    }
}