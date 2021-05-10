namespace Kryptera.Tools.CommandLine
{
    using System.CommandLine;
    using System.IO;
    using Commands;

    public class EncryptCommand : Command
    {
        public static string CommandName = "encrypt";

        public EncryptCommand() : base(CommandName, "Encrypt a file or directory using AES-256-GCM")
        {
            Initialize();
        }

        protected void Initialize()
        {
            // arguments
            AddArgument(new Argument<FileSystemInfo>("source",
                "Specify the source file or directory")
            {
                Arity = ArgumentArity.ExactlyOne
            });

            // options
            AddOption(new Option<string>(new[] {"-k", "/k", "--key"},
                "Specify encryption key"));

            AddOption(new Option<FileSystemInfo>(new[] {"-o", "/o", "--output"},
                "Specify output file or directory"));

            AddOption(new Option<bool>(new[] {"-t", "/t", "--base64"},
                "Encrypt as base64 encoded text"));

            AddOption(new Option<bool>(new[] {"-f", "/f", "--force"},
                "Overwrite existing files"));

            // handler
            Handler = CommandHandlerFactory.CreateFor<EncryptFiles>();
        }
    }
}