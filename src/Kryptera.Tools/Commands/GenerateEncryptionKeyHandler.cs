namespace Kryptera.Tools.Commands
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using CryptHash.Net.Util;
    using MediatR;

    public class GenerateEncryptionKeyHandler : AsyncRequestHandler<GenerateEncryptionKey>
    {
        protected override Task Handle(GenerateEncryptionKey request, CancellationToken cancellationToken)
        {
            var key = CommonMethods.Generate256BitKey();
            Console.WriteLine(Convert.ToBase64String(key));
            return Task.CompletedTask;
        }
    }
}