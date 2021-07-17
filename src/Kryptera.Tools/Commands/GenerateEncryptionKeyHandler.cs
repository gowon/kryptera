namespace Kryptera.Tools.Commands
{
    using System;
    using System.CommandLine;
    using System.CommandLine.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;

    public class GenerateEncryptionKeyHandler : AsyncRequestHandler<GenerateEncryptionKey>
    {
        private readonly IConsole _console;

        public GenerateEncryptionKeyHandler(IConsole console)
        {
            _console = console ?? throw new NullReferenceException(nameof(console));
        }

        protected override Task Handle(GenerateEncryptionKey request, CancellationToken cancellationToken)
        {
            _console.Out.WriteLine(Kryptera.GenerateKey());
            return Task.CompletedTask;
        }
    }
}