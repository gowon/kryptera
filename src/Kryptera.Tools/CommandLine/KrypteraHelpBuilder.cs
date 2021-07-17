namespace Kryptera.Tools.CommandLine
{
    using System;
    using System.CommandLine;
    using System.CommandLine.Help;
    using System.Drawing;
    using System.Reflection;
    using Pastel;

    public class KrypteraHelpBuilder : HelpBuilder
    {
        public static readonly Lazy<string> AssemblyVersion =
            new(() =>
            {
                var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
                var assemblyVersionAttribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
                return assemblyVersionAttribute is null
                    ? assembly.GetName().Version?.ToString()
                    : assemblyVersionAttribute.InformationalVersion;
            });

        public KrypteraHelpBuilder(IConsole console) : base(console)
        {
        }

        public override void Write(ICommand command)
        {
            var color = Color.DodgerBlue;
            if (command.Name.Equals(KrypteraRootCommand.AssemblyName, StringComparison.OrdinalIgnoreCase))
            {
                var asciiArtStrings = new[]
                {
                    "",
                    $"  {"    ████    ".Pastel(color)}",
                    $"  {"  ██    ██  ".Pastel(color)}",
                    $"  {"████████████".Pastel(color)}",
                    $"  {"██░░░░░░░░██".Pastel(color)}",
                    $"  {"██░░████░░██".Pastel(color)} Kryptera Encryption Command-line Tools {AssemblyVersion.Value.Pastel(color)}",
                    $"  {"██░░░░░░░░██".Pastel(color)}",
                    $"  {"████████████".Pastel(color)}",
                    ""
                };

                foreach (var line in asciiArtStrings)
                {
                    System.Console.WriteLine(line);
                }
            }
            else
            {
                System.Console.WriteLine($"Kryptera Encryption Command-line Tools {AssemblyVersion.Value.Pastel(color)}\n");
            }

            base.Write(command);
        }
    }
}