using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Sinks;
using Serilog.Sinks.MQConsole.Output;
using Serilog.Sinks.MQConsole.Themes;

namespace Serilog
{
    public static class MQConsoleLoggerConfigurationExtensions
    {
        internal static readonly object DefaultSyncRoot = new object();
        internal const string DefaultConsoleOutputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{Exception}";

        public static LoggerConfiguration MQConsole(
            this LoggerSinkConfiguration sinkConfiguration,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            string outputTemplate = DefaultConsoleOutputTemplate,
            IFormatProvider? formatProvider = null,
            LoggingLevelSwitch? levelSwitch = null,
            LogEventLevel? standardErrorFromLevel = null,
            ConsoleTheme? theme = null,
            bool applyThemeToRedirectedOutput = false,
            object? syncRoot = null)
        {
            if (sinkConfiguration is null) throw new ArgumentNullException(nameof(sinkConfiguration));
            if (outputTemplate is null) throw new ArgumentNullException(nameof(outputTemplate));

            // see https://no-color.org/
            var appliedTheme = theme ?? MQConsoleThemes.Colored;

            syncRoot ??= DefaultSyncRoot;

            var formatter = new OutputTemplateRenderer(appliedTheme, outputTemplate, formatProvider);
            return sinkConfiguration.Sink(new MQConsoleSink(appliedTheme, formatter, standardErrorFromLevel, syncRoot), restrictedToMinimumLevel, levelSwitch);
        }
        public static LoggerConfiguration Console(
            this LoggerSinkConfiguration sinkConfiguration,
            ITextFormatter formatter,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            LoggingLevelSwitch? levelSwitch = null,
            LogEventLevel? standardErrorFromLevel = null,
            object? syncRoot = null)
        {
            if (sinkConfiguration is null) throw new ArgumentNullException(nameof(sinkConfiguration));
            if (formatter is null) throw new ArgumentNullException(nameof(formatter));

            syncRoot ??= DefaultSyncRoot;

            return sinkConfiguration.Sink(new MQConsoleSink(ConsoleTheme.None, formatter, standardErrorFromLevel, syncRoot), restrictedToMinimumLevel, levelSwitch);
        }
    }
}
