using System.Globalization;
using Serilog.Events;
using Serilog.Parsing;
using Serilog.Sinks.MQConsole.Rendering;
using Serilog.Sinks.MQConsole.Themes;

namespace Serilog.Sinks.MQConsole.Output
{
    class TimestampTokenRenderer : OutputTemplateTokenRenderer
    {
        readonly ConsoleTheme _theme;
        readonly PropertyToken _token;
        readonly IFormatProvider? _formatProvider;

        public TimestampTokenRenderer(ConsoleTheme theme, PropertyToken token, IFormatProvider? formatProvider)
        {
            _theme = theme;
            _token = token;
            _formatProvider = formatProvider;
        }

        public override void Render(LogEvent logEvent, TextWriter output)
        {
            var sv = new DateTimeOffsetValue(logEvent.Timestamp);

            var _ = 0;
            using (_theme.Apply(output, ConsoleThemeStyle.SecondaryText, ref _))
            {
                if (_token.Alignment is null)
                {
                    sv.Render(output, _token.Format, _formatProvider);
                }
                else
                {
                    var buffer = new StringWriter();
                    sv.Render(buffer, _token.Format, _formatProvider);
                    var str = buffer.ToString();
                    Padding.Apply(output, str, _token.Alignment);
                }
            }
        }

        readonly struct DateTimeOffsetValue
        {
            public DateTimeOffsetValue(DateTimeOffset value)
            {
                Value = value;
            }

            public DateTimeOffset Value { get; }

            public void Render(TextWriter output, string? format = null, IFormatProvider? formatProvider = null)
            {
                var custom = (ICustomFormatter?)formatProvider?.GetFormat(typeof(ICustomFormatter));
                if (custom != null)
                {
                    output.Write(custom.Format(format, Value, formatProvider));
                    return;
                }

#if FEATURE_SPAN
                Span<char> buffer = stackalloc char[32];
                if (Value.TryFormat(buffer, out int written, format, formatProvider ?? CultureInfo.InvariantCulture))
                    output.Write(buffer.Slice(0, written));
                else
                    output.Write(Value.ToString(format, formatProvider ?? CultureInfo.InvariantCulture));
#else
                output.Write(Value.ToString(format, formatProvider ?? CultureInfo.InvariantCulture));
#endif
            }
        }
    }
}
