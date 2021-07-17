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
    public class EncryptFilesHandlerTests : IDisposable
    {
        private readonly TestConsole _console;
        private readonly Mock<ILogger<EncryptFilesHandler>> _loggerMock;
        private readonly DirectoryInfo _targetDirectory;
        private readonly DirectoryInfo _tempDirectory;

        public EncryptFilesHandlerTests()
        {
            var testDirectory = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "TestSets", "Decrypted"));
            _tempDirectory = new DirectoryInfo(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
            _targetDirectory = new DirectoryInfo(Path.Combine(_tempDirectory.FullName, "encrypted"));
            _console = new TestConsole();
            _loggerMock = new Mock<ILogger<EncryptFilesHandler>>();

            testDirectory.CopyAllFilesTo(_tempDirectory);
        }

        public void Dispose()
        {
            _tempDirectory.Delete(true);
        }

        public static IEnumerable<object[]> GetKeys(int count)
        {
            for (var i = 0; i < count; i++)
            {
                yield return new object[] {Convert.ToBase64String(CommonMethods.Generate256BitKey())};
            }
        }

        public static IEnumerable<object[]> GetKeyFileData()
        {
            var files = new[] {"sample1.txt", "sample2.txt", "sample3.txt"};
            return files.Select(s => new object[] {Convert.ToBase64String(CommonMethods.Generate256BitKey()), s});
        }

        [IntegrationTest]
        [Theory]
        [MemberData(nameof(GetKeyFileData))]
        public async Task GenerateEncryptedFile(string key, string path)
        {
            // Arrange
            // https://github.com/jbogard/MediatR/issues/526#issue-645312126
            IRequestHandler<EncryptFiles> sut = new EncryptFilesHandler(_console, _loggerMock.Object);
            var inputFile = new FileInfo(Path.Combine(_tempDirectory.FullName, path));
            var encryptedFile = new FileInfo($"{inputFile.FullName}.aes");

            // Act
            await sut.Handle(new EncryptFiles
            {
                Source = inputFile,
                Target = null,
                Key = key,
                EncryptToBase64 = true,
                ForceOverwrite = true
            }, default);

            encryptedFile.Refresh();

            // Assert
            Assert.True(encryptedFile.Exists, $"Could not find expected file '{encryptedFile.FullName}'.");
            var content = await File.ReadAllTextAsync(encryptedFile.FullName);
            Assert.True(IsBase64String(content), "File content is not a Base64-encoded string.");
        }

        [IntegrationTest]
        [Theory]
        [MemberData(nameof(GetKeys), 1)]
        public async Task HandlerEncryptsDirectory(string key)
        {
            // Arrange
            // https://github.com/jbogard/MediatR/issues/526#issue-645312126
            IRequestHandler<EncryptFiles> handler = new EncryptFilesHandler(_console, _loggerMock.Object);

            // Act
            await handler.Handle(new EncryptFiles
            {
                Source = _tempDirectory,
                Target = _targetDirectory,
                Key = key,
                EncryptToBase64 = true,
                ForceOverwrite = true
            }, default);

            _targetDirectory.Refresh();

            // Assert
            Assert.NotEmpty(_targetDirectory.GetFiles());
        }
    }
}