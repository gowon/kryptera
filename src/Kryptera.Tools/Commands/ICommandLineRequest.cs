namespace Kryptera.Tools.Commands
{
    using System.CommandLine.Parsing;
    using MediatR;

    public interface ICommandLineRequest : IRequest
    {
        void Map(ParseResult parseResult);
    }
}