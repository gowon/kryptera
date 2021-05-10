namespace Kryptera.Tools.Tests
{
    using System;
    using System.CommandLine.IO;
    using System.IO;
    using System.Threading.Tasks;
    using Commands;
    using CryptHash.Net.Util;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using Moq;
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

            // https://github.com/jbogard/MediatR/issues/526#issue-645312126
            IRequestHandler<EncryptFiles> handler = new EncryptFilesHandler(console, loggerMock.Object);
            await handler.Handle(new EncryptFiles
            {
                Source = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "Samples")),
                Target = null,
                Key = key,
                EncryptToBase64 = true,
                ForceOverwrite = true
            }, default);

            Assert.NotNull(console.Out.ToString());
        }
    }
}