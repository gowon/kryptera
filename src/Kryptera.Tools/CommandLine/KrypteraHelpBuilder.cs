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
            var color = Color.FromArgb(0, 119, 102); // teal
            var asciiArtStrings = new[]
            {
                "",
                $"                                                                 {"@w".Pastel(color)}",
                $"                                                                             {"\"".Pastel(color)}"
            };

            if (command.Name.Equals(KrypteraRootCommand.AssemblyName, StringComparison.OrdinalIgnoreCase))
            {
                //foreach (var line in asciiArtStrings)
                //{
                //    System.Console.WriteLine(line);
                //}

                System.Console.WriteLine($"Kryptera {AssemblyVersion.Value.Pastel(Color.DarkOrange)}\n");
            }

            base.Write(command);
        }
    }
}