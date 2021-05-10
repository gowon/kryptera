namespace Kryptera.Tools.CommandLine
{
    using Microsoft.Extensions.Logging;

    public class KrypteraOptions
    {
        public LogLevel Verbosity { get; set; } = LogLevel.Warning;
        public string EncryptionKey { get; set; }
    }
}