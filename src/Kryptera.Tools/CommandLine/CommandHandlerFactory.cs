namespace Kryptera.Tools.CommandLine
{
    using System.CommandLine.Invocation;
    using System.CommandLine.Parsing;
    using Commands;
    using MediatR;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    public static class CommandHandlerFactory
    {
        public static ICommandHandler CreateFor<TRequest>() where TRequest : ICommandLineRequest, new()
        {
            return CommandHandler.Create(async (IHost host, ParseResult parseResult) =>
            {
                var mediator = host.Services.GetRequiredService<IMediator>();

                var request = new TRequest();
                request.Map(parseResult);

                // ReSharper disable once SuspiciousTypeConversion.Global
                if (request is IRequest<int> returnRequest)
                {
                    return await mediator.Send(returnRequest);
                }

                request.Map(parseResult);
                await mediator.Send(request);
                return 0;
            });
        }
    }
}