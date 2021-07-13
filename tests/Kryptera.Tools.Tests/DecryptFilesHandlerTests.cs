namespace Kryptera.Tools.Tests
{
    using System;
    using System.Collections.Generic;
    using System.CommandLine.IO;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Commands;
    using CryptHash.Net.Util;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Xunit;
    using Xunit.Categories;
    using static Helpers.Utilities;

    [IntegrationTest]
    public class DecryptFilesHandlerTests : IDisposable
    {
        private const string Key = "urT8KiJwZuGIkcArTCRLrsQqGWYh2M5iX5fPGgAK0WU=";
        private readonly TestConsole _console;
        private readonly Mock<ILogger<DecryptFilesHandler>> _loggerMock;
        private readonly DirectoryInfo _targetDirectory;
        private readonly DirectoryInfo _tempDirectory;

        public DecryptFilesHandlerTests()
        {
            var testDirectory = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "TestSets", "Encrypted"));
            _tempDirectory = new DirectoryInfo(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
            _targetDirectory = new DirectoryInfo(Path.Combine(_tempDirectory.FullName, "decrypted"));
            _console = new TestConsole();
            _loggerMock = new Mock<ILogger<DecryptFilesHandler>>();

            testDirectory.CopyAllFilesTo(_tempDirectory);
        }

        public void Dispose()
        {
            _tempDirectory.Delete(true);
        }

        public static IEnumerable<object[]> GetKeyFileData()
        {
            var files = new[] {"sample1.txt.aes", "sample2.txt.aes", "sample3.txt.aes"};
            return files.Select(s => new object[] {s});
        }

        [IntegrationTest]
        [Theory]
        [MemberData(nameof(GetKeyFileData))]
        public async Task GenerateDecryptedFile(string path)
        {
            // Arrange
            // https://github.com/jbogard/MediatR/issues/526#issue-645312126
            IRequestHandler<DecryptFiles> sut = new DecryptFilesHandler(_console, _loggerMock.Object);
            var inputFile = new FileInfo(Path.Combine(_tempDirectory.FullName, path));
            var decryptedFile = new FileInfo($"{inputFile.FullName[0..^4]}");

            // Act
            await sut.Handle(new DecryptFiles
            {
                Source = inputFile,
                Target = null,
                Key = Key,
                DecryptBase64 = false,
                ForceOverwrite = true
            }, default);

            decryptedFile.Refresh();

            // Assert
            Assert.True(decryptedFile.Exists, $"Could not find expected file '{decryptedFile.FullName}'.");
            var content = await File.ReadAllTextAsync(decryptedFile.FullName);
            Assert.NotEmpty(content);
            //Assert.True(IsBase64String(content), "File content is not a Base64-encoded string.");
        }

        [IntegrationTest]
        [Fact]
        public async Task HandlerDecryptsDirectory()
        {
            // Arrange
            // https://github.com/jbogard/MediatR/issues/526#issue-645312126
            IRequestHandler<DecryptFiles> handler = new DecryptFilesHandler(_console, _loggerMock.Object);

            // Act
            await handler.Handle(new DecryptFiles
            {
                Source = _tempDirectory,
                Target = _targetDirectory,
                Key = Key,
                DecryptBase64 = false,
                ForceOverwrite = true
            }, default);

            _targetDirectory.Refresh();

            // Assert
            Assert.NotEmpty(_targetDirectory.GetFiles());
        }
    }
}