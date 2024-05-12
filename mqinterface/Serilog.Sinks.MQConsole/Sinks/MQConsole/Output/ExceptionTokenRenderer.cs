using Serilog.Events;
using Serilog.Parsing;
using Serilog.Sinks.MQConsole.Themes;

namespace Serilog.Sinks.MQConsole.Output
{
    class ExceptionTokenRenderer : OutputTemplateTokenRenderer
    {
        const string StackFrameLinePrefix = "   ";

        readonly ConsoleTheme _theme;

        public ExceptionTokenRenderer(ConsoleTheme theme)
        {
            _theme = theme;
        }

        public override void Render(LogEvent logEvent, TextWriter output)
        {
            if (logEvent.Exception is null)
                return;

            var lines = new StringReader(logEvent.Exception.ToString());
            string? nextLine;
            while ((nextLine = lines.ReadLine()) != null)
            {
                var style = nextLine.StartsWith(StackFrameLinePrefix) ? ConsoleThemeStyle.SecondaryText : ConsoleThemeStyle.Text;
                var _ = 0;
                using (_theme.Apply(output, style, ref _))
                    output.Write(nextLine);
                output.WriteLine();
            }
        }
    }
}