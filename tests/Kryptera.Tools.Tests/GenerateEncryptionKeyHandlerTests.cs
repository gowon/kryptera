namespace Kryptera.Tools.Tests
{
    using System.CommandLine.IO;
    using System.Threading.Tasks;
    using Commands;
    using MediatR;
    using Xunit;
    using Xunit.Categories;
    using static Helpers.Utilities;

    public class GenerateEncryptionKeyHandlerTests
    {
        [UnitTest]
        [Fact]
        public async Task GenerateBase64EncodedKey()
        {
            // Arrange
            var console = new TestConsole();
            // https://github.com/jbogard/MediatR/issues/526#issue-645312126
            IRequestHandler<GenerateEncryptionKey> sut = new GenerateEncryptionKeyHandler(console);

            // Act
            await sut.Handle(new GenerateEncryptionKey(), default);

            // Assert
            Assert.True(IsBase64String(console.Out.ToString()), "File content is not a Base64-encoded string.");
        }
    }
}