namespace Kryptera.Tools.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
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
            var stopwatch = Stopwatch.StartNew();
            var aes = new AEAD_AES_256_GCM();

            var files = new List<FileInfo>();
            switch (request.Source)
            {
                case DirectoryInfo directoryInfo:
                    if (request.Target is not DirectoryInfo)
                    {
                        throw new InvalidOperationException("Target must be a directory if Source is a directory.");
                    }

                    if (directoryInfo.Exists)
                    {
                        files.AddRange(directoryInfo.GetFiles());
                    }

                    break;
                case FileInfo fileInfo:
                    if (fileInfo.Exists)
                    {
                        files.Add(fileInfo);
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(request.Source));
            }

            foreach (var file in files)
            {
                var bytes = await File.ReadAllBytesAsync(file.FullName, cancellationToken);
                var password = Convert.FromBase64String(request.Key);
                var result = aes.EncryptString(bytes, password);

                if (!result.Success)
                {
                    continue;
                }

                if (request.ConsoleOutputOnly)
                {
                    Console.WriteLine(result.EncryptedDataBase64String);
                }

                if (request.Target == null)
                {
                    var target = new FileInfo($"{file.FullName}.aes");
                    if (!target.Exists || request.ForceOverwrite)
                    {
                        await File.WriteAllTextAsync(target.FullName, result.EncryptedDataBase64String, cancellationToken);
                        Console.WriteLine(
                            $"Wrote {result.EncryptedDataBytes.Length.ToString().Pastel(Color.DeepSkyBlue)} bytes to {target.FullName.Pastel(Color.DimGray)}.");
                    }
                }
            }

            stopwatch.Stop();
            _logger.LogInformation("Operation completed in {DurationMilliseconds}ms", stopwatch.ElapsedMilliseconds);
        }
    }
}