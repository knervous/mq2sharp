using Serilog.Events;
using Serilog.Parsing;
using Serilog.Sinks.MQConsole.Rendering;

namespace Serilog.Sinks.MQConsole.Output
{
    class NewLineTokenRenderer : OutputTemplateTokenRenderer
    {
        readonly Alignment? _alignment;

        public NewLineTokenRenderer(Alignment? alignment)
        {
            _alignment = alignment;
        }

        public override void Render(LogEvent logEvent, TextWriter output)
        {
            if (_alignment.HasValue)
                Padding.Apply(output, Environment.NewLine, _alignment.Value.Widen(Environment.NewLine.Length));
            else
                output.WriteLine();
        }
    }
}
