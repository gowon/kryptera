namespace Kryptera.Tools.Commands
{
    using System.CommandLine.Parsing;
    using System.IO;

    public class EncryptFiles : ICommandLineRequest
    {
        public string Key { get; set; }
        public FileSystemInfo Source { get; set; }
        public FileSystemInfo Target { get; set; }
        public bool EncryptToBase64 { get; set; }
        public bool ForceOverwrite { get; set; }

        public void Map(ParseResult parseResult)
        {
            Source = parseResult.ValueForArgument<FileSystemInfo>("source");
            Key = parseResult.ValueForOption<string>("--key");
            Target = parseResult.ValueForOption<FileSystemInfo>("--output");
            EncryptToBase64 = parseResult.ValueForOption<bool>("--base64");
            ForceOverwrite = parseResult.ValueForOption<bool>("--force");
        }
    }
}