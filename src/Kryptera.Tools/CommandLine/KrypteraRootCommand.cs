namespace Kryptera.Tools.CommandLine
{
    using System.CommandLine;
    using System.CommandLine.Invocation;

    public class KrypteraRootCommand : RootCommand
    {
        public static string AssemblyName => typeof(KrypteraRootCommand).Assembly.GetName().Name!.ToLowerInvariant();

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