namespace Kryptera.Tools.Tests
{
    using System;
    using System.CommandLine.IO;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Commands;
    using CryptHash.Net.Util;
    using Extensions;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using Moq;
    using NeoSmart.StreamCompare;
    using Xunit;
    using Xunit.Categories;

    [UnitTest(nameof(EncryptFilesHandler))]
    [UnitTest(nameof(DecryptFilesHandler))]
    public class EncryptFilesHandlerTests
    {
        [Fact]
        public async Task HandlerEncryptsDirectory()
        {
            var console = new TestConsole();
            var loggerMock = new Mock<ILogger<EncryptFilesHandler>>();
            var key = Convert.ToBase64String(CommonMethods.Generate256BitKey());

            var testDirectory = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "Samples"));
            var tempDirectory = new DirectoryInfo(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
            var targetDirectory = new DirectoryInfo(Path.Combine(tempDirectory.FullName, "encrypted"));
            testDirectory.CopyAllFilesTo(tempDirectory);

            // https://github.com/jbogard/MediatR/issues/526#issue-645312126
            IRequestHandler<EncryptFiles> handler = new EncryptFilesHandler(console, loggerMock.Object);
            await handler.Handle(new EncryptFiles
            {
                Source = tempDirectory,
                Target = targetDirectory,
                Key = key,
                EncryptToBase64 = true,
                ForceOverwrite = true
            }, default);
            
            Assert.NotNull(console.Out.ToString());
        }

        [Fact]
        public async Task EncryptDecryptRoundTrip()
        {
            var console = new TestConsole();
            var loggerMock = new Mock<ILogger<EncryptFilesHandler>>();
            var key = Convert.ToBase64String(CommonMethods.Generate256BitKey());

            var testDirectory = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "Samples"));
            var tempDirectory = new DirectoryInfo(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
            var targetDirectory = new DirectoryInfo(Path.Combine(tempDirectory.FullName, "encrypted"));
            testDirectory.CopyAllFilesTo(tempDirectory);

            // https://github.com/jbogard/MediatR/issues/526#issue-645312126
            IRequestHandler<EncryptFiles> handler = new EncryptFilesHandler(console, loggerMock.Object);
            await handler.Handle(new EncryptFiles
            {
                Source = tempDirectory,
                Target = targetDirectory,
                Key = key,
                EncryptToBase64 = true,
                ForceOverwrite = true
            }, default);

            //Assert.NotNull(console.Out.ToString());

            console = new TestConsole();
            var loggerMock2 = new Mock<ILogger<DecryptFilesHandler>>();
            targetDirectory.Refresh();
            IRequestHandler<DecryptFiles> handler2 = new DecryptFilesHandler(console, loggerMock2.Object);
            await handler2.Handle(new DecryptFiles
            {
                Source = targetDirectory,
                Target = null,
                Key = key,
                DecryptBase64 = true,
                ForceOverwrite = true
            }, default);

            //Assert.NotNull(console.Out.ToString());

            var compare = new FileCompare();
            foreach (var file in tempDirectory.GetFiles())
            {
                var success =
                    await compare.AreEqualAsync(file.FullName, Path.Combine(targetDirectory.FullName, file.Name));
                Assert.True(success);
            }

            targetDirectory.Refresh();
            foreach (var file in targetDirectory.GetFiles().Where(info =>
                info.Extension.Equals(Constants.EncryptedFileExtension, StringComparison.OrdinalIgnoreCase)))
            {
                var x = await Kryptera.DecryptBase64FileAsync(file, key);
                var encWithoutBom = new UTF8Encoding(false);
                var y = encWithoutBom.GetString(await Kryptera.DecryptBase64FileBytesAsync(file, key))
                    .TrimStart('\uFEFF', '\u200B');
                var success = x.Equals(y);
            }


            tempDirectory.Delete(true);
        }
    }
}