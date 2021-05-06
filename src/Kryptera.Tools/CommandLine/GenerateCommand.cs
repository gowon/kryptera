namespace Kryptera.Tools.CommandLine
{
    using System.CommandLine;
    using Commands;

    public class GenerateCommand : Command
    {
        public static string CommandName = "generate";

        public GenerateCommand() : base(CommandName, "Generate a new AES-256 key")
        {
            Initialize();
        }

        protected void Initialize()
        {
            // handler
            Handler = CommandHandlerFactory.CreateFor<GenerateEncryptionKey>();
        }
    }
}