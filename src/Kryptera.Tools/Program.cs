namespace Kryptera.Tools
{
    using System;
    using System.CommandLine.Builder;
    using System.CommandLine.Hosting;
    using System.CommandLine.Parsing;
    using System.Diagnostics;
    using System.IO;
    using System.Threading.Tasks;
    using CommandLine;
    using Extensions;
    using MediatR;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Serilog;
    using Serilog.Core;
    using Serilog.Events;

    internal class Program
    {
        public static LoggingLevelSwitch LevelSwitch = new(LogEventLevel.Warning);

        public static async Task<int> Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(LevelSwitch)
                .WriteTo.Debug()
                .WriteTo.Console()
                .CreateLogger();

            try
            {
                var parser = new CommandLineBuilder(new KrypteraRootCommand())
                    .UseHost(CreateHostBuilder)
                    .UseVerbositySwitch(LevelSwitch)
                    .UseDefaults()
                    .UseHelpBuilder(context => new KrypteraHelpBuilder(context.Console))
                    .UseExceptionHandler((exception, context) =>
                    {
                        Log.Fatal(exception, "Unhandled exception occurred.");
                        context.ExitCode = 1;
                    })
                    .Build();

                return await parser.InvokeAsync(args);
            }
            catch (Exception exception)
            {
                Log.Fatal(exception, "Application terminated unexpectedly.");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    // https://www.hanselman.com/blog/how-do-i-find-which-directory-my-net-core-console-application-was-started-in-or-is-running-from
                    // https://stackoverflow.com/a/97491/7644876
                    var basePath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule!.FileName);

                    config ??= new ConfigurationBuilder();

                    config
                        //.SetBasePath(basePath)
                        .AddJsonFile("kryptera.json", true, false)
                        .AddEnvironmentVariables()
                        .AddCommandLine(args);
                })
                .ConfigureLogging((context, builder) =>
                {
                    builder.ClearProviders();
                    builder.AddSerilog();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<IConfigureOptions<KrypteraOptions>,
                        BindConfigurationOptions<KrypteraOptions>>();

                    services.AddSingleton(
                        provider => provider.GetRequiredService<IOptions<KrypteraOptions>>().Value);

                    services.AddSingleton(LevelSwitch);
                    services.AddMediatR(typeof(KrypteraRootCommand).Assembly);
                });
        }
    }
}