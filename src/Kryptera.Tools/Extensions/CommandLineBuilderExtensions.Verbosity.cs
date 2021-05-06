namespace Kryptera.Tools.Extensions
{
    using System;
    using System.CommandLine;
    using System.CommandLine.Builder;
    using System.CommandLine.Hosting;
    using System.CommandLine.Parsing;
    using CommandLine;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Serilog.Core;
    using Serilog.Events;

    public static class CommandLineBuilderExtensions
    {
        public static CommandLineBuilder UseVerbositySwitch(
            this CommandLineBuilder builder, LoggingLevelSwitch loggingLevelSwitch)
        {
            loggingLevelSwitch = loggingLevelSwitch ?? throw new ArgumentNullException(nameof(loggingLevelSwitch));

            if (builder.Command.Children.GetByAlias("--verbosity") != null)
            {
                return builder;
            }

            var versionOption = new Option<LogLevel>(new[] {"-v", "/v", "--verbosity"},
                "Set output verbosity");

            builder.AddGlobalOption(versionOption);

            builder.UseMiddleware(async (context, next) =>
            {
                var result = context.ParseResult.FindResultFor(versionOption);
                var host = context.GetHost();
                var options = host.Services.GetRequiredService<IOptions<KrypteraOptions>>();
                var verbosity = result?.GetValueOrDefault<LogLevel>() ?? options.Value.Verbosity;

                LogEventLevel minimumLevel;
                switch (verbosity)
                {
                    case LogLevel.Trace:
                        minimumLevel = LogEventLevel.Verbose;
                        break;
                    case LogLevel.Debug:
                        minimumLevel = LogEventLevel.Debug;
                        break;
                    case LogLevel.Information:
                        minimumLevel = LogEventLevel.Information;
                        break;
                    case LogLevel.Warning:
                        minimumLevel = LogEventLevel.Warning;
                        break;
                    case LogLevel.Error:
                        minimumLevel = LogEventLevel.Error;
                        break;
                    case LogLevel.Critical:
                    case LogLevel.None:
                        minimumLevel = LogEventLevel.Fatal;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(verbosity));
                }

                loggingLevelSwitch.MinimumLevel = minimumLevel;
                await next(context);
            });

            return builder;
        }
    }
}