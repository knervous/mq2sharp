using Serilog.Events;
using Serilog.Parsing;
using Serilog.Sinks.MQConsole.Rendering;
using Serilog.Sinks.MQConsole.Themes;

namespace Serilog.Sinks.MQConsole.Output
{
    class LevelTokenRenderer : OutputTemplateTokenRenderer
    {
        readonly ConsoleTheme _theme;
        readonly PropertyToken _levelToken;

        static readonly Dictionary<LogEventLevel, ConsoleThemeStyle> Levels = new Dictionary<LogEventLevel, ConsoleThemeStyle>
        {
            { LogEventLevel.Verbose, ConsoleThemeStyle.LevelVerbose },
            { LogEventLevel.Debug, ConsoleThemeStyle.LevelDebug },
            { LogEventLevel.Information, ConsoleThemeStyle.LevelInformation },
            { LogEventLevel.Warning, ConsoleThemeStyle.LevelWarning },
            { LogEventLevel.Error, ConsoleThemeStyle.LevelError },
            { LogEventLevel.Fatal, ConsoleThemeStyle.LevelFatal },
        };

        public LevelTokenRenderer(ConsoleTheme theme, PropertyToken levelToken)
        {
            _theme = theme;
            _levelToken = levelToken;
        }

        public override void Render(LogEvent logEvent, TextWriter output)
        {
            var moniker = LevelOutputFormat.GetLevelMoniker(logEvent.Level, _levelToken.Format);
            if (!Levels.TryGetValue(logEvent.Level, out var levelStyle))
                levelStyle = ConsoleThemeStyle.Invalid;

            var _ = 0;
            using (_theme.Apply(output, levelStyle, ref _))
                Padding.Apply(output, moniker, _levelToken.Alignment);
        }
    }
}