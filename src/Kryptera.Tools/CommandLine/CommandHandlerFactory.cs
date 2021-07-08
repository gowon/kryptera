namespace Kryptera.Tools.CommandLine
{
    using System.CommandLine.Invocation;
    using System.CommandLine.Parsing;
    using System.Threading;
    using Commands;
    using MediatR;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    public static class CommandHandlerFactory
    {
        public static ICommandHandler CreateFor<TRequest>() where TRequest : ICommandLineRequest, new()
        {
            return CommandHandler.Create(
                async (IHost host, ParseResult parseResult, CancellationToken cancellationToken) =>
                {
                    var mediator = host.Services.GetRequiredService<IMediator>();
                    var request = new TRequest();
                    request.Map(parseResult);
                    await mediator.Send(request, cancellationToken);
                });
        }
    }
}