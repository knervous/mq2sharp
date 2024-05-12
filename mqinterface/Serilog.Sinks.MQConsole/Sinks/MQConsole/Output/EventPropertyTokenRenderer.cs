using Serilog.Events;
using Serilog.Parsing;
using Serilog.Sinks.MQConsole.Rendering;
using Serilog.Sinks.MQConsole.Themes;

namespace Serilog.Sinks.MQConsole.Output
{
    class EventPropertyTokenRenderer : OutputTemplateTokenRenderer
    {
        readonly ConsoleTheme _theme;
        readonly PropertyToken _token;
        readonly IFormatProvider? _formatProvider;

        public EventPropertyTokenRenderer(ConsoleTheme theme, PropertyToken token, IFormatProvider? formatProvider)
        {
            _theme = theme;
            _token = token;
            _formatProvider = formatProvider;
        }

        public override void Render(LogEvent logEvent, TextWriter output)
        {
            if (!logEvent.Properties.TryGetValue(_token.PropertyName, out var propertyValue))
            {
                Padding.Apply(output, string.Empty, _token.Alignment);
                return;
            }

            var _ = 0;
            using (_theme.Apply(output, ConsoleThemeStyle.SecondaryText, ref _))
            {
                var writer = _token.Alignment.HasValue ? new StringWriter() : output;

                if (propertyValue is ScalarValue sv && sv.Value is string literalString)
                {
                    var cased = Casing.Format(literalString, _token.Format);
                    writer.Write(cased);
                }
                else
                {
                    propertyValue.Render(writer, _token.Format, _formatProvider);
                }

                if (_token.Alignment.HasValue)
                {
                    var str = writer.ToString()!;
                    Padding.Apply(output, str, _token.Alignment);
                }
            }
        }
    }
}