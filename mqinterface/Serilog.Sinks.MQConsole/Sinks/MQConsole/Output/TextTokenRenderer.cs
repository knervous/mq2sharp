using Serilog.Events;
using Serilog.Sinks.MQConsole.Themes;

namespace Serilog.Sinks.MQConsole.Output
{
    class TextTokenRenderer : OutputTemplateTokenRenderer
    {
        readonly ConsoleTheme _theme;
        readonly string _text;

        public TextTokenRenderer(ConsoleTheme theme, string text)
        {
            _theme = theme;
            _text = text;
        }

        public override void Render(LogEvent logEvent, TextWriter output)
        {
            var _ = 0;
            using (_theme.Apply(output, ConsoleThemeStyle.TertiaryText, ref _))
                output.Write(_text);
        }
    }
}