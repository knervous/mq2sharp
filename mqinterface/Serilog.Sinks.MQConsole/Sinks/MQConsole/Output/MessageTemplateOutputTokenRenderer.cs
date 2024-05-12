using Serilog.Events;
using Serilog.Parsing;
using Serilog.Sinks.MQConsole.Formatting;
using Serilog.Sinks.MQConsole.Rendering;
using Serilog.Sinks.MQConsole.Themes;

namespace Serilog.Sinks.MQConsole.Output
{
    class MessageTemplateOutputTokenRenderer : OutputTemplateTokenRenderer
    {
        readonly ThemedMessageTemplateRenderer _renderer;

        public MessageTemplateOutputTokenRenderer(ConsoleTheme theme, PropertyToken token, IFormatProvider? formatProvider)
        {
            bool isLiteral = false;
            if (token.Format != null)
            {
                for (var i = 0; i < token.Format.Length; ++i)
                {
                    if (token.Format[i] == 'l')
                        isLiteral = true;
                }
            }

            var valueFormatter = new ThemedDisplayValueFormatter(theme, formatProvider);

            _renderer = new ThemedMessageTemplateRenderer(theme, valueFormatter, isLiteral);
        }

        public override void Render(LogEvent logEvent, TextWriter output)
        {
            _renderer.Render(logEvent.MessageTemplate, logEvent.Properties, output);
        }
    }
}