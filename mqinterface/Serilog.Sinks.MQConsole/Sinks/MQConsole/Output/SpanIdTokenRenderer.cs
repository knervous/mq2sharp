using Serilog.Events;
using Serilog.Parsing;
using Serilog.Sinks.MQConsole.Rendering;
using Serilog.Sinks.MQConsole.Themes;

namespace Serilog.Sinks.MQConsole.Output;

class SpanIdTokenRenderer : OutputTemplateTokenRenderer
{
    readonly ConsoleTheme _theme;
    readonly Alignment? _alignment;

    public SpanIdTokenRenderer(ConsoleTheme theme, PropertyToken spanIdToken)
    {
        _theme = theme;
        _alignment = spanIdToken.Alignment;
    }

    public override void Render(LogEvent logEvent, TextWriter output)
    {
        if (logEvent.SpanId is not { } spanId)
            return;

        var _ = 0;
        using (_theme.Apply(output, ConsoleThemeStyle.Text, ref _))
        {
            if (_alignment is { } alignment)
                Padding.Apply(output, spanId.ToString(), alignment);
            else
                output.Write(spanId);
        }
    }
}