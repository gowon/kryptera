namespace Kryptera.Tools.CommandLine
{
    using System.CommandLine;
    using System.CommandLine.Invocation;
    using Extensions;

    public class KrypteraRootCommand : RootCommand
    {
        public static string AssemblyName => Constants.ToolCommandName;

        public KrypteraRootCommand()
        {
            Initialize();
        }

        protected void Initialize()
        {
            Name = AssemblyName;

            AddCommand(new EncryptCommand());
            AddCommand(new DecryptCommand());
            AddCommand(new GenerateCommand());

            Handler = CommandHandler.Create<IConsole>(console =>
            {
                new KrypteraHelpBuilder(console).Write(this);
            });
        }
    }
}