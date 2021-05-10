namespace Kryptera.Tools.Tests
{
    using System;
    using System.CommandLine.IO;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using Commands;
    using MediatR;
    using Xunit;
    using Xunit.Categories;

    [UnitTest(nameof(GenerateEncryptionKeyHandler))]
    public class GenerateEncryptionKeyHandlerTests
    {
        [Fact]
        public async Task HandlerReturnsBase64EncodedKey()
        {
            var console = new TestConsole();
            // https://github.com/jbogard/MediatR/issues/526#issue-645312126
            IRequestHandler<GenerateEncryptionKey> handler = new GenerateEncryptionKeyHandler(console);
            await handler.Handle(new GenerateEncryptionKey(), default);
            Assert.True(IsBase64String(console.Out.ToString()));
        }

        [ExcludeFromCodeCoverage]
        private static bool IsBase64String(string value)
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
    }
}